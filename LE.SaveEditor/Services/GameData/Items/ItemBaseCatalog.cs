using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace LE.SaveEditor.Services.GameData.Items;

/// <summary>
/// Reads the <c>ItemTypes</c>, <c>Uniques</c>, <c>Properties</c>, <c>Ailments</c> and <c>Icons</c> tables of <c>base.db</c>.
/// </summary>
/// <remarks>
/// Every row keeps its element of maxroll's <c>data.json</c> as JSON. An item type looks like
/// <c>{"baseTypeID":0,"displayName":"Helmet","subItems":[{"subTypeID":1,"name":"Leather Helmet","displayName":"Jewelled Circlet",
/// "levelRequirement":7,"cannotDrop":true,"implicits":[{"property":10,"specialTag":0,"tags":0,"type":0,"implicitValue":5,"implicitMaxValue":5}]}]}</c>.
/// Implicit names are built like the game does: the property name from <c>properties</c>, prefixed with the
/// tags (Minion, Spell, Fire, ...) and the ailment from <c>ailments</c> that <c>specialTag</c> refers to.
/// A unique looks like <c>{"uniqueID":5,"name":"IsadoraRevenge","displayName":"Isadora's Revenge","isSetItem":true,"baseType":0,
/// "subTypes":[5],"overrideLevelRequirement":false,"levelRequirement":0,"hideFromPlayers":false,
/// "mods":[{"value":40,"maxValue":82,"property":7,"specialTag":0,"tags":0,"type":0}]}</c>.
/// Field names are matched case-insensitively, and anything unreadable is skipped.
/// </remarks>
public sealed class ItemBaseCatalog(IIconProvider iconProvider) : IItemBaseCatalog
{
    private readonly object _sync = new();
    private Catalog? _catalog;

    /// <summary>What was read from base.db.</summary>
    /// <param name="Uniques">Unique and set items.</param>
    /// <param name="HasStatNames">The <c>Properties</c> table exists (databases made before it show "Property 10" etc.).</param>
    private sealed record Catalog(IReadOnlyList<ItemBase> Bases, IReadOnlyList<ItemBase> Uniques, bool HasStatNames);

    public bool IsAvailable => GetAll().Count > 0;

    public bool IsOutdated
    {
        get
        {
            lock (_sync)
                return (_catalog ??= Load()) is { Bases.Count: > 0, HasStatNames: false };
        }
    }

    public IReadOnlyList<ItemBase> GetBases(EquipmentSlot slot) =>
        GetAll()
            .Where(b => b.BaseTypeId >= 0
                ? slot.AcceptsBaseType(b.BaseTypeId)
                : slot.AcceptsTypeName(Normalize(b.TypeName)))
            // Bases that still drop first, then by item type, level and name.
            .OrderBy(b => b.CannotDrop)
            .ThenBy(b => b.BaseTypeId)
            .ThenBy(b => b.LevelRequirement)
            .ThenBy(b => b.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

    public IReadOnlyList<ItemBase> GetUniques(EquipmentSlot slot, bool sets)
    {
        Catalog catalog;
        lock (_sync)
            catalog = _catalog ??= Load();

        return catalog.Uniques
            .Where(u => u.IsSet == sets && slot.AcceptsBaseType(u.BaseTypeId))
            .OrderBy(u => u.BaseTypeId)
            .ThenBy(u => u.LevelRequirement)
            .ThenBy(u => u.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    public void ClearCache()
    {
        lock (_sync)
            _catalog = null;
        iconProvider.ClearCache();
    }

    private IReadOnlyList<ItemBase> GetAll()
    {
        lock (_sync)
            return (_catalog ??= Load()).Bases;
    }

    private static Catalog Load()
    {
        if (!File.Exists(GameDataPaths.DatabaseFile))
            return new Catalog([], [], false);

        try
        {
            // Read-only and unpooled: the generator must be able to replace base.db while the editor runs.
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = GameDataPaths.DatabaseFile,
                Mode = SqliteOpenMode.ReadOnly,
                Pooling = false
            }.ToString();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var properties = ReadProperties(connection);
            var names = new ImplicitNames(properties, ReadAilments(connection));
            var icons = ReadIcons(connection);

            var result = new List<ItemBase>();
            foreach (var (key, name, json) in ReadRows(connection, "SELECT Key, Name, Json FROM ItemTypes"))
            {
                using var document = TryParse(json);
                if (document?.RootElement is { ValueKind: JsonValueKind.Object } itemType)
                    result.AddRange(ParseItemType(key, name, itemType, names, icons));
            }

            var uniques = ReadUniques(connection, result, names, icons);
            return new Catalog(result, uniques, properties.Count > 0);
        }
        catch (Exception ex) when (ex is SqliteException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            // A missing or broken database means an empty builder, never a crash.
            return new Catalog([], [], false);
        }
    }

    // ===== Item types =====

    private static List<ItemBase> ParseItemType(string key, string? rowName, JsonElement itemType, ImplicitNames names, IconIndex icons)
    {
        var result = new List<ItemBase>();

        var baseTypeId = GetInt(itemType, "baseTypeID")
                         ?? (int.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : -1);
        var typeName = GetString(itemType, "displayName") ?? GetString(itemType, "BaseTypeName") ?? rowName ?? key;

        if (Find(itemType, "subItems") is not { ValueKind: JsonValueKind.Array } subItems)
            return result;

        var index = 0;
        foreach (var subItem in subItems.EnumerateArray())
        {
            var position = index++;
            if (subItem.ValueKind != JsonValueKind.Object)
                continue;

            // "name" is the internal name (and the icon file name); "displayName" is what the game shows, when set.
            var internalName = GetString(subItem, "name");
            var name = GetString(subItem, "displayName") ?? internalName;
            if (name is null)
                continue;

            result.Add(new ItemBase(
                baseTypeId,
                GetInt(subItem, "subTypeID") ?? position,
                name,
                typeName,
                GetInt(subItem, "levelRequirement") ?? 0,
                GetInt(subItem, "classRequirement") ?? 0,
                GetBool(subItem, "cannotDrop") ?? false,
                icons.Find(typeName, internalName, name),
                ParseImplicits(subItem, names)));
        }

        return result;
    }

    private static List<ItemImplicit> ParseImplicits(JsonElement subItem, ImplicitNames names) =>
        ParseModifiers(subItem, "implicits", "implicitValue", "implicitMaxValue", names);

    // ===== Uniques / set items =====

    private static List<ItemBase> ReadUniques(SqliteConnection connection, IReadOnlyList<ItemBase> bases, ImplicitNames names, IconIndex icons)
    {
        var result = new List<ItemBase>();
        var byId = new Dictionary<(int, int), ItemBase>();
        foreach (var itemBase in bases)
            byId.TryAdd((itemBase.BaseTypeId, itemBase.SubTypeId), itemBase);

        foreach (var (_, rowName, json) in ReadRowsOrEmpty(connection, "SELECT Key, Name, Json FROM Uniques"))
        {
            using var document = TryParse(json);
            if (document?.RootElement is not { ValueKind: JsonValueKind.Object } unique
                || GetBool(unique, "hideFromPlayers") == true
                || GetInt(unique, "uniqueID") is not { } uniqueId
                || GetInt(unique, "baseType") is not { } baseTypeId)
                continue;

            // "name" is internal ("IsadoraRevenge"); "displayName" is what the game shows, when set.
            var internalName = GetString(unique, "name");
            var name = GetString(unique, "displayName") ?? internalName ?? rowName;
            if (name is null)
                continue;

            var subTypeId = Find(unique, "subTypes") is { ValueKind: JsonValueKind.Array } subTypes
                            && subTypes.EnumerateArray().FirstOrDefault() is { ValueKind: JsonValueKind.Number } first
                            && first.TryGetInt32(out var sub)
                ? sub
                : 0;
            byId.TryGetValue((baseTypeId, subTypeId), out var itemBase);

            // Most uniques override the level of their base; the rest inherit it.
            var level = GetBool(unique, "overrideLevelRequirement") == true
                ? GetInt(unique, "levelRequirement") ?? 0
                : itemBase?.LevelRequirement ?? GetInt(unique, "levelRequirement") ?? 0;

            var effects = new List<string>();
            var mods = ParseUniqueMods(unique, names, effects);

            result.Add(new ItemBase(
                baseTypeId,
                subTypeId,
                name,
                itemBase?.TypeName ?? string.Empty,
                level,
                itemBase?.ClassRequirement ?? 0,
                CannotDrop: GetBool(unique, "canDropRandomly") == false,
                icons.Find("uniques", name, internalName) ?? itemBase?.IconPath,
                itemBase?.Implicits ?? [],
                uniqueId,
                GetBool(unique, "isSetItem") ?? false,
                itemBase?.Name,
                mods,
                effects));
        }

        return result;
    }

    /// <summary>
    /// <c>[100,150,0]</c> in a unique tooltip: the shown range of the modifier with rollID 0;
    /// <c>[1,c]</c> / <c>[1,c,5]</c>: a per-level bonus, shown as its first number.
    /// </summary>
    private static readonly Regex RollPlaceholder = new(@"\[(-?[\d.]+),(-?[\d.]+),(\d+)\]", RegexOptions.Compiled);
    private static readonly Regex LevelPlaceholder = new(@"\[(-?[\d.]+),c(?:,\d+)?\]", RegexOptions.Compiled);

    /// <summary>
    /// Unique modifiers as the game's tooltip shows them: modifiers referenced by a tooltip line take that line as
    /// their label (the rolled number follows the slider); other shown modifiers use their property name; tooltip
    /// lines without a roll become <paramref name="effects"/>.
    /// </summary>
    private static List<ItemImplicit> ParseUniqueMods(JsonElement unique, ImplicitNames names, List<string> effects)
    {
        var result = new List<ItemImplicit>();

        // rollID → tooltip line that describes it.
        var described = new Dictionary<int, (string Template, double From, double To)>();
        if (Find(unique, "tooltipDescriptions") is { ValueKind: JsonValueKind.Array } descriptions)
        {
            foreach (var entry in descriptions.EnumerateArray())
            {
                if (entry.ValueKind != JsonValueKind.Object || GetString(entry, "description") is not { } text)
                    continue;

                text = LevelPlaceholder.Replace(text, m => m.Groups[1].Value);
                var matches = RollPlaceholder.Matches(text);
                if (matches.Count == 0)
                {
                    effects.Add(text);
                    continue;
                }

                // The first placeholder follows its slider; any others show their range.
                var first = matches[0];
                var template = RollPlaceholder.Replace(text, m => m.Index == first.Index ? "{value}" : $"{m.Groups[1].Value}–{m.Groups[2].Value}");
                var rollId = int.Parse(first.Groups[3].Value, CultureInfo.InvariantCulture);
                var from = double.Parse(first.Groups[1].Value, CultureInfo.InvariantCulture);
                var to = double.Parse(first.Groups[2].Value, CultureInfo.InvariantCulture);
                if (!described.TryAdd(rollId, (template, from, to)))
                    effects.Add(RollPlaceholder.Replace(text, m => $"{m.Groups[1].Value}–{m.Groups[2].Value}"));
            }
        }

        if (Find(unique, "mods") is not { ValueKind: JsonValueKind.Array } mods)
            return result;

        var usedRolls = new HashSet<int>();
        foreach (var entry in mods.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.Object || ParseModifier(entry, "value", "maxValue", names) is not { } mod)
                continue;

            var rollId = GetInt(entry, "rollID");
            if (rollId is { } id && described.TryGetValue(id, out var line) && usedRolls.Add(id))
            {
                result.Add(mod with { Template = line.Template, TemplateMin = Math.Min(line.From, line.To), TemplateMax = Math.Max(line.From, line.To) });
                continue;
            }

            // Hidden modifiers only exist to drive tooltip lines (handled above).
            if (GetBool(entry, "hideInTooltip") == true)
                continue;

            // Skill-specific properties have no name in the data: keep the slider when it rolls, drop fixed ones.
            if (!names.IsNamed(GetInt(entry, "property") ?? -1, GetInt(entry, "specialTag") ?? 0))
            {
                if (mod.Max - mod.Min > double.Epsilon)
                    result.Add(mod with { Property = "Special modifier" });
                continue;
            }

            result.Add(mod);
        }

        // Tooltip lines whose modifier is missing still describe the item.
        foreach (var (id, line) in described)
        {
            if (!usedRolls.Contains(id))
                effects.Add(line.Template.Replace("{value}", $"{line.From.ToString(CultureInfo.InvariantCulture)}–{line.To.ToString(CultureInfo.InvariantCulture)}"));
        }

        return result;
    }

    // ===== Modifiers (implicits and unique mods) =====

    private static List<ItemImplicit> ParseModifiers(JsonElement owner, string arrayField, string valueField, string maxField, ImplicitNames names)
    {
        var result = new List<ItemImplicit>();
        if (Find(owner, arrayField) is not { ValueKind: JsonValueKind.Array } entries)
            return result;

        foreach (var entry in entries.EnumerateArray())
        {
            if (entry.ValueKind == JsonValueKind.Object && ParseModifier(entry, valueField, maxField, names) is { } modifier)
                result.Add(modifier);
        }

        return result;
    }

    /// <summary>One implicit / unique modifier; a maximum of 0 with a non-zero value means the value is fixed.</summary>
    private static ItemImplicit? ParseModifier(JsonElement entry, string valueField, string maxField, ImplicitNames names)
    {
        if (GetDouble(entry, valueField) is not { } first)
            return null;
        var second = GetDouble(entry, maxField) ?? first;
        if (second == 0 && first != 0)
            second = first;

        var type = GetInt(entry, "type") is { } t && Enum.IsDefined((ImplicitModifierType)t)
            ? (ImplicitModifierType)t
            : ImplicitModifierType.Added;

        var property = GetInt(entry, "property") ?? -1;
        var info = names.GetProperty(property);
        var name = names.Compose(info, property, GetLong(entry, "tags") ?? 0, GetInt(entry, "specialTag") ?? 0);

        // Increased / more are always percentages; added values only for properties shown that way (resistances, ...).
        var isPercent = type != ImplicitModifierType.Added || (info?.AddedAsPercentage ?? false);

        return new ItemImplicit(
            name,
            type,
            Math.Min(first, second),
            Math.Max(first, second),
            isPercent,
            PercentScale: info?.AddedAsTenth == true && type == ImplicitModifierType.Added ? 10 : 100,
            ShowPlus: !(info?.DontDisplayPlus ?? false),
            NameIncludesModifier: info?.NameIncludesModifier ?? false);
    }

    // ===== Implicit names =====

    private sealed record PropertyInfo(string Name, bool AddedAsPercentage, bool AddedAsTenth, bool DontDisplayPlus, bool NameIncludesModifier);

    /// <summary>Builds implicit names like the game: "Minion Spell Fire Damage", "Bleed Chance", "Damage over Time".</summary>
    private sealed class ImplicitNames(Dictionary<int, PropertyInfo> properties, Dictionary<int, string> ailments)
    {
        /// <summary>Tag bits (the game's <c>AT</c> flags) in the order their words go before the property name.</summary>
        private static readonly (long Bit, string Word)[] PrefixTags =
        [
            (8192, "Minion"), (16384, "Totem"),
            (512, "Melee"), (256, "Spell"), (1024, "Throwing"), (2048, "Bow"),
            (1, "Physical"), (2, "Lightning"), (4, "Cold"), (8, "Fire"), (16, "Void"), (32, "Necrotic"), (64, "Poison"), (128, "Elemental")
        ];

        private const long DamageOverTimeTag = 4096;

        /// <summary>Properties that refer to one specific skill and cannot be named from the data.</summary>
        private const int DeprecatedSkillProperty = 58;
        private const int LevelOfSkillsProperty = 88;

        public PropertyInfo? GetProperty(int property) => properties.GetValueOrDefault(property);

        /// <summary>
        /// <c>false</c> for properties without a name and for skill-specific ones (property 58, and "Level of Skills"
        /// with a special tag, whose tags then select a single skill).
        /// </summary>
        public bool IsNamed(int property, int specialTag) =>
            properties.ContainsKey(property)
            && property != DeprecatedSkillProperty
            && !(property == LevelOfSkillsProperty && specialTag != 0);

        public string Compose(PropertyInfo? info, int property, long tags, int specialTag)
        {
            var name = info?.Name is { Length: > 0 } n ? n : $"Property {property}";

            // "Ailment Chance" with special tag 2 → "Bleed Chance"; other properties get the ailment as a prefix.
            if (specialTag != 0 && ailments.TryGetValue(specialTag, out var ailment))
            {
                name = name.Contains("Ailment", StringComparison.OrdinalIgnoreCase)
                    ? name.Replace("Ailment", ailment, StringComparison.OrdinalIgnoreCase)
                    : $"{ailment} {name}";
            }

            var words = PrefixTags.Where(t => (tags & t.Bit) != 0).Select(t => t.Word).ToList();
            if (words.Count > 0)
                name = $"{string.Join(' ', words)} {name}";

            if ((tags & DamageOverTimeTag) != 0)
                name += " over Time";

            return name;
        }
    }

    private static Dictionary<int, PropertyInfo> ReadProperties(SqliteConnection connection)
    {
        var result = new Dictionary<int, PropertyInfo>();

        foreach (var (key, rowName, json) in ReadRowsOrEmpty(connection, "SELECT Key, Name, Json FROM Properties"))
        {
            using var document = TryParse(json);
            if (document?.RootElement is not { ValueKind: JsonValueKind.Object } property)
                continue;

            var id = GetInt(property, "property")
                     ?? (int.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var k) ? (int?)k : null);
            var name = GetString(property, "propertyName") ?? rowName;
            if (id is null || name is null)
                continue;

            result.TryAdd(id.Value, new PropertyInfo(
                name,
                GetBool(property, "displayAddedAsPercentage") ?? false,
                GetBool(property, "displayAddedAsTenthOfValue") ?? false,
                GetBool(property, "dontDisplayPlus") ?? false,
                GetBool(property, "nameIncludesModifier") ?? false));
        }

        return result;
    }

    private static Dictionary<int, string> ReadAilments(SqliteConnection connection)
    {
        var result = new Dictionary<int, string>();

        foreach (var (_, rowName, json) in ReadRowsOrEmpty(connection, "SELECT Key, Name, Json FROM Ailments"))
        {
            using var document = TryParse(json);
            if (document?.RootElement is not { ValueKind: JsonValueKind.Object } ailment)
                continue;

            var name = GetString(ailment, "displayName") ?? GetString(ailment, "instanceName") ?? rowName;
            // Ids repeat in the data; the first entry is the base ailment.
            if (GetInt(ailment, "id") is { } id && name is not null)
                result.TryAdd(id, name);
        }

        return result;
    }

    // ===== Icons =====

    /// <summary>Icon file names, normalized (<c>leather_helmet</c> → <c>leatherhelmet</c>) → icon paths.</summary>
    private sealed class IconIndex
    {
        private readonly Dictionary<string, List<(string Group, string Path)>> _byName = new(StringComparer.Ordinal);

        public void Add(string group, string name, string path)
        {
            var key = Normalize(name);
            if (key.Length == 0)
                return;
            if (!_byName.TryGetValue(key, out var paths))
                _byName[key] = paths = [];
            paths.Add((group, path));
        }

        /// <summary>Icons are named after the internal base name; the shown name is a fallback.</summary>
        public string? Find(string typeName, params string?[] names)
        {
            var typeKey = Normalize(typeName);
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name) || !_byName.TryGetValue(Normalize(name), out var paths))
                    continue;

                // Prefer the folder of the item type (helmets/...) over e.g. uniques/... with the same file name.
                return paths
                    .OrderBy(p => typeKey.Length >= 4 && Normalize(p.Group).Contains(typeKey[..4], StringComparison.Ordinal) ? 0
                        : p.Group.Contains("unique", StringComparison.OrdinalIgnoreCase) ? 2 : 1)
                    .ThenBy(p => p.Path, StringComparer.OrdinalIgnoreCase)
                    .Select(p => p.Path)
                    .First();
            }

            return null;
        }
    }

    private static IconIndex ReadIcons(SqliteConnection connection)
    {
        var index = new IconIndex();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT \"Group\", Name, Path FROM Icons WHERE Path IS NOT NULL";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var group = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                if (!reader.IsDBNull(1))
                    index.Add(group, reader.GetString(1), reader.GetString(2));
            }
        }
        catch (SqliteException)
        {
            // No icons table: bases are shown without images.
        }

        return index;
    }

    // ===== SQLite / JSON helpers =====

    private static IEnumerable<(string Key, string? Name, string Json)> ReadRows(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        using var reader = command.ExecuteReader();
        while (reader.Read())
            yield return (reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetString(1), reader.GetString(2));
    }

    /// <summary>Rows of an optional table: a database generated before the table existed simply has none.</summary>
    private static List<(string Key, string? Name, string Json)> ReadRowsOrEmpty(SqliteConnection connection, string sql)
    {
        try
        {
            return ReadRows(connection, sql).ToList();
        }
        catch (SqliteException)
        {
            return [];
        }
    }

    private static JsonDocument? TryParse(string json)
    {
        try
        {
            return JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 256 });
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>The property named <paramref name="name"/> (case-insensitive), or <c>null</c> when missing or null.</summary>
    private static JsonElement? Find(JsonElement element, string name)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && property.Value.ValueKind != JsonValueKind.Null)
                return property.Value;
        }

        return null;
    }

    private static string? GetString(JsonElement element, string name) =>
        Find(element, name) is { ValueKind: JsonValueKind.String } value && !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()
            : null;

    private static int? GetInt(JsonElement element, string name) => Find(element, name) switch
    {
        { ValueKind: JsonValueKind.Number } n when n.TryGetInt32(out var i) => i,
        { ValueKind: JsonValueKind.String } s when int.TryParse(s.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) => i,
        _ => null
    };

    private static long? GetLong(JsonElement element, string name) => Find(element, name) switch
    {
        { ValueKind: JsonValueKind.Number } n when n.TryGetInt64(out var i) => i,
        _ => null
    };

    private static double? GetDouble(JsonElement element, string name) => Find(element, name) switch
    {
        { ValueKind: JsonValueKind.Number } n => n.GetDouble(),
        { ValueKind: JsonValueKind.String } s when double.TryParse(s.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) => d,
        _ => null
    };

    private static bool? GetBool(JsonElement element, string name) => Find(element, name) switch
    {
        { ValueKind: JsonValueKind.True } => true,
        { ValueKind: JsonValueKind.False } => false,
        _ => null
    };

    private static string Normalize(string value) =>
        new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}

using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace LE.SaveEditor.Services.GameData.Items;

/// <summary>
/// Reads the <c>ItemTypes</c>, <c>Uniques</c>, <c>Affixes</c>, <c>Properties</c>, <c>Ailments</c> and <c>Icons</c> tables of <c>base.db</c>
/// (plus the indexed property lists).
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
    /// <param name="HasStatNames">
    /// The <c>Properties</c>, <c>AbilityProperties</c> and <c>PlayerProperties</c> tables exist (databases made before them
    /// show "Property 10", internal skill affix names or affix names in place of player properties).
    /// </param>
    /// <param name="Affixes">Equipment and idol affixes as the data gives them (not scaled to an item type).</param>
    /// <param name="AffixEffectModifiers">Per item type (<c>baseTypeID</c>) its <c>affixEffectModifier</c>.</param>
    /// <param name="Materials">Runes, glyphs, affix shards and resources (key items).</param>
    private sealed record Catalog(
        IReadOnlyList<ItemBase> Bases,
        IReadOnlyList<ItemBase> Uniques,
        IReadOnlyList<AffixDefinition> Affixes,
        bool HasStatNames,
        IReadOnlyDictionary<int, double> AffixEffectModifiers,
        IReadOnlyList<CraftingMaterial> Materials)
    {
        /// <summary>Affixes scaled to an item type, by affix id and item type, so each is built once.</summary>
        public Dictionary<(int AffixId, int BaseTypeId), AffixDefinition> Scaled { get; } = [];
    }

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

    public IReadOnlyList<AffixDefinition> GetAffixes(int baseTypeId)
    {
        Catalog catalog;
        lock (_sync)
            catalog = _catalog ??= Load();

        lock (_sync)
            return catalog.Affixes.Where(a => a.CanRollOnType(baseTypeId)).Select(a => ScaledFor(catalog, a, baseTypeId)).ToList();
    }

    public ItemBase? FindBase(int baseTypeId, int subTypeId) =>
        GetCatalog().Bases.FirstOrDefault(b => b.BaseTypeId == baseTypeId && b.SubTypeId == subTypeId);

    public ItemBase? FindUnique(int uniqueId) =>
        GetCatalog().Uniques.FirstOrDefault(u => u.UniqueId == uniqueId);

    public AffixDefinition? FindAffix(int affixId, int? baseTypeId = null)
    {
        var catalog = GetCatalog();

        // Equipment and idol affixes are separate lists in the data; prefer the one that can roll on the item.
        var matches = catalog.Affixes.Where(a => a.Id == affixId).ToList();
        var affix = (baseTypeId is { } type ? matches.FirstOrDefault(a => a.CanRollOnType(type)) : null)
                    ?? matches.FirstOrDefault();
        if (affix is null || baseTypeId is not { } itemType)
            return affix;

        lock (_sync)
            return ScaledFor(catalog, affix, itemType);
    }

    public IReadOnlyList<CraftingMaterial> GetCraftingMaterials(CraftingMaterialKind kind) =>
        GetCatalog().Materials
            .Where(m => m.Kind == kind)
            // Materials that still drop first, then by name.
            .OrderBy(m => m.CannotDrop)
            .ThenBy(m => m.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

    public CraftingMaterial? FindCraftingMaterial(CraftingMaterialKind kind, int subTypeId) =>
        GetCatalog().Materials.FirstOrDefault(m => m.Kind == kind && m.SubTypeId == subTypeId);

    /// <summary>
    /// <paramref name="affix"/> with the values it has on item type <paramref name="baseTypeId"/>. The game scales every
    /// tier by <c>(1 + affixEffectModifier of the item type) / (1 + standardAffixEffectModifier of the affix)</c>: the
    /// tiers are written for the item type whose modifier the affix names, e.g. Increased Bow Attack Speed (0) is 75%
    /// stronger on a bow (0.75), Crit Chance and Bow Attack Speed (0.75) is not. Called under <c>_sync</c>.
    /// </summary>
    private static AffixDefinition ScaledFor(Catalog catalog, AffixDefinition affix, int baseTypeId)
    {
        if (catalog.Scaled.TryGetValue((affix.Id, baseTypeId), out var scaled))
            return scaled;

        // In 32-bit floats like the game, so values on a rounding edge land on the same side (0.5 × 1.17 is 58.4999…%,
        // shown as 58%, not 59%).
        var typeModifier = (float)catalog.AffixEffectModifiers.GetValueOrDefault(baseTypeId);
        var standard = 1f + (float)affix.StandardEffectModifier;
        var factor = standard > 0 ? (1f + typeModifier) / standard : 1f;

        scaled = factor == 1f
            ? affix
            : affix with
            {
                Tiers = affix.Tiers
                    .Select(t => t with { Stats = t.Stats.Select(s => Scale(s, factor)).ToList() })
                    .ToList()
            };
        catalog.Scaled[(affix.Id, baseTypeId)] = scaled;
        return scaled;
    }

    /// <summary>
    /// <paramref name="stat"/> times <paramref name="factor"/>, rounded to as many decimals as the data gives (whole
    /// numbers stay whole, 0.13 keeps two decimals), which is what the game shows: 131 Health on an amulet (×1.17) is
    /// 153, 13% Bow Attack Speed on a bow (×1.75) is 23%.
    /// </summary>
    private static ItemImplicit Scale(ItemImplicit stat, float factor)
    {
        var decimals = Math.Max(DecimalsOf(stat.Min), DecimalsOf(stat.Max));
        double Round(double value) => Math.Round((double)((float)value * factor), decimals, MidpointRounding.AwayFromZero);

        return stat with
        {
            Min = Round(stat.Min),
            Max = Round(stat.Max),
            TemplateMin = Round(stat.TemplateMin),
            TemplateMax = Round(stat.TemplateMax)
        };
    }

    /// <summary>Decimals <paramref name="value"/> is written with in the data (0 … 4).</summary>
    private static int DecimalsOf(double value)
    {
        for (var decimals = 0; decimals < 4; decimals++)
        {
            var scaled = value * Math.Pow(10, decimals);
            if (Math.Abs(scaled - Math.Round(scaled)) < 1e-6)
                return decimals;
        }

        return 4;
    }

    private Catalog GetCatalog()
    {
        lock (_sync)
            return _catalog ??= Load();
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
            return new Catalog([], [], [], false, new Dictionary<int, double>(), []);

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
            var abilityProperties = ReadAbilityProperties(connection);
            var playerProperties = ReadIndexedProperties(connection, "PlayerProperties", "Player property");
            var names = new ImplicitNames(
                properties,
                ReadAilments(connection),
                abilityProperties,
                ReadIndexedProperties(connection, "IdolAltarProperties", "Idol altar property"),
                playerProperties);
            var icons = ReadIcons(connection);

            var result = new List<ItemBase>();
            var affixEffectModifiers = new Dictionary<int, double>();
            var materials = new List<CraftingMaterial>();
            foreach (var (key, name, json) in ReadRows(connection, "SELECT Key, Name, Json FROM ItemTypes"))
            {
                using var document = TryParse(json);
                if (document?.RootElement is not { ValueKind: JsonValueKind.Object } itemType)
                    continue;

                var bases = ParseItemType(key, name, itemType, names, icons);
                result.AddRange(bases);
                materials.AddRange(ParseCraftingMaterials(itemType, icons));
                // Affix values on this item type are scaled by its affixEffectModifier (see ScaledFor).
                if ((GetInt(itemType, "baseTypeID") ?? bases.FirstOrDefault()?.BaseTypeId) is { } typeId
                    && GetDouble(itemType, "affixEffectModifier") is { } modifier)
                    affixEffectModifiers[typeId] = modifier;
            }

            var uniques = ReadUniques(connection, result, names, icons);
            var allAffixes = ReadAffixes(connection, names);
            var affixes = LimitToUniques(allAffixes, uniques);
            // A database without skill or player properties (made before them) shows skill affixes by internal name and
            // player properties by the affix name: outdated too.
            return new Catalog(
                result,
                uniques,
                affixes,
                properties.Count > 0 && abilityProperties.Count > 0 && playerProperties.Count > 0,
                affixEffectModifiers,
                NameShards(materials, allAffixes, icons));
        }
        catch (Exception ex) when (ex is SqliteException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            // A missing or broken database means an empty builder, never a crash.
            return new Catalog([], [], [], false, new Dictionary<int, double>(), []);
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

    // ===== Crafting materials =====

    /// <summary>Rich-text tags of material descriptions (<c>&lt;color=#fff&gt;</c>, <c>&lt;font="Caladea SDF"&gt;</c>, ...).</summary>
    private static readonly Regex RichTextTag = new(@"<[^<>]+>", RegexOptions.Compiled);

    /// <summary>The hint every crafting material ends with ("Press [F] to open the Crafting Panel.").</summary>
    private static readonly Regex CraftingPanelHint = new(@"Press \[?F\]? to open the Crafting Panel\.?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex BlankLines = new(@"\n{3,}", RegexOptions.Compiled);

    /// <summary>
    /// The runes (<c>baseTypeID</c> 102), glyphs (103), affix shards (101) or resources (104, "Key Items") of an item
    /// type; empty for any other type.
    /// A sub item looks like <c>{"name":"Rune of Removal","displayName":"","subTypeID":2,"levelRequirement":15,
    /// "cannotDrop":false,"description":"&lt;color=#fff&gt;Removes a random affix ..."}</c>; the <c>name</c> of a shard is
    /// its affix, and its <c>subTypeID</c> the <c>affixId</c>.
    /// </summary>
    private static List<CraftingMaterial> ParseCraftingMaterials(JsonElement itemType, IconIndex icons)
    {
        var result = new List<CraftingMaterial>();
        if (GetInt(itemType, "baseTypeID") is not { } baseTypeId
            || CraftingMaterialKindExtensions.FromBaseTypeId(baseTypeId) is not { } kind
            || Find(itemType, "subItems") is not { ValueKind: JsonValueKind.Array } subItems)
            return result;

        var typeName = GetString(itemType, "BaseTypeName") ?? kind.DisplayName;
        var shardIcon = icons.Find(typeName, "affix-shard", "affix shard");
        foreach (var subItem in subItems.EnumerateArray())
        {
            if (subItem.ValueKind != JsonValueKind.Object || GetInt(subItem, "subTypeID") is not { } subTypeId)
                continue;

            var internalName = GetString(subItem, "name");
            var name = GetString(subItem, "displayName") ?? internalName;
            if (name is null)
                continue;

            result.Add(new CraftingMaterial(
                kind,
                subTypeId,
                name,
                CleanDescription(GetString(subItem, "description")),
                GetInt(subItem, "levelRequirement") ?? 0,
                GetBool(subItem, "cannotDrop") ?? false,
                kind == CraftingMaterialKind.AffixShard ? shardIcon : icons.Find(typeName, internalName, name)));
        }

        return result;
    }

    /// <summary>
    /// Shards with the shown name, type, category and prefix / suffix icon of their affix (the <c>affixId</c> is the
    /// shard's sub type). Shards of affixes the editor does not read keep the internal name and the plain shard icon.
    /// </summary>
    private static List<CraftingMaterial> NameShards(
        List<CraftingMaterial> materials,
        IReadOnlyList<AffixDefinition> affixes,
        IconIndex icons)
    {
        var shardIcons = new Dictionary<AffixType, string?>
        {
            [AffixType.Prefix] = icons.Find("Affix Shard", "prefix-shard"),
            [AffixType.Suffix] = icons.Find("Affix Shard", "suffix-shard")
        };

        var byId = new Dictionary<int, AffixDefinition>();
        foreach (var affix in affixes)
            byId.TryAdd(affix.Id, affix);

        var result = materials
            .Select(m => m.Kind == CraftingMaterialKind.AffixShard && byId.TryGetValue(m.SubTypeId, out var affix)
                ? m with
                {
                    Name = affix.Name,
                    AffixType = affix.Type,
                    GroupName = affix.GroupName,
                    IconPath = shardIcons[affix.Type] ?? m.IconPath
                }
                : m)
            .ToList();
        return result;
    }

    /// <summary>
    /// <paramref name="description"/> as plain text: without rich-text tags and the crafting panel hint, trimmed.
    /// </summary>
    private static string CleanDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        var text = RichTextTag.Replace(description, string.Empty);
        text = CraftingPanelHint.Replace(text, string.Empty).Replace("\r\n", "\n");
        return BlankLines.Replace(text, "\n\n").Trim();
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

            // Uniques like Unsated Rage drop as legendaries only with affixes from their own list.
            var legendaryAffixIds = new List<int>();
            if (GetBool(unique, "dropsSpecificLegendaryAffixes") == true
                && Find(unique, "droppableLegendaryAffixes") is { ValueKind: JsonValueKind.Array } droppable)
            {
                foreach (var id in droppable.EnumerateArray())
                {
                    if (id.ValueKind == JsonValueKind.Number && id.TryGetInt32(out var affixId))
                        legendaryAffixIds.Add(affixId);
                }
            }

            result.Add(new ItemBase(
                baseTypeId,
                subTypeId,
                name,
                itemBase?.TypeName ?? string.Empty,
                level,
                itemBase?.ClassRequirement ?? 0,
                // "canDropRandomly": false marks boss-only uniques (Fractured Crown, ...), not legacy items.
                CannotDrop: false,
                icons.Find("uniques", name, internalName) ?? itemBase?.IconPath,
                itemBase?.Implicits ?? [],
                uniqueId,
                GetBool(unique, "isSetItem") ?? false,
                itemBase?.Name,
                mods,
                effects,
                legendaryAffixIds,
                GetInt(unique, "droppableLegendaryAffixCount") ?? 1));
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
            if (!names.IsNamed(GetInt(entry, "property") ?? -1, GetInt(entry, "specialTag") ?? 0, GetLong(entry, "tags") ?? 0))
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

    // ===== Affixes =====

    /// <summary>Display categories that belong to one class (affixes of other classes are not offered).</summary>
    private static readonly string[] ClassCategories = ["Acolyte", "Mage", "Primalist", "Sentinel", "Rogue"];

    /// <summary>Property id of a skill's own property (see <c>ImplicitNames</c>).</summary>
    private const int SkillPropertyId = 58;

    /// <summary>Property id of an idol altar property (see <c>ImplicitNames</c>).</summary>
    private const int IdolAltarPropertyId = 130;

    /// <summary><c>rollsOn</c> of equipment affixes.</summary>
    private const int RollsOnEquipment = 0;

    /// <summary><c>rollsOn</c> of idol affixes.</summary>
    private const int RollsOnIdols = 1;

    /// <summary>
    /// Equipment affixes (<c>rollsOn</c> 0) of the craftable kinds and idol affixes (<c>rollsOn</c> 1); which item types
    /// an affix fits comes from its <c>canRollOn</c> list. An affix has one stat (<c>property</c>, <c>tags</c>,
    /// <c>modifierType</c> on the affix) or several (<c>affixProperties</c>); each tier gives the range of the first stat
    /// in <c>minRoll</c> / <c>maxRoll</c> and of the others in <c>extraRolls</c>.
    /// </summary>
    private static List<AffixDefinition> ReadAffixes(SqliteConnection connection, ImplicitNames names)
    {
        var result = new List<AffixDefinition>();

        foreach (var (_, rowName, json) in ReadRowsOrEmpty(connection, "SELECT Key, Name, Json FROM Affixes"))
        {
            using var document = TryParse(json);
            if (document?.RootElement is not { ValueKind: JsonValueKind.Object } affix
                || GetInt(affix, "affixId") is not { } id)
                continue;

            var rollsOn = GetInt(affix, "rollsOn") ?? RollsOnEquipment;
            if (rollsOn is not (RollsOnEquipment or RollsOnIdols))
                continue;

            // Idol affixes may leave the special type out: they are plain affixes.
            var special = GetInt(affix, "specialAffixType") ?? (rollsOn == RollsOnIdols ? (int)AffixCategory.Normal : -1);
            if (!Enum.IsDefined((AffixCategory)special))
                continue;

            var name = GetString(affix, "affixDisplayName") ?? GetString(affix, "affixName") ?? rowName;
            if (name is null)
                continue;

            // Most corrupted idol affixes carry a display name left over from other affixes ("Added Throwing Damage and
            // reduced Melee Damage" on Chill Immunity); the game builds their name itself, so show the internal one
            // without its "Idol" prefix ("Chill Immunity").
            if (rollsOn == RollsOnIdols && special == (int)AffixCategory.Corrupted
                && GetBool(affix, "useGeneratedNameForDisplayName") == true
                && GetString(affix, "affixName") is { } internalName)
                name = WithoutIdolPrefix(internalName);

            var canRollOn = new HashSet<int>();
            if (Find(affix, "canRollOn") is { ValueKind: JsonValueKind.Array } types)
            {
                foreach (var type in types.EnumerateArray())
                {
                    if (type.ValueKind == JsonValueKind.Number && type.TryGetInt32(out var typeId))
                        canRollOn.Add(typeId);
                }
            }

            if (canRollOn.Count == 0)
                continue;

            var groupName = GetInt(affix, "displayCategory") is { } category && AffixCategoryNames.TryGetValue(category, out var categoryName)
                ? categoryName
                : ((AffixCategory)special).ToString();

            // Idol altars roll their corrupted affixes beside the others: tell them apart in the affix picker.
            if (canRollOn.Contains(EquipmentSlotExtensions.IdolAltarBaseTypeId) && (AffixCategory)special == AffixCategory.Corrupted)
                groupName = $"{groupName} (Corrupted)";

            var tiers = ParseAffixTiers(affix, name, names);
            if (tiers.Count == 0)
                continue;

            // Skill affixes have only an internal name ("Blood Rage Deadly Strikes") and idol altar affixes a shared,
            // misleading one ("Maximum Idols Equipped"): show what the game shows, the property of their one stat
            // ("You have Incisive Rage", "Health per Equipped Heretical Idol").
            var ownProperty = GetInt(affix, "property");
            if (((ownProperty == SkillPropertyId && GetString(affix, "affixDisplayName") is null) || ownProperty == IdolAltarPropertyId)
                && tiers[0].Stats is [var onlyStat]
                && !string.Equals(onlyStat.Property, name, StringComparison.Ordinal))
                name = onlyStat.Property;

            result.Add(new AffixDefinition(
                id,
                name,
                GetString(affix, "affixTitle"),
                GetInt(affix, "type") == 1 ? AffixType.Suffix : AffixType.Prefix,
                (AffixCategory)special,
                groupName,
                ClassCategories.FirstOrDefault(c => string.Equals(c, groupName, StringComparison.OrdinalIgnoreCase)),
                GetInt(affix, "levelRequirement") ?? 0,
                canRollOn,
                tiers)
            {
                StandardEffectModifier = GetDouble(affix, "standardAffixEffectModifier") ?? 0
            });
        }

        return result;
    }

    /// <summary>"Idol Chill Immunity" → "Chill Immunity".</summary>
    private static string WithoutIdolPrefix(string name)
    {
        const string prefix = "Idol ";
        var trimmed = name.Trim();
        return trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && trimmed.Length > prefix.Length
            ? trimmed[prefix.Length..]
            : trimmed;
    }

    private static List<AffixTier> ParseAffixTiers(JsonElement affix, string affixName, ImplicitNames names)
    {
        // The stats of the affix: its own fields, or the list in affixProperties.
        var stats = new List<(int Property, int SpecialTag, long Tags, int ModifierType)>();
        if (Find(affix, "affixProperties") is { ValueKind: JsonValueKind.Array } properties && properties.GetArrayLength() > 0)
        {
            foreach (var property in properties.EnumerateArray())
            {
                if (property.ValueKind == JsonValueKind.Object)
                    stats.Add((GetInt(property, "property") ?? -1, GetInt(property, "specialTag") ?? 0, GetLong(property, "tags") ?? 0, GetInt(property, "modifierType") ?? 0));
            }
        }
        else
        {
            stats.Add((GetInt(affix, "property") ?? -1, GetInt(affix, "specialTag") ?? 0, GetLong(affix, "tags") ?? 0, GetInt(affix, "modifierType") ?? 0));
        }

        var result = new List<AffixTier>();
        if (Find(affix, "tiers") is not { ValueKind: JsonValueKind.Array } tiers)
            return result;

        var number = 0;
        foreach (var tier in tiers.EnumerateArray())
        {
            number++;
            if (tier.ValueKind != JsonValueKind.Object)
                continue;

            var ranges = new List<(double Min, double Max)> { (GetDouble(tier, "minRoll") ?? 0, GetDouble(tier, "maxRoll") ?? 0) };
            if (Find(tier, "extraRolls") is { ValueKind: JsonValueKind.Array } extra)
            {
                foreach (var roll in extra.EnumerateArray())
                {
                    if (roll.ValueKind == JsonValueKind.Object)
                        ranges.Add((GetDouble(roll, "minRoll") ?? 0, GetDouble(roll, "maxRoll") ?? 0));
                }
            }

            var tierStats = new List<ItemImplicit>();
            for (var i = 0; i < stats.Count && i < ranges.Count; i++)
            {
                var (property, specialTag, tags, modifierType) = stats[i];
                var (min, max) = ranges[i];
                var modifier = BuildModifier(property, specialTag, tags, modifierType, min, max, names);

                // Skill-specific properties have no name in the data: the affix name says what they do.
                if (!names.IsNamed(property, specialTag, tags))
                    modifier = modifier with { Property = affixName };

                tierStats.Add(modifier);
            }

            if (tierStats.Count > 0)
                result.Add(new AffixTier(number, tierStats));
        }

        return result;
    }

    /// <summary>
    /// Affixes that only the listed uniques drop (their <c>droppableLegendaryAffixes</c>) are kept for those uniques:
    /// they get <see cref="AffixDefinition.OnlyOnUniques"/> and the unique's name as their group.
    /// </summary>
    private static List<AffixDefinition> LimitToUniques(List<AffixDefinition> affixes, IReadOnlyList<ItemBase> uniques)
    {
        var owners = new Dictionary<int, List<ItemBase>>();
        foreach (var unique in uniques)
        {
            foreach (var affixId in unique.LegendaryAffixes)
            {
                if (!owners.TryGetValue(affixId, out var list))
                    owners[affixId] = list = [];
                list.Add(unique);
            }
        }

        return affixes
            .Select(a => owners.TryGetValue(a.Id, out var list)
                ? a with
                {
                    OnlyOnUniques = list.Select(u => u.UniqueId!.Value).ToHashSet(),
                    GroupName = string.Join(" / ", list.Select(u => u.Name).Distinct())
                }
                : a)
            .ToList();
    }

    /// <summary>Names of the affix display categories (the <c>affixDisplayCategories</c> list is not stored, so they are fixed here).</summary>
    private static readonly Dictionary<int, string> AffixCategoryNames = new()
    {
        [0] = "Attributes", [1] = "Melee", [2] = "Spell", [3] = "Damage Type", [4] = "General", [5] = "Throwing",
        [6] = "Health", [7] = "Block", [8] = "Resistance and Armor", [9] = "Potion", [10] = "Dodge", [11] = "Glancing Blow",
        [12] = "Health Recovery", [13] = "Ward", [14] = "Stun", [15] = "Critical Strike Avoidance", [16] = "Damage Dealt to Mana",
        [17] = "Leech", [18] = "Movement", [19] = "Cooldown", [20] = "Reflect", [21] = "Mana", [22] = "Minion", [23] = "Ailments",
        [24] = "Acolyte", [25] = "Mage", [26] = "Primalist", [27] = "Sentinel", [28] = "Rogue", [35] = "Bow",
        [36] = "Experimental", [37] = "Personal", [38] = "Parry", [39] = "Set", [42] = "Corrupted", [43] = "Idol Altars",
        [44] = "Variant"
    };

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

        return BuildModifier(
            GetInt(entry, "property") ?? -1,
            GetInt(entry, "specialTag") ?? 0,
            GetLong(entry, "tags") ?? 0,
            GetInt(entry, "type") ?? 0,
            first,
            second,
            names);
    }

    /// <summary>A modifier with the game's wording rules for its property, tags and special tag.</summary>
    private static ItemImplicit BuildModifier(int property, int specialTag, long tags, int modifierType, double first, double second, ImplicitNames names)
    {
        var type = Enum.IsDefined((ImplicitModifierType)modifierType) ? (ImplicitModifierType)modifierType : ImplicitModifierType.Added;
        // A skill's own property (property 58) takes its name and display rules from that skill's list.
        var abilityInfo = names.GetIndexedProperty(property, tags, specialTag);
        var info = abilityInfo ?? names.GetProperty(property);
        var name = abilityInfo?.Name ?? names.Compose(info, property, tags, specialTag);

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
    /// <param name="abilityProperties">
    /// Per skill (<c>abilityID</c>) its own properties in order, from the <c>AbilityProperties</c> table; empty for
    /// databases made before it.
    /// </param>
    /// <param name="idolAltarProperties">Idol altar properties in order (<c>IdolAltarProperties</c> table), or empty.</param>
    /// <param name="playerProperties">Player properties in order (<c>PlayerProperties</c> table), or empty.</param>
    private sealed class ImplicitNames(
        Dictionary<int, PropertyInfo> properties,
        Dictionary<int, string> ailments,
        Dictionary<int, IReadOnlyList<PropertyInfo>> abilityProperties,
        IReadOnlyList<PropertyInfo> idolAltarProperties,
        IReadOnlyList<PropertyInfo> playerProperties)
    {
        /// <summary>Tag bits (the game's <c>AT</c> flags) in the order their words go before the property name.</summary>
        private static readonly (long Bit, string Word)[] PrefixTags =
        [
            (8192, "Minion"), (16384, "Totem"),
            (512, "Melee"), (256, "Spell"), (1024, "Throwing"), (2048, "Bow"),
            (1, "Physical"), (2, "Lightning"), (4, "Cold"), (8, "Fire"), (16, "Void"), (32, "Necrotic"), (64, "Poison"), (128, "Elemental")
        ];

        private const long DamageOverTimeTag = 4096;

        /// <summary>
        /// A property of one specific skill: <c>tags</c> holds the skill's <c>abilityID</c> and <c>specialTag</c> the
        /// index in that skill's property list (e.g. Blood Rage, index 5: "You have Incisive Rage").
        /// </summary>
        private const int AbilityProperty = 58;
        private const int LevelOfSkillsProperty = 88;

        /// <summary>An idol altar property: <c>tags</c> is the index in the idol altar property list.</summary>
        private const int IdolAltarProperty = 130;

        /// <summary>
        /// A player property: <c>tags</c> is the index in the player property list (e.g. 134: "Chance to cast Icicle on
        /// Bow Hit" of Reign of Winter).
        /// </summary>
        private const int PlayerProperty = 98;

        public PropertyInfo? GetProperty(int property) => properties.GetValueOrDefault(property);

        /// <summary>
        /// The skill property (58), idol altar property (130) or player property (98) a modifier refers to; <c>null</c> for
        /// other properties or when unknown.
        /// </summary>
        public PropertyInfo? GetIndexedProperty(int property, long tags, int specialTag) => property switch
        {
            AbilityProperty when tags is >= 0 and <= int.MaxValue
                                 && abilityProperties.TryGetValue((int)tags, out var list)
                                 && specialTag >= 0 && specialTag < list.Count => list[specialTag],
            IdolAltarProperty when tags >= 0 && tags < idolAltarProperties.Count => idolAltarProperties[(int)tags],
            PlayerProperty when tags >= 0 && tags < playerProperties.Count => playerProperties[(int)tags],
            _ => null
        };

        /// <summary>
        /// <c>false</c> for properties without a name, for skill properties (58) whose skill is unknown, and for
        /// "Level of Skills" with a special tag (its tags then select a single skill).
        /// </summary>
        public bool IsNamed(int property, int specialTag, long tags) =>
            property is AbilityProperty or IdolAltarProperty or PlayerProperty
                ? GetIndexedProperty(property, tags, specialTag) is not null
                : properties.ContainsKey(property)
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

    /// <summary>
    /// <c>abilityProperties</c>: <c>{"abilityID":950,"properties":[{"propertyName":"You have Berserking Rage",...}, ...]}</c>.
    /// Databases made before the table existed simply have none.
    /// </summary>
    private static Dictionary<int, IReadOnlyList<PropertyInfo>> ReadAbilityProperties(SqliteConnection connection)
    {
        var result = new Dictionary<int, IReadOnlyList<PropertyInfo>>();

        foreach (var (key, _, json) in ReadRowsOrEmpty(connection, "SELECT Key, Name, Json FROM AbilityProperties"))
        {
            using var document = TryParse(json);
            if (document?.RootElement is not { ValueKind: JsonValueKind.Object } ability
                || (GetInt(ability, "abilityID")
                    ?? (int.TryParse(key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var k) ? (int?)k : null)) is not { } abilityId
                || Find(ability, "properties") is not { ValueKind: JsonValueKind.Array } entries)
                continue;

            var list = new List<PropertyInfo>();
            foreach (var entry in entries.EnumerateArray())
            {
                // Keep the position even for an unreadable entry: affixes refer to properties by index.
                list.Add(entry.ValueKind == JsonValueKind.Object
                    ? new PropertyInfo(
                        GetString(entry, "propertyName") ?? $"Skill property {list.Count}",
                        GetBool(entry, "displayAddedAsPercentage") ?? false,
                        GetBool(entry, "displayAddedAsTenthOfValue") ?? false,
                        GetBool(entry, "dontDisplayPlus") ?? false,
                        GetBool(entry, "nameIncludesModifier") ?? false)
                    : new PropertyInfo($"Skill property {list.Count}", false, false, false, false));
            }

            result.TryAdd(abilityId, list);
        }

        return result;
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

            result.TryAdd(id.Value, ReadPropertyInfo(property, name));
        }

        // Properties the data leaves out but items use; only for databases that have the table at all.
        if (result.Count > 0)
        {
            foreach (var (id, name) in MissingPropertyNames)
                result.TryAdd(id, new PropertyInfo(name, false, false, false, false));
        }

        return result;
    }

    /// <summary>
    /// Names of properties missing from maxroll's <c>properties</c> list. "Ailment" is replaced by the modifier's
    /// ailment (special tag), e.g. property 120 with Frenzy: "30% increased Effect of Frenzy on You".
    /// </summary>
    private static readonly Dictionary<int, string> MissingPropertyNames = new()
    {
        [120] = "Effect of Ailment on You"
    };

    private static PropertyInfo ReadPropertyInfo(JsonElement property, string name) => new(
        name,
        GetBool(property, "displayAddedAsPercentage") ?? false,
        GetBool(property, "displayAddedAsTenthOfValue") ?? false,
        GetBool(property, "dontDisplayPlus") ?? false,
        GetBool(property, "nameIncludesModifier") ?? false);

    /// <summary>
    /// <c>idolAltarProperties</c> / <c>playerProperties</c>: lists of properties (no ids) that affixes refer to by index
    /// (<c>tags</c> of property 130 / 98). Rows are keyed by their position; databases made before the table have none.
    /// </summary>
    private static List<PropertyInfo> ReadIndexedProperties(SqliteConnection connection, string table, string label)
    {
        var rows = ReadRowsOrEmpty(connection, $"SELECT Key, Name, Json FROM {table}")
            .Select(r => (Index: int.TryParse(r.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : -1, r.Json))
            .Where(r => r.Index >= 0)
            .OrderBy(r => r.Index)
            .ToList();

        var result = new List<PropertyInfo>();
        foreach (var (index, json) in rows)
        {
            // Keep positions: a gap would shift every later property.
            while (result.Count < index)
                result.Add(new PropertyInfo($"{label} {result.Count}", false, false, false, false));

            using var document = TryParse(json);
            result.Add(document?.RootElement is { ValueKind: JsonValueKind.Object } property
                       && GetString(property, "propertyName") is { } name
                ? ReadPropertyInfo(property, name)
                : new PropertyInfo($"{label} {index}", false, false, false, false));
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

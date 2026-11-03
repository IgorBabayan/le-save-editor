using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

/// <summary>Writes changes of the gold, the material storage and the discovered blessings back into a stash file.</summary>
public interface IStashSaveWriter
{
    /// <summary>
    /// Writes <paramref name="amounts"/> (every material the storage should hold) and <paramref name="gold"/> into
    /// <paramref name="stash"/>'s file, and adds <paramref name="discoveredBlessings"/> to its <c>unlockedBlessings</c>, keeping
    /// everything else as it is. The previous file is kept as a <c>.bak</c> backup next to it.
    /// </summary>
    /// <returns>The stash as read back from the written file, and the path of the backup.</returns>
    /// <exception cref="SaveFileChangedException">The file changed after it was read (e.g. the game saved it).</exception>
    /// <exception cref="IOException">The file cannot be read or written.</exception>
    /// <exception cref="UnauthorizedAccessException">The file or its folder is read-only.</exception>
    /// <exception cref="JsonException">The file is not a valid stash file.</exception>
    Task<StashSaveResult> SaveAsync(
        StashSave stash,
        IReadOnlyDictionary<CraftingMaterialKey, int> amounts,
        int gold,
        IReadOnlyCollection<int>? discoveredBlessings = null,
        CancellationToken cancellationToken = default);
}

/// <summary>Outcome of <see cref="IStashSaveWriter.SaveAsync"/>.</summary>
public sealed record StashSaveResult(StashSave Stash, string BackupPath);

/// <summary>
/// Writes the material storage of a stash file. Only materials whose amount changed are touched: a changed amount is
/// written into the material's entry, a material set to 0 loses its entry, and a new material gets one at the end of
/// <c>savedShards</c> (<c>{"shardType": affixId, "quantity": n}</c>), <c>materialsList</c> (a rune or glyph item
/// <c>[2, baseTypeID, subTypeID]</c> at position −4, −4, as the game stores them) or <c>keysList</c> (a resource item as
/// the game stores it there: <c>[6, id, id, id, id, 104, subTypeID]</c> in container 100 at position 0, subTypeID; the
/// game drops resource entries in any other shape when it loads the stash, so such entries are rewritten). A changed
/// gold amount is written into <c>gold</c>; blessings the stash has not discovered yet are added at the end of
/// <c>unlockedBlessings</c> (otherwise the game shows them as "Undiscovered"). Every other field and entry stays as
/// the game wrote it; the file is written compact, through a temporary file, keeping the old one as a backup.
/// </summary>
public sealed class StashSaveWriter : IStashSaveWriter
{
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    public async Task<StashSaveResult> SaveAsync(
        StashSave stash,
        IReadOnlyDictionary<CraftingMaterialKey, int> amounts,
        int gold,
        IReadOnlyCollection<int>? discoveredBlessings = null,
        CancellationToken cancellationToken = default)
    {
        var path = stash.FilePath;
        var content = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        if (!string.Equals(CharacterSaveParser.ComputeContentHash(content), stash.ContentHash, StringComparison.Ordinal))
            throw new SaveFileChangedException(path);

        var newContent = Apply(content, stash.Amounts, amounts, stash.Gold, gold, discoveredBlessings);

        // Never write something the editor itself cannot read back.
        var saved = StashSaveParser.Parse(path, newContent);

        cancellationToken.ThrowIfCancellationRequested();
        var tempPath = path + CharacterSaveWriter.TempSuffix;
        await File.WriteAllTextAsync(tempPath, newContent, CharacterSaveWriter.Utf8NoBom, cancellationToken).ConfigureAwait(false);

        var backupPath = await Task.Run(() => CharacterSaveWriter.ReplaceWithBackup(tempPath, path), CancellationToken.None)
            .ConfigureAwait(false);
        return new StashSaveResult(saved, backupPath);
    }

    /// <summary>
    /// The file content with the materials whose amount differs between <paramref name="original"/> and
    /// <paramref name="amounts"/> written into it.
    /// </summary>
    public static string Apply(
        string content,
        IReadOnlyDictionary<CraftingMaterialKey, int> original,
        IReadOnlyDictionary<CraftingMaterialKey, int> amounts) =>
        Apply(content, original, amounts, 0, 0);

    /// <summary>
    /// The file content with the materials whose amount differs between <paramref name="original"/> and
    /// <paramref name="amounts"/>, the gold when <paramref name="gold"/> differs from <paramref name="originalGold"/>, and the
    /// <paramref name="discoveredBlessings"/> missing from <c>unlockedBlessings</c>, written into it.
    /// </summary>
    public static string Apply(
        string content,
        IReadOnlyDictionary<CraftingMaterialKey, int> original,
        IReadOnlyDictionary<CraftingMaterialKey, int> amounts,
        int originalGold,
        int gold,
        IReadOnlyCollection<int>? discoveredBlessings = null)
    {
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        if (start < 0 || end < start)
            throw new JsonException("The stash file holds no JSON object.");

        if (JsonNode.Parse(content[start..(end + 1)], documentOptions: DocumentOptions) is not JsonObject root)
            throw new JsonException("The stash file holds no JSON object.");

        // Every material whose amount changed (0 = remove).
        var changes = amounts.Keys.Concat(original.Keys)
            .Distinct()
            .Select(key => (Key: key, Quantity: Math.Max(0, amounts.GetValueOrDefault(key))))
            .Where(c => c.Quantity != original.GetValueOrDefault(c.Key))
            .ToDictionary(c => c.Key, c => c.Quantity);
        gold = Math.Max(0, gold);
        var goldChanged = gold != Math.Max(0, originalGold);
        var blessingsAdded = AddBlessings(root, discoveredBlessings);
        if (changes.Count == 0 && !goldChanged && !blessingsAdded)
            return content;

        if (goldChanged)
            root[StashSaveParser.GoldField] = gold;

        if (changes.Count == 0)
            return content[..start] + root.ToJsonString(CharacterSaveWriter.CompactJson) + content[(end + 1)..];

        // Resources written by older editor versions ([2, 104, sub] at −4, −4, no container) are dropped by the game.
        if (root[StashSaveParser.KeyList] is JsonArray keys)
            RepairKeyEntries(keys);

        foreach (var list in new[] { StashSaveParser.ShardList, StashSaveParser.MaterialList, StashSaveParser.KeyList })
        {
            // A list the file lacks is only created when a material is added to it.
            var listChanges = changes.Where(c => StashSaveParser.ListOf(c.Key.Kind) == list).ToDictionary(c => c.Key, c => c.Value);
            if (listChanges.Count == 0)
                continue;

            if (list == StashSaveParser.ShardList)
                ApplyTo(GetArray(root, list), listChanges, ShardKey, CreateShardEntry);
            else if (list == StashSaveParser.KeyList)
                ApplyTo(GetArray(root, list), listChanges, entry => MaterialKey(entry, list), CreateKeyEntry);
            else
                ApplyTo(GetArray(root, list), listChanges, entry => MaterialKey(entry, list), CreateMaterialEntry);
        }

        return content[..start] + root.ToJsonString(CharacterSaveWriter.CompactJson) + content[(end + 1)..];
    }

    /// <summary>
    /// Writes <paramref name="changes"/> (all of materials kept in <paramref name="array"/>) into it: the first entry of
    /// a changed material takes its new amount (further entries of it are removed), a material set to 0 loses its
    /// entries and a material without an entry gets a new one.
    /// </summary>
    private static void ApplyTo(
        JsonArray array,
        IReadOnlyDictionary<CraftingMaterialKey, int> changes,
        Func<JsonNode?, CraftingMaterialKey?> keyOf,
        Func<CraftingMaterialKey, int, JsonObject> create)
    {
        var written = new HashSet<CraftingMaterialKey>();
        var entries = array.ToList();
        array.Clear();

        foreach (var entry in entries)
        {
            if (keyOf(entry) is not { } key || !changes.TryGetValue(key, out var quantity))
            {
                array.Add(entry);
                continue;
            }

            if (quantity <= 0 || !written.Add(key))
                continue;

            ((JsonObject)entry!)["quantity"] = quantity;
            array.Add(entry);
        }

        foreach (var (key, quantity) in changes)
        {
            if (quantity > 0 && !written.Contains(key))
                array.Add(create(key, quantity));
        }
    }

    private static JsonArray GetArray(JsonObject root, string name)
    {
        switch (root[name])
        {
            case JsonArray array:
                return array;
            case null:
                var created = new JsonArray();
                root[name] = created;
                return created;
            default:
                throw new JsonException($"{name} is not an array.");
        }
    }

    private static CraftingMaterialKey? ShardKey(JsonNode? entry) =>
        entry is JsonObject shard && shard["shardType"] is JsonValue type && type.TryGetValue<int>(out var affixId)
            ? new CraftingMaterialKey(CraftingMaterialKind.AffixShard, affixId)
            : null;

    /// <summary>The material of an item entry of <paramref name="list"/>, when it belongs in that list.</summary>
    private static CraftingMaterialKey? MaterialKey(JsonNode? entry, string list)
    {
        if (entry is not JsonObject item || item["data"] is not JsonArray data)
            return null;

        var bytes = new List<byte>(data.Count);
        foreach (var value in data)
        {
            if (value is not JsonValue number || !number.TryGetValue<int>(out var b) || b is < 0 or > 255)
                return null;
            bytes.Add((byte)b);
        }

        return CraftingMaterialData.Read(bytes) is { Kind: not CraftingMaterialKind.AffixShard } material
               && StashSaveParser.ListOf(material.Kind) == list
            ? new CraftingMaterialKey(material.Kind, material.SubTypeId)
            : null;
    }

    private static JsonObject CreateShardEntry(CraftingMaterialKey key, int quantity) => new()
    {
        ["shardType"] = key.SubTypeId,
        ["quantity"] = quantity
    };

    /// <summary>Container of the entries of <c>keysList</c> (the Resources tab).</summary>
    private const int KeyContainerId = 100;

    /// <summary>
    /// A resource entry as the game writes it into <c>keysList</c>: format 6 with a new instance id, container 100 and
    /// the Resources-tab slot of the resource (x 0, y = sub type).
    /// </summary>
    private static JsonObject CreateKeyEntry(CraftingMaterialKey key, int quantity)
    {
        var id = new byte[ItemDataReader.InstanceIdLength];
        Random.Shared.NextBytes(id);
        id[0] &= 0x7F;

        var data = new JsonArray(ItemDataReader.Version6);
        foreach (var b in id)
            data.Add((int)b);
        data.Add((int)key.Kind);
        data.Add(key.SubTypeId);

        return new JsonObject
        {
            ["itemData"] = null,
            ["data"] = data,
            ["inventoryPosition"] = new JsonObject { ["x"] = 0, ["y"] = key.SubTypeId },
            ["quantity"] = quantity,
            ["containerID"] = KeyContainerId,
            ["formatVersion"] = 2
        };
    }

    /// <summary>
    /// Appends the blessings of <paramref name="blessings"/> that <c>unlockedBlessings</c> does not list yet (creating the
    /// list when the file has none); <c>true</c> when something was added.
    /// </summary>
    private static bool AddBlessings(JsonObject root, IReadOnlyCollection<int>? blessings)
    {
        if (blessings is null || blessings.Count == 0)
            return false;

        var list = root[StashSaveParser.UnlockedBlessingsField] as JsonArray;
        var known = list?
            .Select(n => n is JsonValue value && value.TryGetValue<int>(out var id) ? id : (int?)null)
            .OfType<int>()
            .ToHashSet() ?? new HashSet<int>();
        var missing = blessings.Where(id => id >= 0 && known.Add(id)).ToList();
        if (missing.Count == 0)
            return false;

        if (list is null)
        {
            list = [];
            root[StashSaveParser.UnlockedBlessingsField] = list;
        }

        foreach (var id in missing)
            list.Add(id);
        return true;
    }

    /// <summary>
    /// Rewrites resource entries of <paramref name="keys"/> that are not in the game's shape (format 6, container 100,
    /// position 0, sub type) into it, keeping their amount; entries in the game's shape and anything else stay as they are.
    /// </summary>
    private static void RepairKeyEntries(JsonArray keys)
    {
        for (var i = 0; i < keys.Count; i++)
        {
            if (keys[i] is not JsonObject entry
                || MaterialKey(entry, StashSaveParser.KeyList) is not { } key
                || IsGameKeyEntry(entry, key))
                continue;

            var quantity = entry["quantity"] is JsonValue value && value.TryGetValue<int>(out var n) ? n : 1;
            keys[i] = CreateKeyEntry(key, Math.Max(1, quantity));
        }
    }

    private static bool IsGameKeyEntry(JsonObject entry, CraftingMaterialKey key) =>
        entry["data"] is JsonArray { Count: > 0 } data
        && data[0] is JsonValue version && version.TryGetValue<int>(out var v) && v == ItemDataReader.Version6
        && entry["containerID"] is JsonValue container && container.TryGetValue<int>(out var c) && c == KeyContainerId
        && entry["inventoryPosition"] is JsonObject position
        && position["x"] is JsonValue x && x.TryGetValue<int>(out var px) && px == 0
        && position["y"] is JsonValue y && y.TryGetValue<int>(out var py) && py == key.SubTypeId;

    /// <summary>A rune or glyph entry with the properties (and their order) the game writes.</summary>
    private static JsonObject CreateMaterialEntry(CraftingMaterialKey key, int quantity) => new()
    {
        ["itemData"] = null,
        ["data"] = new JsonArray(ItemDataReader.Version2, (int)key.Kind, key.SubTypeId),
        ["inventoryPosition"] = new JsonObject { ["x"] = -4, ["y"] = -4 },
        ["quantity"] = quantity,
        ["formatVersion"] = 2
    };
}

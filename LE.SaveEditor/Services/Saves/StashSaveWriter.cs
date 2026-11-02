using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

/// <summary>Writes changes of the material storage back into a stash file.</summary>
public interface IStashSaveWriter
{
    /// <summary>
    /// Writes <paramref name="amounts"/> (every material the storage should hold) into <paramref name="stash"/>'s file,
    /// keeping everything else as it is. The previous file is kept as a <c>.bak</c> backup next to it.
    /// </summary>
    /// <returns>The stash as read back from the written file, and the path of the backup.</returns>
    /// <exception cref="SaveFileChangedException">The file changed after it was read (e.g. the game saved it).</exception>
    /// <exception cref="IOException">The file cannot be read or written.</exception>
    /// <exception cref="UnauthorizedAccessException">The file or its folder is read-only.</exception>
    /// <exception cref="JsonException">The file is not a valid stash file.</exception>
    Task<StashSaveResult> SaveAsync(
        StashSave stash,
        IReadOnlyDictionary<CraftingMaterialKey, int> amounts,
        CancellationToken cancellationToken = default);
}

/// <summary>Outcome of <see cref="IStashSaveWriter.SaveAsync"/>.</summary>
public sealed record StashSaveResult(StashSave Stash, string BackupPath);

/// <summary>
/// Writes the material storage of a stash file. Only materials whose amount changed are touched: a changed amount is
/// written into the material's entry, a material set to 0 loses its entry, and a new material gets one at the end of
/// <c>savedShards</c> (<c>{"shardType": affixId, "quantity": n}</c>) or <c>materialsList</c> (a rune or glyph item
/// <c>[2, baseTypeID, subTypeID]</c> at position −4, −4, as the game stores them). Every other field and entry stays as
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
        CancellationToken cancellationToken = default)
    {
        var path = stash.FilePath;
        var content = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        if (!string.Equals(CharacterSaveParser.ComputeContentHash(content), stash.ContentHash, StringComparison.Ordinal))
            throw new SaveFileChangedException(path);

        var newContent = Apply(content, stash.Amounts, amounts);

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
        IReadOnlyDictionary<CraftingMaterialKey, int> amounts)
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
        if (changes.Count == 0)
            return content;

        ApplyTo(GetArray(root, "savedShards"), changes, ShardKey, CreateShardEntry,
            key => key.Kind == CraftingMaterialKind.AffixShard);
        ApplyTo(GetArray(root, "materialsList"), changes, MaterialKey, CreateMaterialEntry,
            key => key.Kind is CraftingMaterialKind.Rune or CraftingMaterialKind.Glyph);

        return content[..start] + root.ToJsonString(CharacterSaveWriter.CompactJson) + content[(end + 1)..];
    }

    /// <summary>
    /// Writes the changes that <paramref name="belongs"/> to <paramref name="array"/>: the first entry of a changed
    /// material takes its new amount (further entries of it are removed), a material set to 0 loses its entries and a
    /// material without an entry gets a new one.
    /// </summary>
    private static void ApplyTo(
        JsonArray array,
        IReadOnlyDictionary<CraftingMaterialKey, int> changes,
        Func<JsonNode?, CraftingMaterialKey?> keyOf,
        Func<CraftingMaterialKey, int, JsonObject> create,
        Func<CraftingMaterialKey, bool> belongs)
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
            if (quantity > 0 && belongs(key) && !written.Contains(key))
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

    private static CraftingMaterialKey? MaterialKey(JsonNode? entry)
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

        return CraftingMaterialData.Read(bytes) is { Kind: CraftingMaterialKind.Rune or CraftingMaterialKind.Glyph } material
            ? new CraftingMaterialKey(material.Kind, material.SubTypeId)
            : null;
    }

    private static JsonObject CreateShardEntry(CraftingMaterialKey key, int quantity) => new()
    {
        ["shardType"] = key.SubTypeId,
        ["quantity"] = quantity
    };

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

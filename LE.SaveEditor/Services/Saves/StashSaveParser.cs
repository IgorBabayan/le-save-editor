using System.IO;
using System.Text.Json;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

/// <summary>
/// Reads the gold, the material storage and the discovered blessings of a stash file (<see cref="StashSave"/>); everything
/// else in it is ignored.
/// </summary>
public static class StashSaveParser
{
    /// <summary>List of affix shards.</summary>
    internal const string ShardList = "savedShards";

    /// <summary>List of runes and glyphs.</summary>
    internal const string MaterialList = "materialsList";

    /// <summary>List of resources (key items).</summary>
    internal const string KeyList = "keysList";

    /// <summary>Gold of the stash.</summary>
    internal const string GoldField = "gold";

    /// <summary>Blessings (<c>subTypeID</c>s) the characters of the stash have discovered.</summary>
    internal const string UnlockedBlessingsField = "unlockedBlessings";

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    /// <exception cref="IOException">The file cannot be read.</exception>
    /// <exception cref="JsonException">The content is not a valid stash file.</exception>
    public static async Task<StashSave> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var content = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
        return Parse(filePath, content);
    }

    /// <summary>Parses file content (with or without the leading <c>EPOCH</c> marker).</summary>
    /// <exception cref="JsonException">The content is not a valid stash file.</exception>
    public static StashSave Parse(string filePath, string content)
    {
        using var document = JsonDocument.Parse(CharacterSaveParser.StripEpochPrefix(content), DocumentOptions);
        var root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
            throw new JsonException("The stash file holds no JSON object.");

        var amounts = new Dictionary<CraftingMaterialKey, int>();
        foreach (var (key, quantity) in ReadShards(root).Concat(ReadMaterials(root, MaterialList)).Concat(ReadMaterials(root, KeyList)))
            amounts[key] = amounts.GetValueOrDefault(key) + quantity;

        return new StashSave
        {
            FilePath = filePath,
            StashType = GetInt(root, "stashType"),
            SoloCharacterId = root.TryGetProperty("soloChallengeCharacterId", out var solo) ? AsText(solo) : null,
            Gold = ReadGold(root),
            UnlockedBlessings = ReadUnlockedBlessings(root),
            Amounts = amounts,
            ContentHash = CharacterSaveParser.ComputeContentHash(content)
        };
    }

    /// <summary><c>savedShards</c>: <c>{"shardType": affixId, "quantity": n}</c>.</summary>
    internal static IEnumerable<(CraftingMaterialKey Key, int Quantity)> ReadShards(JsonElement root)
    {
        if (!root.TryGetProperty(ShardList, out var shards) || shards.ValueKind != JsonValueKind.Array)
            yield break;

        foreach (var shard in shards.EnumerateArray())
        {
            if (shard.ValueKind == JsonValueKind.Object
                && shard.TryGetProperty("shardType", out var type)
                && type.ValueKind == JsonValueKind.Number
                && type.TryGetInt32(out var affixId))
                yield return (new CraftingMaterialKey(CraftingMaterialKind.AffixShard, affixId), GetInt(shard, "quantity"));
        }
    }

    /// <summary>
    /// The stash list a material of <paramref name="kind"/> is kept in: <c>savedShards</c>, <c>materialsList</c> (runes,
    /// glyphs) or <c>keysList</c> (resources).
    /// </summary>
    internal static string ListOf(CraftingMaterialKind kind) => kind switch
    {
        CraftingMaterialKind.AffixShard => ShardList,
        CraftingMaterialKind.KeyItem => KeyList,
        _ => MaterialList
    };

    /// <summary>
    /// The item entries of <paramref name="list"/> (<c>materialsList</c> or <c>keysList</c>) whose material belongs in
    /// that list; other entries are skipped.
    /// </summary>
    internal static IEnumerable<(CraftingMaterialKey Key, int Quantity)> ReadMaterials(JsonElement root, string list)
    {
        if (!root.TryGetProperty(list, out var materials) || materials.ValueKind != JsonValueKind.Array)
            yield break;

        foreach (var entry in materials.EnumerateArray())
        {
            if (ReadMaterialKey(entry) is { } key && ListOf(key.Kind) == list)
                yield return (key, GetInt(entry, "quantity", 1));
        }
    }

    /// <summary>The rune, glyph or resource an item entry holds; <c>null</c> for anything else (also affix shards).</summary>
    internal static CraftingMaterialKey? ReadMaterialKey(JsonElement entry)
    {
        if (entry.ValueKind != JsonValueKind.Object
            || !entry.TryGetProperty("data", out var data)
            || data.ValueKind != JsonValueKind.Array)
            return null;

        var bytes = new List<byte>();
        foreach (var value in data.EnumerateArray())
        {
            if (value.ValueKind != JsonValueKind.Number || !value.TryGetByte(out var b))
                return null;
            bytes.Add(b);
        }

        return CraftingMaterialData.Read(bytes) is { Kind: not CraftingMaterialKind.AffixShard } material
            ? new CraftingMaterialKey(material.Kind, material.SubTypeId)
            : null;
    }

    /// <summary><c>unlockedBlessings</c>, in file order; empty when the file has none.</summary>
    internal static IReadOnlyList<int> ReadUnlockedBlessings(JsonElement root)
    {
        if (!root.TryGetProperty(UnlockedBlessingsField, out var list) || list.ValueKind != JsonValueKind.Array)
            return [];

        return list.EnumerateArray()
            .Where(e => e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out _))
            .Select(e => e.GetInt32())
            .ToList();
    }

    /// <summary><c>gold</c>, limited to 0 … <see cref="int.MaxValue"/>; 0 when the file has none.</summary>
    internal static int ReadGold(JsonElement root)
    {
        if (!root.TryGetProperty(GoldField, out var gold) || gold.ValueKind != JsonValueKind.Number)
            return 0;

        if (gold.TryGetInt64(out var whole))
            return (int)Math.Clamp(whole, 0, int.MaxValue);

        return gold.TryGetDouble(out var real) ? (int)Math.Clamp(real, 0, int.MaxValue) : 0;
    }

    private static string? AsText(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Number => value.GetRawText(),
        _ => null
    };

    private static int GetInt(JsonElement element, string name, int fallback = 0) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : fallback;
}

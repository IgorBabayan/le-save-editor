using System.IO;
using System.Text.Json;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

/// <summary>Reads the material storage of a stash file (<see cref="StashSave"/>); everything else in it is ignored.</summary>
public static class StashSaveParser
{
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
        foreach (var (key, quantity) in ReadShards(root).Concat(ReadMaterials(root)))
            amounts[key] = amounts.GetValueOrDefault(key) + quantity;

        return new StashSave
        {
            FilePath = filePath,
            StashType = GetInt(root, "stashType"),
            SoloCharacterId = root.TryGetProperty("soloChallengeCharacterId", out var solo) ? AsText(solo) : null,
            Amounts = amounts,
            ContentHash = CharacterSaveParser.ComputeContentHash(content)
        };
    }

    /// <summary><c>savedShards</c>: <c>{"shardType": affixId, "quantity": n}</c>.</summary>
    internal static IEnumerable<(CraftingMaterialKey Key, int Quantity)> ReadShards(JsonElement root)
    {
        if (!root.TryGetProperty("savedShards", out var shards) || shards.ValueKind != JsonValueKind.Array)
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

    /// <summary>The runes and glyphs of <c>materialsList</c>; other entries are skipped.</summary>
    internal static IEnumerable<(CraftingMaterialKey Key, int Quantity)> ReadMaterials(JsonElement root)
    {
        if (!root.TryGetProperty("materialsList", out var materials) || materials.ValueKind != JsonValueKind.Array)
            yield break;

        foreach (var entry in materials.EnumerateArray())
        {
            if (ReadMaterialKey(entry) is { } key)
                yield return (key, GetInt(entry, "quantity", 1));
        }
    }

    /// <summary>The rune or glyph a <c>materialsList</c> entry holds; <c>null</c> for anything else.</summary>
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

        return CraftingMaterialData.Read(bytes) is { Kind: CraftingMaterialKind.Rune or CraftingMaterialKind.Glyph } material
            ? new CraftingMaterialKey(material.Kind, material.SubTypeId)
            : null;
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

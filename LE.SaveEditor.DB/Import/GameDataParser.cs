using System.Buffers;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LE.SaveEditor.DB.Data;
using LE.SaveEditor.DB.Data.Entities;
using LE.SaveEditor.DB.Icons;
using LE.SaveEditor.DB.Infrastructure;

namespace LE.SaveEditor.DB.Import;

/// <summary>
/// Reads the seven stored nodes out of <c>data.json</c>. Each node may be an object keyed by id
/// or an array; for arrays the key is the first id-like field that is unique over the whole node,
/// so keys stay stable between data versions and updates diff row by row.
/// </summary>
internal static class GameDataParser
{
    private static readonly string[] NameProperties =
        ["name", "displayName", "className", "affixName", "uniqueName", "title", "label"];

    private static readonly JsonWriterOptions CompactWriter = new()
    {
        Indented = false,
        // Keep game text readable in the Json column instead of \uXXXX escapes.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static async Task<ParsedGameData> ParseAsync(string file, CancellationToken ct)
    {
        using var bar = new ConsoleProgressBar("Parsing game data");

        string sourceSha256;
        await using (var hashStream = File.OpenRead(file))
            sourceSha256 = Convert.ToHexString(await SHA256.HashDataAsync(hashStream, ct));

        await using var stream = File.OpenRead(file);
        using var document = await JsonDocument.ParseAsync(stream, new JsonDocumentOptions { MaxDepth = 256 }, ct);

        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("data.json: the root is not a JSON object.");

        var records = new Dictionary<GameNode, IReadOnlyList<GameEntity>>();
        for (var i = 0; i < GameNode.All.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var node = GameNode.All[i];

            // A missing node must stop the run: an update would otherwise delete the whole table.
            if (!document.RootElement.TryGetProperty(node.JsonName, out var element))
                throw new InvalidDataException($"data.json: node \"{node.JsonName}\" was not found.");

            records[node] = ParseNode(node, element);
            bar.Report(i + 1, GameNode.All.Count, $"{node.JsonName} ({records[node].Count})");
        }

        var total = records.Values.Sum(r => r.Count);
        bar.Complete($"{total} records in {records.Count} nodes");

        return new ParsedGameData(sourceSha256, records);
    }

    private static List<GameEntity> ParseNode(GameNode node, JsonElement element)
    {
        var keyed = element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().Select(p => (p.Name, p.Value)).ToList(),
            JsonValueKind.Array => KeyArray(element),
            _ => throw new InvalidDataException(
                $"data.json: node \"{node.JsonName}\" is {element.ValueKind}, expected an object or an array.")
        };

        var result = new List<GameEntity>(keyed.Count);
        foreach (var (key, value) in keyed)
            result.Add(CreateEntity(node, key, value));

        return result;
    }

    /// <summary>Picks a key for every array element (see the class remarks).</summary>
    private static List<(string Key, JsonElement Value)> KeyArray(JsonElement array)
    {
        var items = array.EnumerateArray().ToList();

        if (items.Count > 0 && items.All(i => i.ValueKind == JsonValueKind.String))
        {
            var strings = items.Select(i => i.GetString()!).ToList();
            if (strings.Distinct(StringComparer.Ordinal).Count() == strings.Count)
                return strings.Select((s, i) => (s, items[i])).ToList();
        }

        foreach (var field in CandidateKeyFields(items))
        {
            var keys = new List<string>(items.Count);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in items)
            {
                if (item.ValueKind != JsonValueKind.Object
                    || !item.TryGetProperty(field, out var value)
                    || PrimitiveToString(value) is not { } key
                    || !seen.Add(key))
                {
                    keys.Clear();
                    break;
                }

                keys.Add(key);
            }

            if (keys.Count == items.Count)
                return keys.Select((k, i) => (k, items[i])).ToList();
        }

        // No unique id: fall back to the position in the array.
        return items.Select((item, i) => (i.ToString(CultureInfo.InvariantCulture), item)).ToList();
    }

    /// <summary><c>id</c>, then <c>key</c>, then any <c>…Id</c>/<c>…ID</c> field, in declaration order.</summary>
    private static IEnumerable<string> CandidateKeyFields(List<JsonElement> items)
    {
        var first = items.FirstOrDefault(i => i.ValueKind == JsonValueKind.Object);
        if (first.ValueKind != JsonValueKind.Object)
            yield break;

        var names = first.EnumerateObject().Select(p => p.Name).ToList();

        foreach (var name in names.Where(n => n.Equals("id", StringComparison.OrdinalIgnoreCase)))
            yield return name;

        foreach (var name in names.Where(n => n.Equals("key", StringComparison.OrdinalIgnoreCase)))
            yield return name;

        foreach (var name in names.Where(n => n.Length > 2 && (n.EndsWith("Id", StringComparison.Ordinal) || n.EndsWith("ID", StringComparison.Ordinal))))
            yield return name;
    }

    private static GameEntity CreateEntity(GameNode node, string key, JsonElement value)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer, CompactWriter))
            value.WriteTo(writer);

        var entity = node.Create();
        entity.Key = key;
        entity.Name = FindName(value);
        entity.Json = Encoding.UTF8.GetString(buffer.WrittenSpan);
        entity.Hash = Convert.ToHexString(SHA256.HashData(buffer.WrittenSpan));

        if (entity is Icon icon)
            icon.Path = IconPaths.Extract(key, value);

        return entity;
    }

    private static string? FindName(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.String)
            return value.GetString();

        if (value.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var name in NameProperties)
        {
            foreach (var property in value.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String
                    && property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return property.Value.GetString();
            }
        }

        foreach (var property in value.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String
                && property.Name.EndsWith("Name", StringComparison.OrdinalIgnoreCase))
                return property.Value.GetString();
        }

        return null;
    }

    private static string? PrimitiveToString(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Number => value.GetRawText(),
        _ => null
    };
}

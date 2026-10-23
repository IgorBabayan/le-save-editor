using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

public sealed class CharacterSaveParser : ICharacterSaveParser
{
    /// <summary>Marker the game writes in front of the JSON body.</summary>
    public const string EpochPrefix = "EPOCH";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public async Task<CharacterSave> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var content = await File.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
        return Parse(filePath, content);
    }

    public CharacterSave Parse(string filePath, string content)
    {
        var json = StripEpochPrefix(content);
        var dto = JsonSerializer.Deserialize<CharacterSaveDto>(json, SerializerOptions)
                  ?? throw new JsonException("The save file is empty.");

        return new CharacterSave
        {
            FilePath = filePath,
            CharacterName = dto.CharacterName ?? string.Empty,
            Level = dto.Level,
            Slot = dto.Slot,
            Hardcore = dto.Hardcore,
            CharacterClass = (CharacterClass)dto.CharacterClass,
            ChosenMastery = dto.ChosenMastery,
            LastPlayed = dto.LastPlayed,
            SavedItems = ReadSavedItems(dto.SavedItems)
        };
    }

    /// <summary>
    /// Reads <c>savedItems</c> leniently: an entry the editor does not understand is skipped instead of making
    /// the whole save unreadable.
    /// </summary>
    private static IReadOnlyList<SavedItem> ReadSavedItems(JsonElement savedItems)
    {
        if (savedItems.ValueKind != JsonValueKind.Array)
            return [];

        var result = new List<SavedItem>();
        foreach (var entry in savedItems.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.Object
                || !entry.TryGetProperty("data", out var data)
                || data.ValueKind != JsonValueKind.Array
                || ReadBytes(data) is not { } bytes)
                continue;

            int x = 0, y = 0;
            if (entry.TryGetProperty("inventoryPosition", out var position) && position.ValueKind == JsonValueKind.Object)
            {
                x = GetInt(position, "x");
                y = GetInt(position, "y");
            }

            result.Add(new SavedItem(
                GetInt(entry, "containerID"),
                x,
                y,
                GetInt(entry, "quantity", 1),
                GetInt(entry, "formatVersion"),
                bytes));
        }

        return result;
    }

    private static byte[]? ReadBytes(JsonElement array)
    {
        var bytes = new byte[array.GetArrayLength()];
        var i = 0;
        foreach (var value in array.EnumerateArray())
        {
            if (value.ValueKind != JsonValueKind.Number || !value.TryGetByte(out var b))
                return null;
            bytes[i++] = b;
        }

        return bytes;
    }

    private static int GetInt(JsonElement element, string name, int fallback = 0) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : fallback;

    /// <summary>Removes the leading <c>EPOCH</c> word (and surrounding whitespace / BOM) so the rest is plain JSON.</summary>
    public static string StripEpochPrefix(string content)
    {
        var text = content.AsSpan().TrimStart('﻿').TrimStart();
        if (text.StartsWith(EpochPrefix, StringComparison.Ordinal))
            text = text[EpochPrefix.Length..].TrimStart();

        return text.ToString();
    }

    /// <summary>Only the fields the editor currently needs; everything else in the file is ignored.</summary>
    private sealed class CharacterSaveDto
    {
        [JsonPropertyName("characterName")] public string? CharacterName { get; init; }
        [JsonPropertyName("level")] public int Level { get; init; }
        [JsonPropertyName("slot")] public int Slot { get; init; }
        [JsonPropertyName("hardcore")] public bool Hardcore { get; init; }
        [JsonPropertyName("characterClass")] public int CharacterClass { get; init; }
        [JsonPropertyName("chosenMastery")] public int ChosenMastery { get; init; }
        /// <summary>Display only, so a format the game changed must never make the whole save unreadable.</summary>
        [JsonPropertyName("lastPlayed")]
        [JsonConverter(typeof(LenientDateTimeOffsetConverter))]
        public DateTimeOffset? LastPlayed { get; init; }

        /// <summary>Kept as raw JSON and read by <see cref="ReadSavedItems"/>, so a bad item never fails the save.</summary>
        [JsonPropertyName("savedItems")] public JsonElement SavedItems { get; init; }
    }
}

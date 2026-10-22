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
            LastPlayed = dto.LastPlayed
        };
    }

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
        [JsonPropertyName("lastPlayed")] public DateTimeOffset? LastPlayed { get; init; }
    }
}

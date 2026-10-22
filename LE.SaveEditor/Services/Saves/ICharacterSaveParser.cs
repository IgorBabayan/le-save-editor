using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

/// <summary>Reads <c>1CHARACTERSLOT_*</c> files.</summary>
public interface ICharacterSaveParser
{
    /// <exception cref="System.IO.IOException">The file cannot be read.</exception>
    /// <exception cref="System.Text.Json.JsonException">The content is not a valid character save.</exception>
    Task<CharacterSave> ReadAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>Parses file content (with or without the leading <c>EPOCH</c> marker).</summary>
    CharacterSave Parse(string filePath, string content);
}

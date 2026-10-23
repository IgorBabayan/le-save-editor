using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

/// <summary>Writes changes made in the editor back into a <c>1CHARACTERSLOT_*</c> file.</summary>
public interface ICharacterSaveWriter
{
    /// <summary>
    /// Applies <paramref name="updates"/> to the <c>savedItems</c> of <paramref name="character"/>'s file and writes it,
    /// keeping everything else in the file as it is. The previous file is kept as a <c>.bak</c> backup next to it.
    /// </summary>
    /// <returns>The character as read back from the written file, and the path of the backup.</returns>
    /// <exception cref="SaveFileChangedException">The file changed after it was read (e.g. the game saved it).</exception>
    /// <exception cref="System.IO.IOException">The file cannot be read or written.</exception>
    /// <exception cref="UnauthorizedAccessException">The file or its folder is read-only.</exception>
    /// <exception cref="System.Text.Json.JsonException">The file is not a valid character save.</exception>
    Task<CharacterSaveResult> SaveAsync(
        CharacterSave character,
        IReadOnlyCollection<SavedItemUpdate> updates,
        CancellationToken cancellationToken = default);
}

/// <summary>Outcome of <see cref="ICharacterSaveWriter.SaveAsync"/>.</summary>
/// <param name="Character">The character read back from the written file.</param>
/// <param name="BackupPath">Copy of the file as it was before saving.</param>
public sealed record CharacterSaveResult(CharacterSave Character, string BackupPath);

/// <summary>The save file no longer holds what the editor read, so writing it would lose the newer content.</summary>
public sealed class SaveFileChangedException(string filePath)
    : System.IO.IOException($"{System.IO.Path.GetFileName(filePath)} was changed after it was loaded (is Last Epoch running?). Reload the save folder and make the changes again.")
{
    public string FilePath { get; } = filePath;
}

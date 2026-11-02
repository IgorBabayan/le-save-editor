using System.IO;
using System.Text.Json;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

/// <summary>Reads the characters and the material storage of the stashes in a save folder.</summary>
public sealed class SaveFolderLoader(ISaveFolderScanner scanner, ICharacterSaveParser parser) : ISaveFolderLoader
{
    public async Task<SaveFolderContent> LoadAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            return SaveFolderContent.Empty(folderPath);

        // Directory enumeration is synchronous I/O; keep it off the UI thread.
        var files = await Task.Run(() => scanner.Scan(folderPath), cancellationToken).ConfigureAwait(false);

        var characters = new List<CharacterSave>(files.CharacterFiles.Count);
        var errors = new List<SaveFileError>();

        foreach (var file in files.CharacterFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                characters.Add(await parser.ReadAsync(file, cancellationToken).ConfigureAwait(false));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                errors.Add(new SaveFileError(file, ex.Message));
            }
        }

        characters.Sort((a, b) => a.Slot.CompareTo(b.Slot));

        // The material storage ("Crafting Materials") of each stash; tab files hold only stash items.
        var stashes = new List<StashSave>();
        foreach (var file in files.StashFiles.Where(StashLocator.IsMainStashFile))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                stashes.Add(await StashSaveParser.ReadAsync(file, cancellationToken).ConfigureAwait(false));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                errors.Add(new SaveFileError(file, ex.Message));
            }
        }

        return new SaveFolderContent(folderPath, characters, files.StashFiles, errors, stashes);
    }
}

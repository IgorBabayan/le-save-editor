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

        // Every character and stash file is read and parsed at the same time (each one is independent), instead of
        // one after another: loading a folder with many characters no longer waits for each file in turn.
        var characterReads = files.CharacterFiles
            .Select(file => ReadAsync(file, f => parser.ReadAsync(f, cancellationToken), cancellationToken))
            .ToList();
        // The material storage ("Crafting Materials") of each stash; tab files hold only stash items.
        var stashReads = files.StashFiles
            .Where(StashLocator.IsMainStashFile)
            .Select(file => ReadAsync(file, f => StashSaveParser.ReadAsync(f, cancellationToken), cancellationToken))
            .ToList();

        var characterResults = await Task.WhenAll(characterReads).ConfigureAwait(false);
        var stashResults = await Task.WhenAll(stashReads).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        var characters = characterResults.Where(r => r.Value is not null).Select(r => r.Value!).ToList();
        characters.Sort((a, b) => a.Slot.CompareTo(b.Slot));

        // Errors in file order, as before (characters first, then stashes).
        var errors = characterResults.Where(r => r.Error is not null).Select(r => r.Error!)
            .Concat(stashResults.Where(r => r.Error is not null).Select(r => r.Error!))
            .ToList();
        var stashes = stashResults.Where(r => r.Value is not null).Select(r => r.Value!).ToList();

        return new SaveFolderContent(folderPath, characters, files.StashFiles, errors, stashes);
    }

    /// <summary>Reads one file on the thread pool; a file that cannot be read becomes an error instead of failing the load.</summary>
    private static Task<(T? Value, SaveFileError? Error)> ReadAsync<T>(
        string file,
        Func<string, Task<T>> read,
        CancellationToken cancellationToken) where T : class =>
        Task.Run<(T? Value, SaveFileError? Error)>(async () =>
        {
            try
            {
                return ((T?)await read(file).ConfigureAwait(false), (SaveFileError?)null);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                return (null, new SaveFileError(file, ex.Message));
            }
        }, cancellationToken);
}

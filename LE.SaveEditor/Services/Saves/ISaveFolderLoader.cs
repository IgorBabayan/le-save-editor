using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

/// <summary>Scans a save folder and parses every character file in it.</summary>
public interface ISaveFolderLoader
{
    /// <summary>Never throws for individual bad files; they are reported in <see cref="SaveFolderContent.Errors"/>.</summary>
    Task<SaveFolderContent> LoadAsync(string folderPath, CancellationToken cancellationToken = default);
}

namespace LE.SaveEditor.Services.Saves.Models;

/// <summary>Save files found in a folder, split by category (paths only, nothing parsed yet).</summary>
public sealed record SaveFolderFiles(IReadOnlyList<string> CharacterFiles, IReadOnlyList<string> StashFiles)
{
    public static SaveFolderFiles Empty { get; } = new([], []);
}

/// <summary>A save file that could not be read or parsed.</summary>
public sealed record SaveFileError(string FilePath, string Message);

/// <summary>Result of loading a save folder.</summary>
public sealed record SaveFolderContent(
    string FolderPath,
    IReadOnlyList<CharacterSave> Characters,
    IReadOnlyList<string> StashFiles,
    IReadOnlyList<SaveFileError> Errors)
{
    public static SaveFolderContent Empty(string folderPath) => new(folderPath, [], [], []);
}

using System.IO;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

public sealed class SaveFolderScanner : ISaveFolderScanner
{
    public const string CharacterPrefix = "1CHARACTERSLOT_";
    public const string StashPrefix = "STASH_";

    public SaveFolderFiles Scan(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            return SaveFolderFiles.Empty;

        var characters = new List<string>();
        var stashes = new List<string>();

        foreach (var path in Directory.EnumerateFiles(folderPath, "*", SearchOption.TopDirectoryOnly))
        {
            var fileName = Path.GetFileName(path);
            if (IsIgnored(fileName))
                continue;

            if (fileName.StartsWith(CharacterPrefix, StringComparison.OrdinalIgnoreCase))
                characters.Add(path);
            else if (fileName.StartsWith(StashPrefix, StringComparison.OrdinalIgnoreCase))
                stashes.Add(path);
        }

        characters.Sort(StringComparer.OrdinalIgnoreCase);
        stashes.Sort(StringComparer.OrdinalIgnoreCase);
        return new SaveFolderFiles(characters, stashes);
    }

    /// <summary>Backups and copies made by the game or by the user: <c>*.bak</c>, <c>* - Copy*</c>, <c>*_temp*</c>.</summary>
    public static bool IsIgnored(string fileName) =>
        fileName.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)
        || fileName.Contains(" - Copy", StringComparison.OrdinalIgnoreCase)
        || fileName.Contains("_temp", StringComparison.OrdinalIgnoreCase);
}

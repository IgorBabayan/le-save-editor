using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.Saves;

/// <summary>Finds save files in a folder and splits them into characters and stashes.</summary>
public interface ISaveFolderScanner
{
    SaveFolderFiles Scan(string folderPath);
}

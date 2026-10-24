using System.IO;

namespace LE.SaveEditor.Services.Settings;

public static class LastEpochPaths
{
    /// <summary>
    /// Default location of offline saves:
    /// <c>%USERPROFILE%\AppData\LocalLow\Eleventh Hour Games\Last Epoch\Saves</c>.
    /// </summary>
    public static string DefaultSaveFolder
    {
        get
        {
            // There is no SpecialFolder for LocalLow; it is a sibling of %LOCALAPPDATA%.
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appData = Path.GetDirectoryName(local.TrimEnd(Path.DirectorySeparatorChar)) ?? local;
            return Path.Combine(appData, "LocalLow", "Eleventh Hour Games", "Last Epoch", "Saves");
        }
    }
}

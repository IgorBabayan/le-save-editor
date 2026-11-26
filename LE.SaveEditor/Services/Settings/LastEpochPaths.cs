using System.IO;

namespace LE.SaveEditor.Services.Settings;

public static class LastEpochPaths
{
    private const string SteamAppId = "899770";

    /// <summary>
    /// Default location of offline saves (the first one that exists, else the platform's usual one):
    /// <list type="bullet">
    /// <item>Windows: <c>%USERPROFILE%\AppData\LocalLow\Eleventh Hour Games\Last Epoch\Saves</c>.</item>
    /// <item>Linux: the native game's <c>~/.config/unity3d/Eleventh Hour Games/Last Epoch/Saves</c>, or the Windows
    /// version run by Steam Proton (<c>compatdata/899770</c>).</item>
    /// <item>macOS: <c>~/Library/Application Support/Eleventh Hour Games/Last Epoch/Saves</c>.</item>
    /// </list>
    /// </summary>
    public static string DefaultSaveFolder
    {
        get
        {
            var candidates = Candidates().ToList();
            return candidates.FirstOrDefault(Directory.Exists) ?? candidates[0];
        }
    }

    private static IEnumerable<string> Candidates()
    {
        if (OperatingSystem.IsWindows())
        {
            // There is no SpecialFolder for LocalLow; it is a sibling of %LOCALAPPDATA%.
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appData = Path.GetDirectoryName(local.TrimEnd(Path.DirectorySeparatorChar)) ?? local;
            yield return Path.Combine(appData, "LocalLow", "Eleventh Hour Games", "Last Epoch", "Saves");
            yield break;
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (OperatingSystem.IsMacOS())
        {
            yield return Path.Combine(home, "Library", "Application Support", "Eleventh Hour Games", "Last Epoch", "Saves");
            yield return Path.Combine(home, "Library", "Application Support", "com.EleventhHourGames.LastEpoch", "Saves");
            yield break;
        }

        // Linux (and other Unix systems): the native build stores its data under XDG_CONFIG_HOME.
        var config = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        if (string.IsNullOrWhiteSpace(config))
            config = Path.Combine(home, ".config");
        yield return Path.Combine(config, "unity3d", "Eleventh Hour Games", "Last Epoch", "Saves");

        // The Windows build under Steam Proton keeps the Windows layout inside its prefix.
        const string protonSaves = "pfx/drive_c/users/steamuser/AppData/LocalLow/Eleventh Hour Games/Last Epoch/Saves";
        foreach (var steam in new[] { ".local/share/Steam", ".steam/steam", ".var/app/com.valvesoftware.Steam/.local/share/Steam" })
            yield return Path.Combine(home, steam, "steamapps", "compatdata", SteamAppId, protonSaves);
    }
}

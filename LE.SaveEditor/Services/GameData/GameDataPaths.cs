using System.IO;

namespace LE.SaveEditor.Services.GameData;

/// <summary>
/// Game data produced by <c>LE.SaveEditor.DB</c> into the editor's config folder
/// <c>%APPDATA%\LE.SaveEditor</c> (the same folder as <c>settings.json</c>).
/// </summary>
public static class GameDataPaths
{
    public static string ConfigDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LE.SaveEditor");

    /// <summary>SQLite database with classes, affixes, item types/categories, uniques, stats and icons.</summary>
    public static string DatabaseFile { get; } = Path.Combine(ConfigDirectory, "base.db");

    /// <summary>PNG icons, e.g. <c>Icons/uniques/spirit_xylem.png</c>.</summary>
    public static string IconsDirectory { get; } = Path.Combine(ConfigDirectory, "Icons");
}

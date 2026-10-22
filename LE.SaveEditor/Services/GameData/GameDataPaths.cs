using System.IO;

namespace LE.SaveEditor.Services.GameData;

/// <summary>
/// Game data produced by <c>LE.SaveEditor.DB</c> into <c>LE.SaveEditor/Config</c>
/// and copied next to the exe on build/publish.
/// </summary>
public static class GameDataPaths
{
    public static string ConfigDirectory { get; } = Path.Combine(AppContext.BaseDirectory, "Config");

    /// <summary>SQLite database with classes, affixes, item types/categories, uniques, stats and icons.</summary>
    public static string DatabaseFile { get; } = Path.Combine(ConfigDirectory, "base.db");

    /// <summary>PNG icons, e.g. <c>Icons/uniques/spirit_xylem.png</c>.</summary>
    public static string IconsDirectory { get; } = Path.Combine(ConfigDirectory, "Icons");
}

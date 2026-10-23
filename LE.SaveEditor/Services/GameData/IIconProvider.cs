using System.Windows.Media;

namespace LE.SaveEditor.Services.GameData;

/// <summary>Loads game icons from <c>%APPDATA%\LE.SaveEditor\Icons</c>.</summary>
public interface IIconProvider
{
    /// <summary>
    /// Icon by its path from the <c>Icons</c> table (e.g. <c>uniques/spirit_xylem</c>),
    /// or <c>null</c> when the image is missing. The result is frozen and cached, so it can be
    /// bound from any thread and shared by any number of controls.
    /// </summary>
    ImageSource? Get(string? iconPath);

    /// <summary>Forgets loaded (and missing) icons, so files written by the generator are picked up.</summary>
    void ClearCache();
}

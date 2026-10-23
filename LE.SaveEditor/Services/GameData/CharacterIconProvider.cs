using System.IO;
using System.Windows.Media;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.GameData;

/// <summary>
/// Finds class / mastery icons among the PNGs <c>LE.SaveEditor.DB</c> downloaded into <c>Icons</c>.
/// </summary>
/// <remarks>
/// The editor does not read <c>base.db</c>, and the icon group of classes and masteries is whatever maxroll
/// calls it, so the folder is indexed by file name instead: <c>void_knight.png</c>, <c>VoidKnight.png</c> and
/// <c>void-knight_icon.png</c> all match <see cref="Mastery.VoidKnight"/>. When several files match, one in a
/// folder named after classes / masteries wins over e.g. an ability icon with the same name.
/// </remarks>
public sealed class CharacterIconProvider(IIconProvider iconProvider) : ICharacterIconProvider
{
    /// <summary>Extra words an icon file name may carry around the class / mastery name.</summary>
    private static readonly string[] CommonAffixes = ["icon", "portrait", "emblem", "symbol"];

    private readonly object _sync = new();

    /// <summary>Normalized file name → icon paths (<c>group/name</c>, no extension); built on first use.</summary>
    private Dictionary<string, List<string>>? _index;

    public ImageSource? Get(CharacterClass characterClass, Mastery mastery)
    {
        // A character with a mastery shows the mastery; without one (or if its image is missing) the base class.
        if (mastery is not (Mastery.None or Mastery.Unknown))
        {
            var masteryIcon = iconProvider.Get(FindIconPath(mastery.ToString(), "master"));
            if (masteryIcon is not null)
                return masteryIcon;
        }

        return CharacterClassCatalog.IsKnown(characterClass)
            ? iconProvider.Get(FindIconPath(characterClass.ToString(), "class"))
            : null;
    }

    public void ClearCache()
    {
        lock (_sync)
            _index = null;
    }

    /// <param name="name">Enum name, e.g. <c>VoidKnight</c>.</param>
    /// <param name="kindWord">Word in a preferred folder or file name (<c>class</c> / <c>master</c>).</param>
    private string? FindIconPath(string name, string kindWord)
    {
        var index = GetIndex();
        if (index.Count == 0)
            return null;

        var key = Normalize(name);
        var kindPrefix = kindWord == "class" ? "class" : "mastery";

        // Rank 0 = plain name, then the name with a class / mastery word, then with a generic word.
        var candidates = new List<(string Key, int Rank)> { (key, 0), (kindPrefix + key, 1), (key + kindPrefix, 1) };
        candidates.AddRange(CommonAffixes.SelectMany(a => new[] { (a + key, 2), (key + a, 2) }));

        return candidates
            .Where(c => index.ContainsKey(c.Key))
            .SelectMany(c => index[c.Key].Select(path => (Path: path, c.Rank)))
            .OrderBy(m => FolderRank(m.Path, kindWord))
            .ThenBy(m => m.Rank)
            .ThenBy(m => m.Path.Length)
            .ThenBy(m => m.Path, StringComparer.OrdinalIgnoreCase)
            .Select(m => m.Path)
            .FirstOrDefault();
    }

    /// <summary>0: folder named after the wanted kind; 1: after classes or masteries in general; 2: anything else.</summary>
    private static int FolderRank(string iconPath, string kindWord)
    {
        var separator = iconPath.LastIndexOf('/');
        var folder = separator < 0 ? string.Empty : iconPath[..separator].ToLowerInvariant();

        if (folder.Contains(kindWord))
            return 0;
        return folder.Contains("class") || folder.Contains("master") ? 1 : 2;
    }

    private Dictionary<string, List<string>> GetIndex()
    {
        lock (_sync)
            return _index ??= BuildIndex();
    }

    private static Dictionary<string, List<string>> BuildIndex()
    {
        var index = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var root = GameDataPaths.IconsDirectory;
        if (!Directory.Exists(root))
            return index;

        try
        {
            foreach (var file in Directory.EnumerateFiles(root, "*.png", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(root, file);
                var iconPath = Path.ChangeExtension(relative, null).Replace(Path.DirectorySeparatorChar, '/');
                var key = Normalize(Path.GetFileNameWithoutExtension(file));
                if (key.Length == 0)
                    continue;

                if (!index.TryGetValue(key, out var paths))
                    index[key] = paths = [];
                paths.Add(iconPath);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // A folder that cannot be read simply means no character icons; never break the list.
        }

        return index;
    }

    /// <summary><c>Void_Knight</c>, <c>void-knight</c>, <c>VoidKnight</c> → <c>voidknight</c>.</summary>
    private static string Normalize(string value) =>
        new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}

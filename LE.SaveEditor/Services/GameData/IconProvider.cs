using System.Collections.Concurrent;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LE.SaveEditor.Services.GameData;

public sealed class IconProvider : IIconProvider
{
    private readonly ConcurrentDictionary<string, ImageSource?> _cache = new(StringComparer.OrdinalIgnoreCase);

    public ImageSource? Get(string? iconPath)
    {
        var path = Normalize(iconPath);
        return path is null ? null : _cache.GetOrAdd(path, Load);
    }

    public void ClearCache() => _cache.Clear();

    private static ImageSource? Load(string iconPath)
    {
        var file = Path.Combine(GameDataPaths.IconsDirectory, iconPath.Replace('/', Path.DirectorySeparatorChar) + ".png");
        if (!File.Exists(file))
            return null;

        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(file, UriKind.Absolute);
            // Read the file now and release it, so the generator can replace icons while the editor runs.
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch (Exception ex) when (ex is IOException or NotSupportedException or UnauthorizedAccessException or FileFormatException)
        {
            // A broken icon must never break a view; it simply shows nothing.
            return null;
        }
    }

    private static string? Normalize(string? iconPath)
    {
        if (string.IsNullOrWhiteSpace(iconPath))
            return null;

        var path = iconPath.Trim().Replace('\\', '/').TrimStart('/');
        foreach (var extension in new[] { ".png", ".webp" })
        {
            if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                return path[..^extension.Length];
        }

        return path;
    }
}

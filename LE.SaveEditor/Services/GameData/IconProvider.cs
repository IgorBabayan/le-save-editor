using System.Collections.Concurrent;
using System.IO;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace LE.SaveEditor.Services.GameData;

public sealed class IconProvider : IIconProvider
{
    private readonly ConcurrentDictionary<string, IImage?> _cache = new(StringComparer.OrdinalIgnoreCase);

    private static readonly string[] Extensions = [".png", ".webp"];

    public IImage? Get(string? iconPath)
    {
        var path = Normalize(iconPath);
        return path is null ? null : _cache.GetOrAdd(path, Load);
    }

    public void ClearCache() => _cache.Clear();

    private static IImage? Load(string iconPath)
    {
        var file = Path.Combine(GameDataPaths.IconsDirectory, iconPath.Replace('/', Path.DirectorySeparatorChar) + ".png");
        if (!File.Exists(file))
            return null;

        try
        {
            // Read the file now and release it, so the generator can replace icons while the editor runs.
            using var stream = File.OpenRead(file);
            return new Bitmap(stream);
        }
        catch (Exception ex) when (ex is IOException or NotSupportedException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
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
        foreach (var extension in Extensions)
        {
            if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                return path[..^extension.Length];
        }

        return path;
    }
}

using System.Text.Json;

namespace LE.SaveEditor.DB.Icons;

/// <summary>
/// Turns entries of the <c>icons</c> node into asset paths such as <c>uniques/spirit_xylem</c>,
/// downloaded from <c>https://assets-ng.maxroll.gg/last-epoch/webp/uniques/spirit_xylem.webp</c>.
/// </summary>
internal static class IconPaths
{
    public const string BaseUrl = "https://assets-ng.maxroll.gg/last-epoch/webp/";

    private static readonly string[] PathProperties = ["path", "src", "url", "file", "icon", "img", "image", "sprite"];
    private static readonly string[] KnownExtensions = [".webp", ".png", ".jpg", ".jpeg"];

    /// <summary>
    /// Finds the asset path of one <c>icons</c> entry. Handles a plain string value,
    /// an object with a path-like property, any string value containing <c>/</c>,
    /// or a key that is itself a path. Returns <c>null</c> when nothing looks like a path.
    /// </summary>
    public static string? Extract(string key, JsonElement element)
    {
        var raw = element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Object => FindInObject(element),
            _ => null
        };

        if (string.IsNullOrWhiteSpace(raw) && key.Contains('/'))
            raw = key;

        return Normalize(raw);
    }

    /// <summary>Download URL of an icon path.</summary>
    public static string ToUrl(string path) => BaseUrl + path + ".webp";

    /// <summary>Local file of an icon path (without extension) under <paramref name="iconsDirectory"/>.</summary>
    public static string ToLocalFile(string iconsDirectory, string path, string extension) =>
        Path.Combine(iconsDirectory, path.Replace('/', Path.DirectorySeparatorChar) + extension);

    /// <summary><c>/last-epoch/webp/uniques/Spirit_Xylem.webp</c> → <c>uniques/Spirit_Xylem</c>.</summary>
    public static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var path = raw.Trim().Replace('\\', '/');

        if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https")
            path = uri.AbsolutePath;

        path = path.TrimStart('/');

        const string assetRoot = "last-epoch/webp/";
        if (path.StartsWith(assetRoot, StringComparison.OrdinalIgnoreCase))
            path = path[assetRoot.Length..];

        foreach (var extension in KnownExtensions)
        {
            if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                path = path[..^extension.Length];
                break;
            }
        }

        // Never let a path escape the Icons folder or contain characters Windows cannot store.
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || segments.Any(s => s is "." or ".." || s.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
            return null;

        return string.Join('/', segments);
    }

    private static string? FindInObject(JsonElement element)
    {
        foreach (var name in PathProperties)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String
                    && property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return property.Value.GetString();
            }
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String && property.Value.GetString()!.Contains('/'))
                return property.Value.GetString();
        }

        return null;
    }
}

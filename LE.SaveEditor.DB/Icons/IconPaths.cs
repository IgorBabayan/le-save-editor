namespace LE.SaveEditor.DB.Icons;

/// <summary>
/// Icon paths such as <c>uniques/spirit_xylem</c> (group / name from the <c>icons</c> node),
/// downloaded from <c>https://assets-ng.maxroll.gg/last-epoch/webp/uniques/spirit_xylem.webp</c>.
/// </summary>
internal static class IconPaths
{
    public const string BaseUrl = "https://assets-ng.maxroll.gg/last-epoch/webp/";

    private static readonly string[] KnownExtensions = [".webp", ".png", ".jpg", ".jpeg"];

    /// <summary>Download URL of an icon path; segments are escaped (some names contain spaces).</summary>
    public static string ToUrl(string path) =>
        BaseUrl + string.Join('/', path.Split('/').Select(Uri.EscapeDataString)) + ".webp";

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
}

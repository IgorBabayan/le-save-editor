using System.Collections.Concurrent;
using System.Net;
using LE.SaveEditor.DB.Data.Entities;
using LE.SaveEditor.DB.Infrastructure;
using SkiaSharp;

namespace LE.SaveEditor.DB.Icons;

/// <summary>
/// Keeps <c>%APPDATA%\LE.SaveEditor\Icons</c> in line with the <c>Icons</c> table: downloads the WebP images,
/// converts them to PNG (WPF cannot decode WebP without an optional Windows codec) and
/// deletes the WebP files and the PNGs of icons that no longer exist.
/// </summary>
internal sealed class IconSynchronizer(HttpClient http, ConfigPaths paths)
{
    private const int MaxParallelDownloads = 8;
    private const int MaxAttempts = 3;

    /// <param name="icons">All icons now in the database.</param>
    /// <param name="changedKeys">Icons to download even if their PNG exists; <c>null</c> rebuilds the whole folder.</param>
    public async Task<IconSyncResult> SyncAsync(IReadOnlyList<Icon> icons, IReadOnlySet<string>? changedKeys, CancellationToken ct)
    {
        var root = paths.IconsDirectory;
        if (changedKeys is null && Directory.Exists(root))
            Directory.Delete(root, recursive: true);
        Directory.CreateDirectory(root);

        // Several icon entries may point to one image.
        var wanted = icons
            .Where(i => i.Path is not null)
            .GroupBy(i => i.Path!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => changedKeys is null || g.Any(i => changedKeys.Contains(i.Key)),
                StringComparer.OrdinalIgnoreCase);

        var withoutPath = icons.Count(i => i.Path is null);
        var removed = RemoveStaleFiles(root, wanted.Keys);

        var toDownload = wanted
            .Where(w => w.Value || !File.Exists(IconPaths.ToLocalFile(root, w.Key, ".png")))
            .Select(w => w.Key)
            .ToList();

        var failures = new ConcurrentBag<string>();
        await DownloadAsync(toDownload, failures, ct);
        var downloaded = toDownload.Count - failures.Count;
        var converted = ConvertAll(failures, ct);

        return new IconSyncResult(wanted.Count, downloaded, converted, removed, withoutPath, failures.ToList());
    }

    private async Task DownloadAsync(List<string> iconPaths, ConcurrentBag<string> failures, CancellationToken ct)
    {
        using var bar = new ConsoleProgressBar("Downloading icons");
        var done = 0;

        await Parallel.ForEachAsync(
            iconPaths,
            new ParallelOptions { MaxDegreeOfParallelism = MaxParallelDownloads, CancellationToken = ct },
            async (iconPath, token) =>
            {
                var error = await DownloadOneAsync(iconPath, token);
                if (error is not null)
                    failures.Add($"download {iconPath}: {error}");

                var count = Interlocked.Increment(ref done);
                bar.Report(count, iconPaths.Count, $"{count}/{iconPaths.Count} {iconPath}");
            });

        bar.Complete(iconPaths.Count == 0
            ? "all icons are up to date"
            : $"{iconPaths.Count - failures.Count} of {iconPaths.Count} downloaded");
    }

    /// <returns><c>null</c> on success, otherwise the reason it failed.</returns>
    private async Task<string?> DownloadOneAsync(string iconPath, CancellationToken ct)
    {
        var url = IconPaths.ToUrl(iconPath);
        var file = IconPaths.ToLocalFile(paths.IconsDirectory, iconPath, ".webp");

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden)
                    return $"{(int)response.StatusCode} {url}";

                response.EnsureSuccessStatusCode();

                Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                await using (var target = File.Create(file))
                    await response.Content.CopyToAsync(target, ct);

                return null;
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException or TaskCanceledException
                                       && !ct.IsCancellationRequested)
            {
                TryDelete(file);
                if (attempt >= MaxAttempts)
                    return ex.Message;

                await Task.Delay(TimeSpan.FromMilliseconds(500 * attempt), ct);
            }
        }
    }

    /// <summary>Converts every <c>.webp</c> under <c>Icons</c> to <c>.png</c> and deletes the <c>.webp</c>.</summary>
    private int ConvertAll(ConcurrentBag<string> failures, CancellationToken ct)
    {
        var files = Directory.EnumerateFiles(paths.IconsDirectory, "*.webp", SearchOption.AllDirectories).ToList();

        using var bar = new ConsoleProgressBar("Converting icons to PNG");
        var done = 0;
        var converted = 0;

        Parallel.ForEach(
            files,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = ct },
            file =>
            {
                try
                {
                    ConvertToPng(file);
                    Interlocked.Increment(ref converted);
                }
                catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException)
                {
                    failures.Add($"convert {Path.GetRelativePath(paths.IconsDirectory, file)}: {ex.Message}");
                }
                finally
                {
                    // The .webp is removed either way: a broken file would only be retried forever.
                    TryDelete(file);
                }

                var count = Interlocked.Increment(ref done);
                bar.Report(count, files.Count, $"{count}/{files.Count}");
            });

        bar.Complete(files.Count == 0 ? "nothing to convert" : $"{converted} of {files.Count} converted");
        return converted;
    }

    private static void ConvertToPng(string webpFile)
    {
        using var bitmap = SKBitmap.Decode(webpFile)
                           ?? throw new InvalidDataException("the file is not a readable image");
        using var image = SKImage.FromBitmap(bitmap);
        using var png = image.Encode(SKEncodedImageFormat.Png, 100)
                        ?? throw new InvalidDataException("PNG encoding failed");

        var pngFile = Path.ChangeExtension(webpFile, ".png");
        using var target = File.Create(pngFile);
        png.SaveTo(target);
    }

    /// <summary>Deletes PNGs whose icon is gone and leftovers of interrupted runs.</summary>
    private static int RemoveStaleFiles(string root, IEnumerable<string> wantedPaths)
    {
        var wanted = new HashSet<string>(
            wantedPaths.Select(p => IconPaths.ToLocalFile(root, p, ".png")),
            StringComparer.OrdinalIgnoreCase);

        var removed = 0;
        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).ToList())
        {
            if (!wanted.Contains(file))
            {
                TryDelete(file);
                removed++;
            }
        }

        foreach (var dir in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories)
                     .OrderByDescending(d => d.Length).ToList())
        {
            if (!Directory.EnumerateFileSystemEntries(dir).Any())
                Directory.Delete(dir);
        }

        return removed;
    }

    private static void TryDelete(string file)
    {
        try
        {
            File.Delete(file);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}

internal sealed record IconSyncResult(
    int Total,
    int Downloaded,
    int Converted,
    int Removed,
    int WithoutPath,
    IReadOnlyList<string> Failures);

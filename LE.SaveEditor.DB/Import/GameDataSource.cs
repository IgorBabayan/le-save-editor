using LE.SaveEditor.DB.Infrastructure;

namespace LE.SaveEditor.DB.Import;

/// <summary>Downloads the planner's game data from maxroll.</summary>
internal static class GameDataSource
{
    public const string Url = "https://assets-ng.maxroll.gg/leplanner/game/data.json";

    /// <summary>Downloads <see cref="Url"/> into a temp file and returns its path (the caller deletes it).</summary>
    public static async Task<string> DownloadAsync(HttpClient http, CancellationToken ct)
    {
        using var bar = new ConsoleProgressBar("Downloading game data");

        // The shared client does not request gzip, so Content-Length is the real size
        // and the percentage is accurate.
        using var response = await http.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var total = response.Content.Headers.ContentLength ?? -1;
        var file = Path.Combine(Path.GetTempPath(), $"le-game-data-{Guid.NewGuid():N}.json");

        try
        {
            await using var source = await response.Content.ReadAsStreamAsync(ct);
            await using var target = File.Create(file);

            var buffer = new byte[81920];
            long received = 0;
            int read;
            while ((read = await source.ReadAsync(buffer, ct)) > 0)
            {
                await target.WriteAsync(buffer.AsMemory(0, read), ct);
                received += read;

                var detail = total > 0
                    ? $"{ConsoleProgressBar.FormatBytes(received)} / {ConsoleProgressBar.FormatBytes(total)}"
                    : ConsoleProgressBar.FormatBytes(received);
                bar.Report(received, total, detail);
            }

            bar.Complete(ConsoleProgressBar.FormatBytes(received));
            return file;
        }
        catch
        {
            TryDelete(file);
            throw;
        }
    }

    public static void TryDelete(string file)
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

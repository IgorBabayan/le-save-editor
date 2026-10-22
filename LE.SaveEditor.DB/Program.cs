using System.Net;
using System.Text;
using LE.SaveEditor.DB.Icons;
using LE.SaveEditor.DB.Import;
using LE.SaveEditor.DB.Infrastructure;

namespace LE.SaveEditor.DB;

/// <summary>
/// Generator of <c>LE.SaveEditor/Config/base.db</c> and <c>LE.SaveEditor/Config/Icons</c>
/// from maxroll's Last Epoch planner data.
/// Usage: <c>LE.SaveEditor.DB [--config &lt;folder&gt;]</c>.
/// </summary>
internal static class Program
{
    /// <summary>Seconds the "Application close in" countdown starts from.</summary>
    private const int CloseTimerSeconds = 5;

    private static CancellationTokenSource? _running;

    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Ctrl+C cancels the running Build/Update and returns to the menu instead of killing the app.
        Console.CancelKeyPress += (_, e) =>
        {
            if (_running is { } running)
            {
                e.Cancel = true;
                running.Cancel();
            }
        };

        ConfigPaths paths;
        try
        {
            paths = ConfigPaths.Resolve(args);
        }
        catch (DirectoryNotFoundException ex)
        {
            WriteLine(ex.Message, ConsoleColor.Red);
            await CloseAsync(CloseTimerSeconds);
            return 1;
        }

        using var http = CreateHttpClient();

        Console.WriteLine($"Config folder: {paths.ConfigDirectory}");

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("What we should do with database?");
            Console.Write("Build (B), Update (U), Close (C): ");

            var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
            switch (answer)
            {
                case "b" or "build":
                    await RunAsync("Build", ct => BuildAsync(http, paths, ct));
                    break;

                case "u" or "update":
                    await RunAsync("Update", ct => UpdateAsync(http, paths, ct));
                    break;

                // null = end of input stream (Ctrl+Z / closed pipe): close as well.
                case "c" or "close" or null:
                    await CloseAsync(CloseTimerSeconds);
                    return 0;

                default:
                    WriteLine("Unknown answer. Please type B, U or C.", ConsoleColor.Yellow);
                    break;
            }
        }
    }

    private static async Task BuildAsync(HttpClient http, ConfigPaths paths, CancellationToken ct)
    {
        var dataFile = await GameDataSource.DownloadAsync(http, ct);
        try
        {
            var data = await GameDataParser.ParseAsync(dataFile, ct);
            await new DatabaseBuilder(paths).BuildAsync(data, ct);
            WriteLine($"Database created: {paths.DatabaseFile}", ConsoleColor.Green);

            var icons = await new IconSynchronizer(http, paths).SyncAsync(data.Icons, changedKeys: null, ct);
            ReportIcons(icons);
        }
        finally
        {
            GameDataSource.TryDelete(dataFile);
        }
    }

    private static async Task UpdateAsync(HttpClient http, ConfigPaths paths, CancellationToken ct)
    {
        if (!File.Exists(paths.DatabaseFile))
        {
            WriteLine($"{ConfigPaths.DatabaseFileName} was not found, building a new database instead.", ConsoleColor.Yellow);
            await BuildAsync(http, paths, ct);
            return;
        }

        var updater = new DatabaseUpdater(paths);
        var last = await updater.GetLastMigrationAsync(ct);
        if (last is not null)
            Console.WriteLine($"Current data: {last.Name} (applied {last.AppliedAtUtc.ToLocalTime():g})");

        var dataFile = await GameDataSource.DownloadAsync(http, ct);
        try
        {
            var data = await GameDataParser.ParseAsync(dataFile, ct);
            var result = await updater.UpdateAsync(data, ct);

            if (result.Applied is { } applied)
            {
                WriteLine(
                    $"Migration {applied.Name} applied: +{applied.Added} added, ~{applied.Updated} updated, -{applied.Removed} removed.",
                    ConsoleColor.Green);
            }
            else
            {
                WriteLine("No differences found, the database is up to date.", ConsoleColor.Green);
            }

            // Runs even without data changes, so missing or failed icons from earlier runs are fetched.
            var icons = await new IconSynchronizer(http, paths).SyncAsync(data.Icons, result.Plan.ChangedIconKeys, ct);
            ReportIcons(icons);
        }
        finally
        {
            GameDataSource.TryDelete(dataFile);
        }
    }

    private static async Task RunAsync(string operation, Func<CancellationToken, Task> action)
    {
        using var cts = new CancellationTokenSource();
        _running = cts;
        try
        {
            Console.WriteLine();
            await action(cts.Token);
            WriteLine($"{operation} finished.", ConsoleColor.Green);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            WriteLine($"{operation} cancelled.", ConsoleColor.Yellow);
        }
        catch (Exception ex)
        {
            WriteLine($"{operation} failed: {ex.Message}", ConsoleColor.Red);
        }
        finally
        {
            _running = null;
        }
    }

    private static void ReportIcons(IconSyncResult result)
    {
        Console.WriteLine(
            $"Icons: {result.Total} in database, {result.Downloaded} downloaded, {result.Converted} converted, {result.Removed} stale files removed.");

        if (result.WithoutPath > 0)
            WriteLine($"{result.WithoutPath} icon entries have no image path and were skipped.", ConsoleColor.Yellow);

        if (result.Failures.Count == 0)
            return;

        WriteLine($"{result.Failures.Count} icons failed (run Update to retry):", ConsoleColor.Yellow);
        foreach (var failure in result.Failures.Take(10))
            WriteLine($"  {failure}", ConsoleColor.Yellow);
        if (result.Failures.Count > 10)
            WriteLine($"  ... and {result.Failures.Count - 10} more", ConsoleColor.Yellow);
    }

    private static async Task CloseAsync(int seconds)
    {
        for (var left = seconds; left > 0; left--)
        {
            Console.Write($"\rApplication close in {left} sec ");
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        Console.WriteLine();
    }

    private static HttpClient CreateHttpClient()
    {
        // No automatic decompression: Content-Length then matches the bytes we read,
        // which keeps the download percentage accurate.
        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.None,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            MaxConnectionsPerServer = 16
        };

        var http = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(5) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("LE.SaveEditor.DB/1.0 (+https://github.com/IgorBabayan/le-save-editor)");
        return http;
    }

    private static void WriteLine(string text, ConsoleColor color)
    {
        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = previous;
    }
}

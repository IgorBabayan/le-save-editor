using System.Net;
using System.Text;
using LE.SaveEditor.DB.Icons;
using LE.SaveEditor.DB.Import;
using LE.SaveEditor.DB.Infrastructure;

namespace LE.SaveEditor.DB;

/// <summary>
/// Generator of <c>%APPDATA%\LE.SaveEditor\base.db</c> and <c>%APPDATA%\LE.SaveEditor\Icons</c>
/// (the editor's config folder) from maxroll's Last Epoch planner data.
/// Usage: <c>LE.SaveEditor.DB [--config &lt;folder&gt;] [--run build|update] [--machine]</c>.
/// <list type="bullet">
/// <item>Without <c>--run</c> it asks what to do (Build / Update / Close) in a loop.</item>
/// <item><c>--run build|update</c> runs that operation once and exits (0 = succeeded, 1 = failed, 2 = cancelled).</item>
/// <item><c>--machine</c> prints JSON lines instead of console text; used by the editor (see <see cref="ToolOutput"/>).</item>
/// </list>
/// </summary>
internal static class Program
{
    /// <summary>Seconds the "Application close in" countdown starts from.</summary>
    private const int CloseTimerSeconds = 5;

    private const string BuildOperation = "Build";
    private const string UpdateOperation = "Update";

    private static CancellationTokenSource? _running;

    private static async Task<int> Main(string[] args)
    {
        if (args.Contains("--machine"))
            ToolOutput.EnableMachineMode();
        else
            Console.OutputEncoding = Encoding.UTF8;

        var runOperation = ParseRunOperation(args, out var runError);
        if (runError is not null)
        {
            ToolOutput.Line(runError, OutputLevel.Error);
            return (int)RunStatus.Failed;
        }

        // Ctrl+C cancels the running Build/Update and returns to the menu instead of killing the app.
        // In machine mode there is no console: the editor stops the process itself.
        if (!ToolOutput.IsMachine)
        {
            Console.CancelKeyPress += (_, e) =>
            {
                if (_running is { } running)
                {
                    e.Cancel = true;
                    running.Cancel();
                }
            };
        }

        ConfigPaths paths;
        try
        {
            paths = ConfigPaths.Resolve(args);
        }
        catch (DirectoryNotFoundException ex)
        {
            ToolOutput.Line(ex.Message, OutputLevel.Error);
            if (runOperation is not null)
            {
                ReportDone(runOperation, RunStatus.Failed);
                return (int)RunStatus.Failed;
            }

            await CloseAsync(CloseTimerSeconds);
            return 1;
        }

        using var http = CreateHttpClient();

        if (runOperation is not null)
            return (int)await RunOnceAsync(runOperation, http, paths);

        ToolOutput.Line($"Config folder: {paths.ConfigDirectory}");

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("What we should do with database?");
            Console.Write("Build (B), Update (U), Close (C): ");

            var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
            switch (answer)
            {
                case "b" or "build":
                    await RunAsync(BuildOperation, ct => BuildAsync(http, paths, ct));
                    break;

                case "u" or "update":
                    await RunAsync(UpdateOperation, ct => UpdateAsync(http, paths, ct));
                    break;

                // null = end of input stream (Ctrl+Z / closed pipe): close as well.
                case "c" or "close" or null:
                    await CloseAsync(CloseTimerSeconds);
                    return 0;

                default:
                    ToolOutput.Line("Unknown answer. Please type B, U or C.", OutputLevel.Warning);
                    break;
            }
        }
    }

    /// <summary>Non-interactive mode: runs one operation, reports the outcome and exits.</summary>
    private static async Task<RunStatus> RunOnceAsync(string operation, HttpClient http, ConfigPaths paths)
    {
        var status = await RunAsync(operation, ct => operation == BuildOperation
            ? BuildAsync(http, paths, ct)
            : UpdateAsync(http, paths, ct));

        ReportDone(operation, status);
        return status;
    }

    /// <summary>Reads <c>--run build|update</c>; <c>null</c> when absent (interactive menu).</summary>
    private static string? ParseRunOperation(string[] args, out string? error)
    {
        error = null;
        var index = Array.FindIndex(args, a => a is "--run" or "-r");
        if (index < 0)
            return null;

        var value = index + 1 < args.Length ? args[index + 1].Trim().ToLowerInvariant() : string.Empty;
        switch (value)
        {
            case "b" or "build":
                return BuildOperation;
            case "u" or "update":
                return UpdateOperation;
            default:
                error = "--run expects \"build\" or \"update\".";
                return null;
        }
    }

    private static void ReportDone(string operation, RunStatus status)
    {
        if (ToolOutput.IsMachine)
            ToolOutput.Event(new ToolEvent("done") { Operation = operation, Status = status });
    }

    private static async Task BuildAsync(HttpClient http, ConfigPaths paths, CancellationToken ct)
    {
        var dataFile = await GameDataSource.DownloadAsync(http, ct);
        try
        {
            var data = await GameDataParser.ParseAsync(dataFile, ct);
            await new DatabaseBuilder(paths).BuildAsync(data, ct);
            ToolOutput.Line($"Database created: {paths.DatabaseFile}", OutputLevel.Success);

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
            ToolOutput.Line($"{ConfigPaths.DatabaseFileName} was not found, building a new database instead.", OutputLevel.Warning);
            await BuildAsync(http, paths, ct);
            return;
        }

        var updater = new DatabaseUpdater(paths);
        if (!await updater.IsSchemaCurrentAsync(ct))
        {
            ToolOutput.Line($"{ConfigPaths.DatabaseFileName} was made by an older generator version, rebuilding it.", OutputLevel.Warning);
            await BuildAsync(http, paths, ct);
            return;
        }

        var last = await updater.GetLastMigrationAsync(ct);
        if (last is not null)
            ToolOutput.Line($"Current data: {last.Name} (applied {last.AppliedAtUtc.ToLocalTime():g})");

        var dataFile = await GameDataSource.DownloadAsync(http, ct);
        try
        {
            var data = await GameDataParser.ParseAsync(dataFile, ct);
            var result = await updater.UpdateAsync(data, ct);

            if (result.Applied is { } applied)
            {
                ToolOutput.Line(
                    $"Migration {applied.Name} applied: +{applied.Added} added, ~{applied.Updated} updated, -{applied.Removed} removed.",
                    OutputLevel.Success);
            }
            else
            {
                ToolOutput.Line("No differences found, the database is up to date.", OutputLevel.Success);
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

    private static async Task<RunStatus> RunAsync(string operation, Func<CancellationToken, Task> action)
    {
        using var cts = new CancellationTokenSource();
        _running = cts;
        try
        {
            if (!ToolOutput.IsMachine)
                Console.WriteLine();

            await action(cts.Token);
            ToolOutput.Line($"{operation} finished.", OutputLevel.Success);
            return RunStatus.Succeeded;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            ToolOutput.Line($"{operation} cancelled.", OutputLevel.Warning);
            return RunStatus.Cancelled;
        }
        catch (Exception ex)
        {
            ToolOutput.Line($"{operation} failed: {ex.Message}", OutputLevel.Error);
            return RunStatus.Failed;
        }
        finally
        {
            _running = null;
        }
    }

    private static void ReportIcons(IconSyncResult result)
    {
        ToolOutput.Line(
            $"Icons: {result.Total} in database, {result.Downloaded} downloaded, {result.Converted} converted, {result.Removed} stale files removed.");

        if (result.WithoutPath > 0)
            ToolOutput.Line($"{result.WithoutPath} icon names are not valid file names and were skipped.", OutputLevel.Warning);

        if (result.Failures.Count == 0)
            return;

        ToolOutput.Line($"{result.Failures.Count} icons failed (run Update to retry):", OutputLevel.Warning);
        foreach (var failure in result.Failures.Take(10))
            ToolOutput.Line($"  {failure}", OutputLevel.Warning);
        if (result.Failures.Count > 10)
            ToolOutput.Line($"  ... and {result.Failures.Count - 10} more", OutputLevel.Warning);
    }

    private static async Task CloseAsync(int seconds)
    {
        // Nobody reads a countdown when the editor runs the generator.
        if (ToolOutput.IsMachine)
            return;

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
}

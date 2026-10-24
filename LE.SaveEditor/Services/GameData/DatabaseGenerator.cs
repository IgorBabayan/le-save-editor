using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace LE.SaveEditor.Services.GameData;

/// <summary>
/// Starts <c>LE.SaveEditor.DB --run update --machine</c> hidden and turns its JSON lines into
/// <see cref="DatabaseGeneratorEvent"/>s. "update" builds a new database when none exists yet.
/// </summary>
public sealed class DatabaseGenerator : IDatabaseGenerator
{
    private const string ExecutableName = "LE.SaveEditor.DB.exe";

    /// <summary>At most this many stderr lines are added to a failed result.</summary>
    private const int MaxErrorLines = 10;

    public string ExecutablePath { get; } =
        Path.Combine(AppContext.BaseDirectory, "Tools", "LE.SaveEditor.DB", ExecutableName);

    public async Task<DatabaseGenerationResult> RunAsync(
        IProgress<DatabaseGeneratorEvent>? progress,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<DatabaseGeneratorMessage>();

        void AddMessage(DatabaseGeneratorMessageLevel level, string text)
        {
            var message = new DatabaseGeneratorMessage(level, text);
            messages.Add(message);
            progress?.Report(new DatabaseGeneratorMessageReceived(message));
        }

        DatabaseGenerationResult Fail(string text)
        {
            AddMessage(DatabaseGeneratorMessageLevel.Error, text);
            return new DatabaseGenerationResult(DatabaseGenerationStatus.Failed, messages);
        }

        if (!File.Exists(ExecutablePath))
            return Fail($"{ExecutableName} was not found in \"{Path.GetDirectoryName(ExecutablePath)}\". Reinstall the editor.");

        Process process;
        try
        {
            process = Process.Start(CreateStartInfo())
                      ?? throw new InvalidOperationException("The process did not start.");
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            return Fail($"{ExecutableName} could not be started: {ex.Message}");
        }

        using (process)
        {
            // Read stderr concurrently so a chatty crash cannot block the process on a full pipe.
            var errorOutput = process.StandardError.ReadToEndAsync(CancellationToken.None);
            DatabaseGenerationStatus? reportedStatus = null;

            try
            {
                while (await process.StandardOutput.ReadLineAsync(cancellationToken) is { } line)
                {
                    switch (Parse(line, out var status))
                    {
                        case DatabaseGeneratorMessageReceived received:
                            AddMessage(received.Message.Level, received.Message.Text);
                            break;

                        case { } generatorEvent:
                            progress?.Report(generatorEvent);
                            break;

                        case null when status is not null:
                            reportedStatus = status;
                            break;
                    }
                }

                await process.WaitForExitAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                TryKill(process);
                AddMessage(DatabaseGeneratorMessageLevel.Warning, "Database generation cancelled.");
                return new DatabaseGenerationResult(DatabaseGenerationStatus.Cancelled, messages);
            }

            var errors = await errorOutput;
            var finalStatus = reportedStatus ?? process.ExitCode switch
            {
                0 => DatabaseGenerationStatus.Succeeded,
                2 => DatabaseGenerationStatus.Cancelled,
                _ => DatabaseGenerationStatus.Failed
            };

            if (finalStatus == DatabaseGenerationStatus.Failed)
            {
                // Unhandled crash: the reason is only on stderr.
                var errorLines = errors
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Take(MaxErrorLines);
                foreach (var errorLine in errorLines)
                    AddMessage(DatabaseGeneratorMessageLevel.Error, errorLine);

                if (!messages.Any(m => m.Level == DatabaseGeneratorMessageLevel.Error))
                    AddMessage(DatabaseGeneratorMessageLevel.Error, $"{ExecutableName} exited with code {process.ExitCode}.");
            }

            return new DatabaseGenerationResult(finalStatus, messages);
        }
    }

    private ProcessStartInfo CreateStartInfo()
    {
        // No console window: the editor shows the progress itself.
        var startInfo = new ProcessStartInfo(ExecutablePath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            WorkingDirectory = Path.GetDirectoryName(ExecutablePath) ?? AppContext.BaseDirectory
        };

        startInfo.ArgumentList.Add("--run");
        startInfo.ArgumentList.Add("update");
        startInfo.ArgumentList.Add("--machine");
        startInfo.ArgumentList.Add("--config");
        startInfo.ArgumentList.Add(GameDataPaths.ConfigDirectory);
        return startInfo;
    }

    /// <summary>
    /// Converts one stdout line. Returns <c>null</c> for the final <c>done</c> line (its status is returned
    /// in <paramref name="status"/>) and for unknown events. A line that is not JSON becomes a plain message.
    /// </summary>
    private static DatabaseGeneratorEvent? Parse(string line, out DatabaseGenerationStatus? status)
    {
        status = null;
        if (string.IsNullOrWhiteSpace(line))
            return null;

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(line);
        }
        catch (JsonException)
        {
            return new DatabaseGeneratorMessageReceived(new DatabaseGeneratorMessage(DatabaseGeneratorMessageLevel.Info, line.Trim()));
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return null;

            switch (GetString(root, "type"))
            {
                case "bar-start":
                    return new DatabaseGeneratorStepStarted(GetInt(root, "id"), GetString(root, "title") ?? "Working");

                case "bar":
                    return new DatabaseGeneratorStepProgress(GetInt(root, "id"), GetDouble(root, "fraction"), GetString(root, "detail"));

                case "bar-end":
                    return new DatabaseGeneratorStepFinished(
                        GetInt(root, "id"),
                        GetDouble(root, "fraction"),
                        GetString(root, "detail"),
                        root.TryGetProperty("completed", out var completed) && completed.ValueKind == JsonValueKind.True);

                case "log":
                    var level = Enum.TryParse<DatabaseGeneratorMessageLevel>(GetString(root, "level"), ignoreCase: true, out var parsed)
                        ? parsed
                        : DatabaseGeneratorMessageLevel.Info;
                    return new DatabaseGeneratorMessageReceived(new DatabaseGeneratorMessage(level, GetString(root, "text") ?? string.Empty));

                case "done":
                    if (Enum.TryParse<DatabaseGenerationStatus>(GetString(root, "status"), ignoreCase: true, out var done))
                        status = done;
                    return null;

                default:
                    return null;
            }
        }
    }

    private static string? GetString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static int GetInt(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.TryGetInt32(out var number) ? number : 0;

    private static double GetDouble(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.TryGetDouble(out var number) ? Math.Clamp(number, 0, 1) : 0;

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
        {
            // Already gone.
        }
    }
}

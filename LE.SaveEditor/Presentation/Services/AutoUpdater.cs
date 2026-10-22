using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using LE.SaveEditor.Presentation.Views.Updates;
using LE.SaveEditor.Services.Settings;
using LE.SaveEditor.Services.Updates;

namespace LE.SaveEditor.Presentation.Services;

public sealed class AutoUpdater(
    ISettingsService settingsService,
    IUpdateService updateService,
    IServiceProvider services) : IAutoUpdater
{
    /// <summary>The check runs before the main window appears, so a slow network must not hold up startup.</summary>
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(8);

    public async Task<bool> TryUpdateOnStartupAsync()
    {
        if (!settingsService.Current.AutoUpdate || !updateService.IsInstalled)
            return false;

        UpdateInfo? update;
        try
        {
            using var timeout = new CancellationTokenSource(CheckTimeout);
            update = await updateService.CheckForUpdateAsync(timeout.Token);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or JsonException or IOException)
        {
            // Offline, GitHub unavailable or rate-limited: start normally.
            Debug.WriteLine($"Update check failed: {ex.Message}");
            return false;
        }

        if (update is null)
        {
            // Up to date: remove installers left over from the previous update.
            updateService.CleanupDownloads();
            return false;
        }

        var window = services.GetRequiredService<UpdateWindow>();
        var isClosed = false;
        window.Closed += (_, _) => isClosed = true;
        window.Show();

        var installerStarted = await window.ViewModel.RunAsync(update);

        if (!isClosed)
            window.Close();

        return installerStarted;
    }
}

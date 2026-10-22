using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using LE.SaveEditor.Services.Updates;

namespace LE.SaveEditor.Presentation.ViewModels.Updates;

/// <summary>Drives the small "Updating..." window shown at startup while a new version downloads.</summary>
public sealed partial class UpdateViewModel(IUpdateService updateService) : ViewModelBase
{
    private readonly CancellationTokenSource _cancellation = new();

    [ObservableProperty]
    private string _status = "Preparing update...";

    [ObservableProperty]
    private string _details = string.Empty;

    /// <summary>Download progress, 0..100.</summary>
    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private string _progressText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SkipCommand))]
    private bool _canSkip = true;

    /// <summary>
    /// Downloads the update and starts the installer.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the installer was started and the app must exit;
    /// <c>false</c> if the update was skipped or failed and the app should start normally.
    /// </returns>
    public async Task<bool> RunAsync(UpdateInfo update)
    {
        ArgumentNullException.ThrowIfNull(update);

        Status = $"Downloading version {update.Version.ToString(3)}";
        Details = $"Current version: {updateService.CurrentVersion.ToString(3)}";

        var progress = new Progress<DownloadProgress>(OnProgress);

        try
        {
            var installerPath = await updateService.DownloadAsync(update, progress, _cancellation.Token);

            CanSkip = false;
            Status = "Installing update";
            ProgressText = "The editor will restart automatically.";

            updateService.LaunchInstaller(installerPath);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or UnauthorizedAccessException
                                       or InvalidDataException or Win32Exception)
        {
            // Network problem, bad download or the UAC prompt was declined: just start the current version.
            Debug.WriteLine($"Update failed: {ex}");
            return false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSkip))]
    private void Skip() => _cancellation.Cancel();

    /// <summary>Called by the view when the user closes the window.</summary>
    public void OnClosing()
    {
        if (CanSkip)
            _cancellation.Cancel();
    }

    private void OnProgress(DownloadProgress value)
    {
        Progress = (value.Fraction ?? 0) * 100;
        ProgressText = value.TotalBytes > 0
            ? $"{ToMegabytes(value.BytesReceived):0.0} MB of {ToMegabytes(value.TotalBytes):0.0} MB"
            : $"{ToMegabytes(value.BytesReceived):0.0} MB";
    }

    private static double ToMegabytes(long bytes) => bytes / 1024d / 1024d;
}

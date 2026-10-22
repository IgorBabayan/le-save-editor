using System.Collections.ObjectModel;
using System.IO;
using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Saves.Models;
using LE.SaveEditor.Services.Settings;

namespace LE.SaveEditor.Presentation.ViewModels.Shell;

public partial class MainViewModel(
    IDialogService dialogService,
    ISettingsService settingsService,
    ISaveFolderLoader saveFolderLoader) : ViewModelBase
{
    private CancellationTokenSource? _loadCts;

    /// <summary>Characters parsed from <c>1CHARACTERSLOT_*</c> files of the selected save folder.</summary>
    public ObservableCollection<CharacterSave> Characters { get; } = [];

    /// <summary>Paths of <c>STASH_*</c> files of the selected save folder (parsed later).</summary>
    public ObservableCollection<string> StashFiles { get; } = [];

    /// <summary>Character files that could not be read.</summary>
    public ObservableCollection<SaveFileError> LoadErrors { get; } = [];

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        // Re-read the folder whenever the user presses Save in the settings dialog.
        if (dialogService.ShowSettings())
            await LoadSavesAsync();
    }

    /// <summary>Loads the save folder from the current settings. Also called once when the main window opens.</summary>
    [RelayCommand]
    private async Task LoadSavesAsync()
    {
        // A newer load (e.g. Save pressed again) supersedes the running one.
        _loadCts?.Cancel();
        using var cts = new CancellationTokenSource();
        _loadCts = cts;

        IsLoading = true;
        try
        {
            var folder = settingsService.Current.SaveFolderPath ?? string.Empty;
            var content = await saveFolderLoader.LoadAsync(folder, cts.Token);

            if (cts.IsCancellationRequested)
                return;

            Replace(Characters, content.Characters);
            Replace(StashFiles, content.StashFiles);
            Replace(LoadErrors, content.Errors);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // Superseded by a newer load.
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Folder disappeared or is not accessible.
            Characters.Clear();
            StashFiles.Clear();
            LoadErrors.Clear();
            LoadErrors.Add(new SaveFileError(settingsService.Current.SaveFolderPath ?? string.Empty, ex.Message));
        }
        finally
        {
            if (ReferenceEquals(_loadCts, cts))
            {
                _loadCts = null;
                IsLoading = false;
            }
        }
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
            target.Add(item);
    }
}

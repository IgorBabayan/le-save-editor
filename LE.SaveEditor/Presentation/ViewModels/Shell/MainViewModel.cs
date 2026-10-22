using System.Collections.ObjectModel;
using System.IO;
using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Presentation.ViewModels.Inventory;
using LE.SaveEditor.Presentation.ViewModels.Notifications;
using LE.SaveEditor.Services.Notifications;
using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Saves.Models;
using LE.SaveEditor.Services.Settings;

namespace LE.SaveEditor.Presentation.ViewModels.Shell;

public partial class MainViewModel(
    IDialogService dialogService,
    ISettingsService settingsService,
    ISaveFolderLoader saveFolderLoader,
    INotificationService notificationService,
    NotificationCenterViewModel notificationCenter) : ViewModelBase
{
    private CancellationTokenSource? _loadCts;

    /// <summary>Bell button and notification popup in the title bar.</summary>
    public NotificationCenterViewModel NotificationCenter { get; } = notificationCenter;

    /// <summary>Characters of the selected save folder, ordered by their save slot.</summary>
    public ObservableCollection<CharacterSave> Characters { get; } = [];

    /// <summary>Paths of <c>STASH_*</c> files of the selected save folder (parsed later).</summary>
    public ObservableCollection<string> StashFiles { get; } = [];

    /// <summary>Character files that could not be read.</summary>
    public ObservableCollection<SaveFileError> LoadErrors { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedCharacter))]
    [NotifyPropertyChangedFor(nameof(ShowSelectCharacterHint))]
    private CharacterSave? _selectedCharacter;

    /// <summary>Inventory of <see cref="SelectedCharacter"/>; <c>null</c> when nothing is selected.</summary>
    [ObservableProperty]
    private InventoryViewModel? _inventory;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowNoCharacters))]
    [NotifyPropertyChangedFor(nameof(ShowLoading))]
    private bool _isLoading;

    public bool HasSaveFolder => IsExistingFolder(settingsService.Current.SaveFolderPath);
    public bool ShowNoSaveFolder => !HasSaveFolder;
    public bool HasCharacters => Characters.Count > 0;
    public bool HasSelectedCharacter => SelectedCharacter is not null;
    public bool HasLoadErrors => LoadErrors.Count > 0;

    /// <summary>A folder is selected and loaded, but it holds no readable characters.</summary>
    public bool ShowNoCharacters => HasSaveFolder && !IsLoading && !HasCharacters;

    /// <summary>Loading message is shown only while the list is still empty (a reload keeps the old list visible).</summary>
    public bool ShowLoading => IsLoading && !HasCharacters;

    public bool ShowSelectCharacterHint => HasCharacters && !HasSelectedCharacter;

    partial void OnSelectedCharacterChanged(CharacterSave? value) =>
        Inventory = value is null ? null : new InventoryViewModel(value);

    /// <summary>Called once when the main window opens: loads characters if a save folder was selected earlier.</summary>
    public async Task InitializeAsync()
    {
        if (HasSaveFolder)
            await LoadSavesAsync();
        else
            NotifyStateChanged();
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        // Re-read the folder whenever the user presses Save in the settings dialog.
        if (dialogService.ShowSettings())
            await LoadSavesAsync();
    }

    [RelayCommand]
    private async Task LoadSavesAsync()
    {
        // A newer load (e.g. Save pressed again) supersedes the running one.
        _loadCts?.Cancel();
        using var cts = new CancellationTokenSource();
        _loadCts = cts;

        var folder = settingsService.Current.SaveFolderPath ?? string.Empty;
        var previousSelection = SelectedCharacter?.FilePath;

        IsLoading = true;
        try
        {
            var content = await saveFolderLoader.LoadAsync(folder, cts.Token);
            if (cts.IsCancellationRequested)
                return;

            Replace(Characters, content.Characters.OrderBy(c => c.Slot));
            Replace(StashFiles, content.StashFiles);
            Replace(LoadErrors, content.Errors);

            if (content.Errors.Count > 0)
                NotifyLoadErrors(content.Errors);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // Superseded by a newer load.
            return;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Folder disappeared or is not accessible.
            Characters.Clear();
            StashFiles.Clear();
            LoadErrors.Clear();
            LoadErrors.Add(new SaveFileError(folder, ex.Message));

            notificationService.Notify("Save folder could not be read", $"{folder}: {ex.Message}", NotificationSeverity.Error);
        }
        finally
        {
            if (ReferenceEquals(_loadCts, cts))
            {
                _loadCts = null;
                IsLoading = false;
            }
        }

        // Keep the same character selected after a reload, otherwise select the first one.
        SelectedCharacter = Characters.FirstOrDefault(c =>
                                string.Equals(c.FilePath, previousSelection, StringComparison.OrdinalIgnoreCase))
                            ?? Characters.FirstOrDefault();

        NotifyStateChanged();
    }

    private void NotifyLoadErrors(IReadOnlyCollection<SaveFileError> errors)
    {
        var title = errors.Count == 1
            ? "1 save file could not be read"
            : $"{errors.Count} save files could not be read";

        var message = string.Join(Environment.NewLine,
            errors.Select(e => $"{Path.GetFileName(e.FilePath)}: {e.Message}"));

        notificationService.Notify(title, message, NotificationSeverity.Warning);
    }

    private void NotifyStateChanged()
    {
        OnPropertyChanged(nameof(HasSaveFolder));
        OnPropertyChanged(nameof(ShowNoSaveFolder));
        OnPropertyChanged(nameof(HasCharacters));
        OnPropertyChanged(nameof(HasLoadErrors));
        OnPropertyChanged(nameof(ShowNoCharacters));
        OnPropertyChanged(nameof(ShowLoading));
        OnPropertyChanged(nameof(ShowSelectCharacterHint));
    }

    private static bool IsExistingFolder(string? path) =>
        !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
            target.Add(item);
    }
}

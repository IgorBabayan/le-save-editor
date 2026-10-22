using System.IO;
using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Services.Settings;
using LE.SaveEditor.Services.Theming;

namespace LE.SaveEditor.Presentation.ViewModels.Settings;

public sealed partial class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly IThemeService _themeService;
    private readonly IFolderPickerService _folderPicker;

    /// <summary>Theme that was active when the dialog opened; restored if the user cancels the live preview.</summary>
    private readonly AppTheme _originalTheme;
    private bool _isSaved;

    public SettingsViewModel(
        ISettingsService settingsService,
        IThemeService themeService,
        IFolderPickerService folderPicker)
    {
        _settingsService = settingsService;
        _themeService = themeService;
        _folderPicker = folderPicker;

        ThemeOptions =
        [
            CreateOption(AppTheme.DefaultLight, "Default light"),
            CreateOption(AppTheme.DefaultNight, "Default night"),
            CreateOption(AppTheme.NightOwl, "Night owl")
        ];

        var settings = settingsService.Current;
        _originalTheme = themeService.CurrentTheme;

        SelectedThemeOption = ThemeOptions.FirstOrDefault(o => o.Theme == settings.Theme)
                              ?? ThemeOptions.First(o => o.Theme == AppTheme.DefaultNight);

        SaveFolderPath = !string.IsNullOrWhiteSpace(settings.SaveFolderPath)
            ? settings.SaveFolderPath
            : Directory.Exists(LastEpochPaths.DefaultSaveFolder) ? LastEpochPaths.DefaultSaveFolder : string.Empty;
    }

    /// <summary>Asks the view to close: <c>true</c> = saved, <c>false</c> = cancelled.</summary>
    public event EventHandler<bool>? CloseRequested;

    public IReadOnlyList<ThemeOption> ThemeOptions { get; }

    [ObservableProperty]
    private ThemeOption _selectedThemeOption = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSaveFolderMissing))]
    private string _saveFolderPath = string.Empty;

    /// <summary>True when a path is entered but the folder does not exist.</summary>
    public bool IsSaveFolderMissing =>
        !string.IsNullOrWhiteSpace(SaveFolderPath) && !Directory.Exists(SaveFolderPath.Trim());

    /// <summary>Live preview: apply the theme as soon as it is selected.</summary>
    partial void OnSelectedThemeOptionChanged(ThemeOption value) => _themeService.Apply(value.Theme);

    [RelayCommand]
    private void SelectFolder()
    {
        var initial = Directory.Exists(SaveFolderPath.Trim())
            ? SaveFolderPath.Trim()
            : LastEpochPaths.DefaultSaveFolder;

        var folder = _folderPicker.PickFolder("Select Last Epoch saves folder", initial);
        if (folder is not null)
            SaveFolderPath = folder;
    }

    [RelayCommand]
    private void Save()
    {
        var path = SaveFolderPath.Trim();

        _settingsService.Save(_settingsService.Current with
        {
            Theme = SelectedThemeOption.Theme,
            SaveFolderPath = string.IsNullOrEmpty(path) ? null : path
        });

        _isSaved = true;
        CloseRequested?.Invoke(this, true);
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(this, false);

    /// <summary>Called by the view when the window closes by any means (Cancel, Esc, Alt+F4, ×).</summary>
    public void OnClosed()
    {
        if (!_isSaved)
            _themeService.Apply(_originalTheme);
    }

    private ThemeOption CreateOption(AppTheme theme, string displayName) =>
        new(theme, displayName, _themeService.GetPalette(theme));
}

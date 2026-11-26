using LE.SaveEditor.Presentation.ViewModels.Settings;

namespace LE.SaveEditor.Presentation.Views.Settings;

/// <summary>Settings dialog. Closes with <c>true</c> when the settings were saved.</summary>
public partial class SettingsWindow : ChromeWindow
{
    private readonly SettingsViewModel? _viewModel;

    /// <summary>For the XAML previewer.</summary>
    public SettingsWindow()
    {
        InitializeComponent();
    }

    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _viewModel = viewModel;

        _viewModel.CloseRequested += OnCloseRequested;
        Closed += OnClosed;
    }

    private void OnCloseRequested(object? sender, bool saved)
    {
        if (IsVisible)
            Close(saved);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (_viewModel is null)
            return;

        _viewModel.CloseRequested -= OnCloseRequested;
        _viewModel.OnClosed();
    }
}

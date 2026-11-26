using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LE.SaveEditor.Presentation.ViewModels.Shell;

namespace LE.SaveEditor.Presentation.Views;

/// <summary>Main window: character list on the left, the selected character's details on the right.</summary>
public partial class MainWindow : ChromeWindow
{
    private readonly MainViewModel? _viewModel;

    /// <summary>Set once the user agreed to discard unsaved changes, so the second close goes through.</summary>
    private bool _closeConfirmed;

    /// <summary>For the XAML previewer.</summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _viewModel = viewModel;

        // Auto-load characters if a save folder was selected in a previous session.
        Loaded += async (_, _) => await viewModel.InitializeAsync();

        Closing += OnClosing;

        // The title is dimmed while another window is active (class "inactive", see MainWindow.axaml).
        Activated += (_, _) => TitleText.Classes.Set("inactive", false);
        Deactivated += (_, _) => TitleText.Classes.Set("inactive", true);
    }

    /// <summary>
    /// Unsaved changes are lost with the window: ask first. The question is a modal dialog, which is not shown from
    /// inside the closing notification of the platform: the close is cancelled, the question asked right after, and
    /// the window closed again if the user agrees.
    /// </summary>
    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_closeConfirmed || _viewModel is null || !_viewModel.HasUnsavedChanges)
            return;

        e.Cancel = true;
        Dispatcher.UIThread.Post(() =>
        {
            if (!_viewModel.ConfirmClose())
                return;

            _closeConfirmed = true;
            Close();
        });
    }

    /// <summary>
    /// Neither resizing nor maximizing is supported (CanResize=False). Safety net for paths that maximize anyway
    /// (e.g. a window manager shortcut): put the window back to normal.
    /// </summary>
    protected override void OnPropertyChanged(Avalonia.AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WindowStateProperty && WindowState is WindowState.Maximized or WindowState.FullScreen)
            WindowState = WindowState.Normal;
    }

    #region Caption buttons

    private void OnMinimizeClick(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

    #endregion
}

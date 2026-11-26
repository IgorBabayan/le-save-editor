using Avalonia.Controls;
using Avalonia.Interactivity;
using LE.SaveEditor.Presentation.ViewModels.Database;
using LE.SaveEditor.Services.GameData;

namespace LE.SaveEditor.Presentation.Views.Database;

/// <summary>
/// Progress of <c>LE.SaveEditor.DB</c>. Starts the generator when shown and refuses to close
/// (Alt+F4 included) until it has exited; then OK closes it.
/// </summary>
public partial class DatabaseGeneratorWindow : ChromeWindow
{
    private readonly DatabaseGeneratorViewModel? _viewModel;

    /// <summary>For the XAML previewer.</summary>
    public DatabaseGeneratorWindow()
    {
        InitializeComponent();
    }

    public DatabaseGeneratorWindow(DatabaseGeneratorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _viewModel = viewModel;

        _viewModel.CloseRequested += OnCloseRequested;
        Loaded += OnLoaded;
        Closing += OnClosing;
        Closed += OnClosed;
    }

    /// <summary>Outcome of the run; <c>null</c> if the window was closed before the generator finished.</summary>
    public DatabaseGenerationResult? Result => _viewModel?.Result;

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // StartAsync never throws: every problem ends up in the view model's result.
        if (_viewModel is not null)
            await _viewModel.StartAsync();
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_viewModel?.IsRunning == true)
            e.Cancel = true;
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        if (IsVisible)
            Close(true);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.CloseRequested -= OnCloseRequested;
        Loaded -= OnLoaded;
    }
}

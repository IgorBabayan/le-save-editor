using System.ComponentModel;
using System.Windows;
using LE.SaveEditor.Presentation.ViewModels.Database;
using LE.SaveEditor.Services.GameData;

namespace LE.SaveEditor.Presentation.Views.Database;

/// <summary>
/// Progress of <c>LE.SaveEditor.DB</c>. Starts the generator when shown and refuses to close
/// (Alt+F4 included) until it has exited; then OK closes it.
/// </summary>
public partial class DatabaseGeneratorWindow : Window
{
    private readonly DatabaseGeneratorViewModel _viewModel;

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
    public DatabaseGenerationResult? Result => _viewModel.Result;

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // StartAsync never throws: every problem ends up in the view model's result.
        await _viewModel.StartAsync();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_viewModel.IsRunning)
            e.Cancel = true;
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        // Setting DialogResult on a modal window closes it.
        if (IsVisible)
            DialogResult = true;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _viewModel.CloseRequested -= OnCloseRequested;
        Loaded -= OnLoaded;
    }
}

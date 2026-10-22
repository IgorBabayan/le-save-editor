using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LE.SaveEditor.Presentation.ViewModels.Shell;

namespace LE.SaveEditor.Presentation.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Read the save folder chosen in a previous session.
        Loaded += async (_, _) => await viewModel.LoadSavesCommand.ExecuteAsync(null);

        StateChanged += (_, _) => UpdateWindowState();
        // Resource references (not brush instances) so the title follows runtime theme changes.
        Activated += (_, _) => TitleText.SetResourceReference(TextBlock.ForegroundProperty, "WindowForegroundBrush");
        Deactivated += (_, _) => TitleText.SetResourceReference(TextBlock.ForegroundProperty, "TitleBarInactiveForegroundBrush");
    }

    #region Caption buttons

    private void OnMinimizeExecuted(object sender, ExecutedRoutedEventArgs e) => SystemCommands.MinimizeWindow(this);

    private void OnMaximizeExecuted(object sender, ExecutedRoutedEventArgs e) => SystemCommands.MaximizeWindow(this);

    private void OnRestoreExecuted(object sender, ExecutedRoutedEventArgs e) => SystemCommands.RestoreWindow(this);

    private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e) => SystemCommands.CloseWindow(this);

    #endregion

    #region Maximized state handling

    /// <summary>
    /// With WindowChrome, Windows sizes a maximized window slightly larger than the monitor
    /// (by the resize frame thickness), so the edges get clipped. Compensate with a margin,
    /// and swap the maximize / restore buttons.
    /// </summary>
    private void UpdateWindowState()
    {
        var isMaximized = WindowState == WindowState.Maximized;

        MaximizeButton.Visibility = isMaximized ? Visibility.Collapsed : Visibility.Visible;
        RestoreButton.Visibility = isMaximized ? Visibility.Visible : Visibility.Collapsed;

        if (isMaximized)
        {
            var frame = SystemParameters.WindowResizeBorderThickness;
            var padded = GetPaddedBorderThickness();
            RootBorder.Margin = new Thickness(
                frame.Left + padded,
                frame.Top + padded,
                frame.Right + padded,
                frame.Bottom + padded);
            RootBorder.BorderThickness = new Thickness(0);
        }
        else
        {
            RootBorder.Margin = new Thickness(0);
            RootBorder.BorderThickness = new Thickness(1);
        }
    }

    private const int SM_CXPADDEDBORDER = 92;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    /// <summary>Padded border width (SM_CXPADDEDBORDER) converted to device-independent pixels.</summary>
    private double GetPaddedBorderThickness()
    {
        var dpiScale = VisualTreeHelper.GetDpi(this).DpiScaleX;
        return GetSystemMetrics(SM_CXPADDEDBORDER) / dpiScale;
    }

    #endregion
}

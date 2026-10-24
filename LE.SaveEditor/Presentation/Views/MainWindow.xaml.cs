using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

        NotificationsPopup.CustomPopupPlacementCallback = PlaceNotificationsPopup;

        // Auto-load characters if a save folder was selected in a previous session.
        Loaded += async (_, _) => await viewModel.InitializeAsync();

        // Unsaved changes are lost with the window: ask first.
        Closing += (_, e) => e.Cancel = !viewModel.ConfirmClose();

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

    #region Notification popup

    /// <summary>Transparent margin around the popup content that holds its drop shadow (see MainWindow.xaml).</summary>
    private const double PopupShadowMargin = 8;

    /// <summary>Gap between the bell button and the popup.</summary>
    private const double PopupGap = 2;

    /// <summary>
    /// Opens the popup below the bell with its right edge aligned to the bell's right edge,
    /// independent of the system menu-drop alignment. Sizes and offsets here are in device pixels.
    /// </summary>
    private CustomPopupPlacement[] PlaceNotificationsPopup(Size popupSize, Size targetSize, Point offset)
    {
        var dpi = VisualTreeHelper.GetDpi(this);
        var margin = PopupShadowMargin * dpi.DpiScaleX;
        var x = targetSize.Width - popupSize.Width + margin;
        var y = targetSize.Height + (PopupGap - PopupShadowMargin) * dpi.DpiScaleY;

        return [new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.Vertical)];
    }

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

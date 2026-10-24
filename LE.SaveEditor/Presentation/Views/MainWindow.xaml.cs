using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
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

        // Maximizing is not supported: strip WS_MAXIMIZEBOX once the HWND exists.
        SourceInitialized += (_, _) => DisableMaximize();
        StateChanged += (_, _) => PreventMaximizedState();
        // Resource references (not brush instances) so the title follows runtime theme changes.
        Activated += (_, _) => TitleText.SetResourceReference(TextBlock.ForegroundProperty, "WindowForegroundBrush");
        Deactivated += (_, _) => TitleText.SetResourceReference(TextBlock.ForegroundProperty, "TitleBarInactiveForegroundBrush");
    }

    #region Caption buttons

    private void OnMinimizeExecuted(object sender, ExecutedRoutedEventArgs e) => SystemCommands.MinimizeWindow(this);

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

    #region Maximize disabled

    private const int GWL_STYLE = -16;
    private const int WS_MAXIMIZEBOX = 0x00010000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    /// <summary>
    /// Removes WS_MAXIMIZEBOX from the native window. Windows then rejects every maximize path
    /// (title bar double-click, Win+Up, dragging to the top edge, the Alt+Space menu entry)
    /// while the resize borders keep working.
    /// </summary>
    private void DisableMaximize()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var style = GetWindowLong(hwnd, GWL_STYLE);
        SetWindowLong(hwnd, GWL_STYLE, style & ~WS_MAXIMIZEBOX);
    }

    /// <summary>Safety net: if anything still maximizes the window, put it back to normal.</summary>
    private void PreventMaximizedState()
    {
        if (WindowState == WindowState.Maximized)
            WindowState = WindowState.Normal;
    }

    #endregion
}

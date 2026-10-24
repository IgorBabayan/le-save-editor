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

        // Maximizing is not supported: block it in the window procedure once the HWND exists.
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

    private const int WM_SYSCOMMAND = 0x0112;
    private const int WM_NCLBUTTONDBLCLK = 0x00A3;
    private const int WM_INITMENUPOPUP = 0x0117;
    private const int SC_MAXIMIZE = 0xF030;
    private const int HTCAPTION = 2;
    private const uint MF_BYCOMMAND = 0x0000;
    private const uint MF_GRAYED = 0x0001;

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("user32.dll")]
    private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

    /// <summary>
    /// Blocks maximizing in the window procedure (title bar double-click, Win+Up, the Alt+Space menu entry, which is
    /// also greyed out). The window style is left alone on purpose: removing WS_MAXIMIZEBOX from a WindowChrome window
    /// makes Windows paint its classic caption buttons and frame line over the custom title bar whenever the frame is
    /// repainted (at start-up, when a dialog closes, after saving).
    /// </summary>
    private void DisableMaximize()
    {
        if (PresentationSource.FromVisual(this) is HwndSource source)
            source.AddHook(BlockMaximize);
    }

    private static IntPtr BlockMaximize(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_SYSCOMMAND when ((int)wParam.ToInt64() & 0xFFF0) == SC_MAXIMIZE:
            case WM_NCLBUTTONDBLCLK when wParam.ToInt64() == HTCAPTION:
                handled = true;
                break;
            case WM_INITMENUPOPUP:
                // The system menu is about to open: grey out "Maximize" (the menu itself is still shown by Windows).
                EnableMenuItem(GetSystemMenu(hwnd, false), SC_MAXIMIZE, MF_BYCOMMAND | MF_GRAYED);
                break;
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Safety net for paths that do not go through WM_SYSCOMMAND (e.g. dragging the window to the top edge with
    /// Aero Snap): put the window back to normal.
    /// </summary>
    private void PreventMaximizedState()
    {
        if (WindowState == WindowState.Maximized)
            WindowState = WindowState.Normal;
    }

    #endregion
}

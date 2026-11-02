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

        // Maximize/restore is allowed, resizing is not: block the "Size" command once the HWND exists.
        SourceInitialized += (_, _) => DisableResize();
        StateChanged += (_, _) => OnWindowStateChanged();
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

    #region Resizing disabled, maximize allowed

    private const int WM_SYSCOMMAND = 0x0112;
    private const int WM_INITMENUPOPUP = 0x0117;
    private const int SC_SIZE = 0xF000;
    private const uint MF_BYCOMMAND = 0x0000;
    private const uint MF_GRAYED = 0x0001;
    private const int SM_CXSIZEFRAME = 32;
    private const int SM_CYSIZEFRAME = 33;
    private const int SM_CXPADDEDBORDER = 92;

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("user32.dll")]
    private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    /// <summary>
    /// Drag-resizing is already off (WindowChrome.ResizeBorderThickness is 0, so the edges are not resize grips).
    /// This also blocks keyboard resizing through the Alt+Space "Size" entry, which is greyed out.
    /// ResizeMode stays CanResize on purpose: it is what keeps Maximize working (button, title bar double-click,
    /// Win+Up, Aero Snap to the top edge).
    /// </summary>
    private void DisableResize()
    {
        if (PresentationSource.FromVisual(this) is HwndSource source)
            source.AddHook(BlockResize);

        UpdateMaximizeRestoreButton();
    }

    private static IntPtr BlockResize(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_SYSCOMMAND when ((int)wParam.ToInt64() & 0xFFF0) == SC_SIZE:
                handled = true;
                break;
            case WM_INITMENUPOPUP:
                // The system menu is about to open: grey out "Size" (the menu itself is still shown by Windows).
                EnableMenuItem(GetSystemMenu(hwnd, false), SC_SIZE, MF_BYCOMMAND | MF_GRAYED);
                break;
        }

        return IntPtr.Zero;
    }

    private void OnWindowStateChanged()
    {
        UpdateMaximizeRestoreButton();

        // A maximized WindowChrome window is placed partly off-screen by the size of its (invisible) frame,
        // which would clip the title bar and the window edges: pad the content by that amount instead.
        if (WindowState == WindowState.Maximized)
        {
            var dpi = VisualTreeHelper.GetDpi(this);
            var padded = GetSystemMetrics(SM_CXPADDEDBORDER);
            var x = (GetSystemMetrics(SM_CXSIZEFRAME) + padded) / dpi.DpiScaleX;
            var y = (GetSystemMetrics(SM_CYSIZEFRAME) + padded) / dpi.DpiScaleY;
            RootBorder.Margin = new Thickness(x, y, x, y);
            RootBorder.BorderThickness = new Thickness(0);
        }
        else
        {
            RootBorder.Margin = new Thickness(0);
            RootBorder.BorderThickness = new Thickness(1);
        }
    }

    /// <summary>Shows "Maximize" (U+E922) in the normal state and "Restore" (U+E923) while maximized.</summary>
    private void UpdateMaximizeRestoreButton()
    {
        var maximized = WindowState == WindowState.Maximized;
        MaximizeRestoreButton.Content = maximized ? "\uE923" : "\uE922";
        MaximizeRestoreButton.ToolTip = maximized ? "Restore" : "Maximize";
        MaximizeRestoreButton.Command = maximized ? SystemCommands.RestoreWindowCommand : SystemCommands.MaximizeWindowCommand;
    }

    #endregion
}

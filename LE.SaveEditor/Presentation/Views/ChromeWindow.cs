using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.VisualTree;

namespace LE.SaveEditor.Presentation.Views;

/// <summary>
/// Window with the editor's own title bar (drawn by the view) on every platform.
/// </summary>
/// <remarks>
/// <para>Windows: the client area is extended into the frame, so snapping, the shadow and native resizing stay.</para>
/// <para>Linux and macOS: no system decorations; the window is moved from its title bar and, when resizable,
/// resized from a <see cref="ResizeBorder"/>-wide band along its edges.</para>
/// <para>Any element with the class <c>titlebar</c> moves the window when dragged (buttons in it keep working,
/// they handle their own presses); a double click on it maximizes / restores a resizable window.</para>
/// </remarks>
public class ChromeWindow : Window
{
    /// <summary>Width of the band along the edges that resizes the window (Linux / macOS).</summary>
    private const double ResizeBorder = 5;

    private static readonly bool UsesNativeFrame = OperatingSystem.IsWindows();

    /// <summary>Resize cursor shown along an edge; <c>null</c> elsewhere (the controls' own cursors apply).</summary>
    private StandardCursorType? _edgeCursor;

    public ChromeWindow()
    {
        if (UsesNativeFrame)
        {
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            ExtendClientAreaTitleBarHeightHint = -1;
        }
        else
        {
            SystemDecorations = SystemDecorations.None;
            AddHandler(PointerMovedEvent, OnEdgePointerMoved, RoutingStrategies.Tunnel);
            AddHandler(PointerPressedEvent, OnEdgePointerPressed, RoutingStrategies.Tunnel);
        }

        // Bubble, after the controls in the title bar had their chance (buttons mark the press handled).
        AddHandler(PointerPressedEvent, OnTitleBarPointerPressed, RoutingStrategies.Bubble);
    }

    protected override Type StyleKeyOverride => typeof(Window);

    private void OnTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Handled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || !IsInTitleBar(e.Source))
            return;

        if (e.ClickCount == 2 && CanResize)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            e.Handled = true;
            return;
        }

        BeginMoveDrag(e);
        e.Handled = true;
    }

    private static bool IsInTitleBar(object? source)
    {
        for (var node = source as Visual; node is not null; node = node.GetVisualParent())
        {
            if (node is StyledElement element && element.Classes.Contains("titlebar"))
                return true;
            if (node is Window)
                return false;
        }

        return false;
    }

    private void OnEdgePointerMoved(object? sender, PointerEventArgs e)
    {
        var edge = EdgeAt(e.GetPosition(this));
        var cursor = edge switch
        {
            WindowEdge.West or WindowEdge.East => StandardCursorType.SizeWestEast,
            WindowEdge.North or WindowEdge.South => StandardCursorType.SizeNorthSouth,
            WindowEdge.NorthWest => StandardCursorType.TopLeftCorner,
            WindowEdge.NorthEast => StandardCursorType.TopRightCorner,
            WindowEdge.SouthWest => StandardCursorType.BottomLeftCorner,
            WindowEdge.SouthEast => StandardCursorType.BottomRightCorner,
            _ => (StandardCursorType?)null
        };

        // Only touch the cursor when it changes: this runs on every pointer move.
        if (cursor == _edgeCursor)
            return;

        _edgeCursor = cursor;
        if (cursor is { } type)
            Cursor = new Cursor(type);
        else
            ClearValue(CursorProperty);
    }

    private void OnEdgePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || EdgeAt(e.GetPosition(this)) is not { } edge)
            return;

        BeginResizeDrag(edge, e);
        e.Handled = true;
    }

    private WindowEdge? EdgeAt(Point point)
    {
        if (!CanResize || WindowState != WindowState.Normal)
            return null;

        var left = point.X <= ResizeBorder;
        var right = point.X >= Bounds.Width - ResizeBorder;
        var top = point.Y <= ResizeBorder;
        var bottom = point.Y >= Bounds.Height - ResizeBorder;

        return (left, right, top, bottom) switch
        {
            (true, _, true, _) => WindowEdge.NorthWest,
            (_, true, true, _) => WindowEdge.NorthEast,
            (true, _, _, true) => WindowEdge.SouthWest,
            (_, true, _, true) => WindowEdge.SouthEast,
            (true, _, _, _) => WindowEdge.West,
            (_, true, _, _) => WindowEdge.East,
            (_, _, true, _) => WindowEdge.North,
            (_, _, _, true) => WindowEdge.South,
            _ => null
        };
    }
}

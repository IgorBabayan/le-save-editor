using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LE.SaveEditor.Presentation.ViewModels.Inventory;

namespace LE.SaveEditor.Presentation.Views.Inventory;

/// <summary>
/// "Inventory" tab of the character view. Items of the backpack are dragged with the mouse inside the backpack grid:
/// pressing an item and moving it past the system drag distance starts a drag, a highlight shows the cells it would
/// take (green when it fits, red otherwise), and releasing the button over a free spot moves it there. Releasing
/// over a taken spot, pressing Escape or losing the mouse leaves the item where it was. A click without moving
/// still opens the item inspector.
/// </summary>
public partial class InventoryView : UserControl
{
    private const string ValidDropBrushKey = "SuccessForegroundBrush";
    private const string InvalidDropBrushKey = "ErrorForegroundBrush";

    /// <summary>Opacity of the highlight's fill (its border is fully opaque).</summary>
    private const double DropFillOpacity = 0.2;

    private BackpackItemViewModel? _pressedItem;
    private Point _pressPoint;
    private int _grabColumn;
    private int _grabRow;
    private bool _isDragging;
    private int _targetColumn;
    private int _targetRow;
    private bool _canDrop;

    public InventoryView()
    {
        InitializeComponent();
        PreviewKeyDown += OnPreviewKeyDown;
    }

    private BackpackViewModel? Backpack => BackpackItems.DataContext as BackpackViewModel;

    private double CellWidth => BackpackItems.ActualWidth / BackpackViewModel.Columns;

    private double CellHeight => BackpackItems.ActualHeight / BackpackViewModel.Rows;

    private void BackpackItems_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (FindItem(e.OriginalSource as DependencyObject) is not { } item || CellWidth <= 0 || CellHeight <= 0)
            return;

        // Not handled: a plain click must still reach the item's button.
        _pressedItem = item;
        _pressPoint = e.GetPosition(BackpackItems);

        // The cell of the item under the mouse stays under the mouse while dragging.
        _grabColumn = Math.Clamp((int)(_pressPoint.X / CellWidth) - item.Column, 0, item.Width - 1);
        _grabRow = Math.Clamp((int)(_pressPoint.Y / CellHeight) - item.Row, 0, item.Height - 1);
    }

    private void BackpackItems_OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_pressedItem is null)
            return;

        if (e.LeftButton != MouseButtonState.Pressed)
        {
            EndDrag();
            return;
        }

        var position = e.GetPosition(BackpackItems);
        if (!_isDragging)
        {
            if (Math.Abs(position.X - _pressPoint.X) < SystemParameters.MinimumHorizontalDragDistance
                && Math.Abs(position.Y - _pressPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            StartDrag(_pressedItem);
        }

        UpdateTarget(_pressedItem, position);
        e.Handled = true;
    }

    private void BackpackItems_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging)
        {
            // A click: the button opens the inspector.
            _pressedItem = null;
            return;
        }

        if (_pressedItem is { } item && _canDrop)
            Backpack?.MoveItem(item, _targetColumn, _targetRow);

        EndDrag();

        // The drag took the mouse from the button, but make sure the release is not seen as a click.
        e.Handled = true;
    }

    private void BackpackItems_OnLostMouseCapture(object sender, MouseEventArgs e)
    {
        // The item's button losing the mouse to the drag bubbles here too; only our own capture ends a drag.
        if (_isDragging && ReferenceEquals(e.OriginalSource, BackpackItems))
            EndDrag();
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!_isDragging || e.Key != Key.Escape)
            return;

        EndDrag();
        e.Handled = true;
    }

    private void StartDrag(BackpackItemViewModel item)
    {
        _isDragging = true;
        item.IsDragging = true;
        DropPreview.Visibility = Visibility.Visible;
        Mouse.OverrideCursor = Cursors.SizeAll;

        // Keeps the mouse events coming while the pointer is over other items or outside of the grid.
        BackpackItems.CaptureMouse();
    }

    /// <summary>
    /// Puts the item's top-left cell where the grabbed cell is under the mouse, kept inside the grid,
    /// and shows whether it fits there.
    /// </summary>
    private void UpdateTarget(BackpackItemViewModel item, Point position)
    {
        var column = (int)Math.Floor(position.X / CellWidth) - _grabColumn;
        var row = (int)Math.Floor(position.Y / CellHeight) - _grabRow;
        _targetColumn = Math.Clamp(column, 0, BackpackViewModel.Columns - item.Width);
        _targetRow = Math.Clamp(row, 0, BackpackViewModel.Rows - item.Height);
        _canDrop = Backpack?.CanPlace(item.Footprint, _targetColumn, _targetRow, item) == true;

        Canvas.SetLeft(DropPreview, _targetColumn * CellWidth + 1);
        Canvas.SetTop(DropPreview, _targetRow * CellHeight + 1);
        DropPreview.Width = Math.Max(0, item.Width * CellWidth - 2);
        DropPreview.Height = Math.Max(0, item.Height * CellHeight - 2);

        var brushKey = _canDrop ? ValidDropBrushKey : InvalidDropBrushKey;
        DropPreview.SetResourceReference(Border.BorderBrushProperty, brushKey);
        DropPreview.Background = TryFindResource(brushKey) is SolidColorBrush brush
            ? new SolidColorBrush(brush.Color) { Opacity = DropFillOpacity }
            : null;
    }

    /// <summary>Ends a drag (or forgets a press); the item stays where it is unless it was moved before.</summary>
    private void EndDrag()
    {
        var wasDragging = _isDragging;
        _isDragging = false;
        _canDrop = false;

        if (_pressedItem is { } item)
            item.IsDragging = false;
        _pressedItem = null;

        if (!wasDragging)
            return;

        DropPreview.Visibility = Visibility.Collapsed;
        Mouse.OverrideCursor = null;

        // Releasing raises LostMouseCapture; _isDragging is already false, so it is ignored.
        if (BackpackItems.IsMouseCaptured)
            BackpackItems.ReleaseMouseCapture();
    }

    /// <summary>The backpack item shown by <paramref name="source"/> or one of its ancestors inside the item layer.</summary>
    private BackpackItemViewModel? FindItem(DependencyObject? source)
    {
        for (var node = source; node is not null && !ReferenceEquals(node, BackpackItems);
             node = node is Visual ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node))
        {
            if (node is FrameworkElement { DataContext: BackpackItemViewModel item })
                return item;
        }

        return null;
    }
}

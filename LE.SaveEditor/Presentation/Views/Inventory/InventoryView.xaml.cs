using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LE.SaveEditor.Presentation.ViewModels.Inventory;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.Views.Inventory;

/// <summary>
/// "Inventory" tab of the character view. Items of the backpack are dragged with the mouse inside the backpack grid:
/// pressing an item and moving it past the system drag distance starts a drag, a highlight shows the cells it would
/// take (green when it fits, red otherwise), and releasing the button over a free spot moves it there. Releasing
/// over a taken spot, pressing Escape or losing the mouse leaves the item where it was. A click without moving
/// still opens the item inspector. Double-clicking an empty cell offers the item types to build there and opens the
/// item inspector for a new item of the chosen type next to that cell.
/// </summary>
public partial class InventoryView : UserControl
{
    private const string ValidDropBrushKey = "SuccessForegroundBrush";
    private const string InvalidDropBrushKey = "ErrorForegroundBrush";

    /// <summary>Opacity of the highlight's fill (its border is fully opaque).</summary>
    private const double DropFillOpacity = 0.2;

    /// <summary>Item types offered for a new backpack item, in menu order (rings are built as left rings).</summary>
    private static readonly EquipmentSlot[] NewItemSlots =
    [
        EquipmentSlot.Helmet,
        EquipmentSlot.BodyArmour,
        EquipmentSlot.Gloves,
        EquipmentSlot.Belt,
        EquipmentSlot.Boots,
        EquipmentSlot.Weapon,
        EquipmentSlot.OffHand,
        EquipmentSlot.Amulet,
        EquipmentSlot.LeftRing,
        EquipmentSlot.Relic
    ];

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
        if (CellWidth <= 0 || CellHeight <= 0)
            return;

        if (FindItem(e.OriginalSource as DependencyObject) is not { } item)
        {
            if (e.ClickCount == 2 && ShowNewItemMenu(e.GetPosition(BackpackItems)))
                e.Handled = true;
            return;
        }

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

    /// <summary>
    /// Offers the item types that can be built in the empty cell under <paramref name="position"/>; types with no
    /// room left anywhere in the backpack are disabled.
    /// </summary>
    /// <returns><c>false</c> when the position is not over an empty cell (nothing was shown).</returns>
    private bool ShowNewItemMenu(Point position)
    {
        var column = (int)Math.Floor(position.X / CellWidth);
        var row = (int)Math.Floor(position.Y / CellHeight);
        if (Backpack is not { } backpack || !backpack.IsCellFree(column, row))
            return false;

        var anchor = BackpackCells.ItemContainerGenerator.ContainerFromIndex(row * BackpackViewModel.Columns + column) as FrameworkElement;
        var menu = new ContextMenu
        {
            PlacementTarget = BackpackItems,
            Placement = PlacementMode.MousePoint
        };
        if (TryFindResource("ThemedContextMenuStyle") is Style menuStyle)
            menu.Style = menuStyle;

        var itemStyle = TryFindResource("ThemedMenuItemStyle") as Style;
        menu.ItemContainerStyle = itemStyle;

        // Title: a menu item that can be neither highlighted nor clicked.
        menu.Items.Add(new MenuItem
        {
            Header = new TextBlock
            {
                Text = "Build a new item",
                FontWeight = FontWeights.SemiBold,
                Style = TryFindResource("SecondaryTextStyle") as Style
            },
            Style = itemStyle,
            Focusable = false,
            IsHitTestVisible = false
        });
        foreach (var slot in NewItemSlots)
        {
            var hasRoom = backpack.HasRoomFor(slot);
            var menuItem = new MenuItem
            {
                Header = slot.DisplayName,
                IsEnabled = hasRoom,
                Style = itemStyle,
                ToolTip = hasRoom ? null : "No room left in the backpack for this item type"
            };
            ToolTipService.SetShowOnDisabled(menuItem, true);

            var chosen = slot;
            // After the menu has closed, so the inspector keeps the focus it takes.
            menuItem.Click += (_, _) => Dispatcher.InvokeAsync(
                () => BuildNewItem(chosen, column, row, anchor),
                DispatcherPriority.Background);
            menu.Items.Add(menuItem);
        }

        menu.IsOpen = true;
        return true;
    }

    private void BuildNewItem(EquipmentSlot slot, int column, int row, FrameworkElement? anchor)
    {
        if (DataContext is not InventoryViewModel inventory)
            return;

        var request = new NewBackpackItemRequest(slot, column, row, anchor ?? BackpackItems);
        if (inventory.BuildBackpackItemCommand.CanExecute(request))
            inventory.BuildBackpackItemCommand.Execute(request);
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

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
/// "Inventory" tab of the character view. Items of the backpack and idols of the idol grid are dragged with the mouse
/// inside their own grid: pressing an item and moving it past the system drag distance starts a drag, a highlight shows
/// the cells it would take (green when it fits, red otherwise), and releasing the button over a free spot moves it
/// there. Releasing over a taken spot, pressing Escape or losing the mouse leaves the item where it was. A click
/// without moving still opens the item inspector. Right-clicking an empty backpack cell offers the item types to build
/// there and opens the item inspector for a new item of the chosen type next to that cell; right-clicking an empty
/// idol cell opens it for a new idol right away. Right-clicking an equipped item, a backpack item or an idol offers to
/// edit or remove it.
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
        EquipmentSlot.Relic,
        EquipmentSlot.Idol
    ];

    /// <summary>The controls of one grid: its item layer, its background cells and its drop highlight.</summary>
    private sealed record GridParts(ItemsControl Items, ItemsControl Cells, Border Preview)
    {
        public BackpackViewModel? Grid => Items.DataContext as BackpackViewModel;

        public double CellWidth => Grid is { ColumnCount: > 0 } grid ? Items.ActualWidth / grid.ColumnCount : 0;

        public double CellHeight => Grid is { RowCount: > 0 } grid ? Items.ActualHeight / grid.RowCount : 0;
    }

    /// <summary>Grid of the pressed / dragged item.</summary>
    private GridParts? _parts;
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

    private GridParts? PartsOf(object sender) =>
        ReferenceEquals(sender, BackpackItems) ? new GridParts(BackpackItems, BackpackCells, DropPreview)
        : ReferenceEquals(sender, IdolItems) ? new GridParts(IdolItems, IdolCells, IdolDropPreview)
        : null;

    private void GridItems_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging || PartsOf(sender) is not { CellWidth: > 0, CellHeight: > 0 } parts)
            return;

        if (FindItem(e.OriginalSource as DependencyObject, parts.Items) is not { } item)
            return;

        // Not handled: a plain click must still reach the item's button.
        _parts = parts;
        _pressedItem = item;
        _pressPoint = e.GetPosition(parts.Items);

        // The cell of the item under the mouse stays under the mouse while dragging.
        _grabColumn = Math.Clamp((int)(_pressPoint.X / parts.CellWidth) - item.Column, 0, item.Width - 1);
        _grabRow = Math.Clamp((int)(_pressPoint.Y / parts.CellHeight) - item.Row, 0, item.Height - 1);
    }

    private void GridItems_OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_pressedItem is null || _parts is not { } parts || !ReferenceEquals(sender, parts.Items))
            return;

        if (e.LeftButton != MouseButtonState.Pressed)
        {
            EndDrag();
            return;
        }

        var position = e.GetPosition(parts.Items);
        if (!_isDragging)
        {
            if (Math.Abs(position.X - _pressPoint.X) < SystemParameters.MinimumHorizontalDragDistance
                && Math.Abs(position.Y - _pressPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            StartDrag(parts, _pressedItem);
        }

        UpdateTarget(parts, _pressedItem, position);
        e.Handled = true;
    }

    private void GridItems_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging)
        {
            // A click: the button opens the inspector.
            _pressedItem = null;
            _parts = null;
            return;
        }

        if (_pressedItem is { } item && _canDrop)
            _parts?.Grid?.MoveItem(item, _targetColumn, _targetRow);

        EndDrag();

        // The drag took the mouse from the button, but make sure the release is not seen as a click.
        e.Handled = true;
    }

    private void GridItems_OnLostMouseCapture(object sender, MouseEventArgs e)
    {
        // The item's button losing the mouse to the drag bubbles here too; only our own capture ends a drag.
        if (_isDragging && _parts is { } parts && ReferenceEquals(e.OriginalSource, parts.Items))
            EndDrag();
    }

    /// <summary>
    /// A right-click on an empty cell builds a new item there: the backpack offers the item types, the idol grid opens
    /// the inspector for a new idol at once. A right-click on an item offers to edit or remove it.
    /// </summary>
    private void GridItems_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging || PartsOf(sender) is not { CellWidth: > 0, CellHeight: > 0 } parts)
            return;

        if (FindItem(e.OriginalSource as DependencyObject, parts.Items) is { } item)
        {
            ShowItemMenu(parts.Items, item.Equipment, () => ExecuteInventory(i => i.RemoveGridItemCommand, item));
            e.Handled = true;
            return;
        }

        var position = e.GetPosition(parts.Items);
        var column = (int)Math.Floor(position.X / parts.CellWidth);
        var row = (int)Math.Floor(position.Y / parts.CellHeight);
        if (parts.Grid is not { } grid || !grid.IsCellFree(column, row))
            return;

        if (grid.Layout.OnlySlot is { } onlySlot)
        {
            var anchor = CellAt(parts, grid, column, row) ?? parts.Items;
            // After the click is done, so the inspector keeps the focus it takes.
            Dispatcher.InvokeAsync(() => BuildNewItem(onlySlot, grid, column, row, anchor), DispatcherPriority.Background);
        }
        else
        {
            ShowNewItemMenu(parts, grid, column, row);
        }

        e.Handled = true;
    }

    /// <summary>A right-click on an equipment slot that holds an item offers to edit or remove it.</summary>
    private void EquipmentGrid_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        for (var node = e.OriginalSource as DependencyObject; node is not null && !ReferenceEquals(node, sender);
             node = node is Visual ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node))
        {
            if (node is not ButtonBase { DataContext: EquipmentSlotViewModel slot } button)
                continue;

            if (slot.HasItem)
            {
                ShowItemMenu(button, slot, () => ExecuteInventory(i => i.RemoveEquippedItemCommand, slot));
                e.Handled = true;
            }

            return;
        }
    }

    /// <summary>
    /// Context menu of an item: "Edit…" (when the editor can show it, <paramref name="slot"/> is set) and "Remove".
    /// </summary>
    private void ShowItemMenu(UIElement target, EquipmentSlotViewModel? slot, Action remove)
    {
        var menu = new ContextMenu
        {
            PlacementTarget = target,
            Placement = PlacementMode.MousePoint
        };
        if (TryFindResource("ThemedContextMenuStyle") is Style menuStyle)
            menu.Style = menuStyle;

        var itemStyle = TryFindResource("ThemedMenuItemStyle") as Style;
        menu.ItemContainerStyle = itemStyle;

        if (slot is not null)
        {
            var edit = new MenuItem { Header = "Edit…", Style = itemStyle };
            // Through the slot's own button, so the inspector opens next to it.
            edit.Click += (_, _) => Dispatcher.InvokeAsync(() =>
            {
                if (FindSlotButton(target, slot) is { } button)
                {
                    button.Focus();
                    button.Command?.Execute(button.CommandParameter);
                }
            }, DispatcherPriority.Background);
            menu.Items.Add(edit);
        }

        var removeItem = new MenuItem { Header = "Remove", Style = itemStyle };
        // After the menu has closed: removing asks for confirmation first.
        removeItem.Click += (_, _) => Dispatcher.InvokeAsync(remove, DispatcherPriority.Background);
        menu.Items.Add(removeItem);

        menu.IsOpen = true;
    }

    /// <summary>The button bound to <paramref name="slot"/> at or under <paramref name="root"/>.</summary>
    private static ButtonBase? FindSlotButton(DependencyObject root, EquipmentSlotViewModel slot)
    {
        if (root is ButtonBase { DataContext: var context } self && ReferenceEquals(context, slot))
            return self;

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            if (FindSlotButton(VisualTreeHelper.GetChild(root, i), slot) is { } found)
                return found;
        }

        return null;
    }

    private void ExecuteInventory(Func<InventoryViewModel, System.Windows.Input.ICommand> command, object parameter)
    {
        if (DataContext is InventoryViewModel inventory && command(inventory) is { } c && c.CanExecute(parameter))
            c.Execute(parameter);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!_isDragging || e.Key != Key.Escape)
            return;

        EndDrag();
        e.Handled = true;
    }

    private void StartDrag(GridParts parts, BackpackItemViewModel item)
    {
        _isDragging = true;
        item.IsDragging = true;
        parts.Preview.Visibility = Visibility.Visible;
        Mouse.OverrideCursor = Cursors.SizeAll;

        // Keeps the mouse events coming while the pointer is over other items or outside of the grid.
        parts.Items.CaptureMouse();
    }

    /// <summary>
    /// Puts the item's top-left cell where the grabbed cell is under the mouse, kept inside the grid,
    /// and shows whether it fits there.
    /// </summary>
    private void UpdateTarget(GridParts parts, BackpackItemViewModel item, Point position)
    {
        if (parts.Grid is not { } grid)
            return;

        var cellWidth = parts.CellWidth;
        var cellHeight = parts.CellHeight;
        var column = (int)Math.Floor(position.X / cellWidth) - _grabColumn;
        var row = (int)Math.Floor(position.Y / cellHeight) - _grabRow;
        _targetColumn = Math.Clamp(column, 0, Math.Max(0, grid.ColumnCount - item.Width));
        _targetRow = Math.Clamp(row, 0, Math.Max(0, grid.RowCount - item.Height));
        _canDrop = grid.CanPlace(item.Footprint, _targetColumn, _targetRow, item);

        var preview = parts.Preview;
        Canvas.SetLeft(preview, _targetColumn * cellWidth + 1);
        Canvas.SetTop(preview, _targetRow * cellHeight + 1);
        preview.Width = Math.Max(0, item.Width * cellWidth - 2);
        preview.Height = Math.Max(0, item.Height * cellHeight - 2);

        var brushKey = _canDrop ? ValidDropBrushKey : InvalidDropBrushKey;
        preview.SetResourceReference(Border.BorderBrushProperty, brushKey);
        preview.Background = TryFindResource(brushKey) is SolidColorBrush brush
            ? new SolidColorBrush(brush.Color) { Opacity = DropFillOpacity }
            : null;
    }

    /// <summary>Ends a drag (or forgets a press); the item stays where it is unless it was moved before.</summary>
    private void EndDrag()
    {
        var wasDragging = _isDragging;
        var parts = _parts;
        _isDragging = false;
        _canDrop = false;
        _parts = null;

        if (_pressedItem is { } item)
            item.IsDragging = false;
        _pressedItem = null;

        if (!wasDragging || parts is null)
            return;

        parts.Preview.Visibility = Visibility.Collapsed;
        Mouse.OverrideCursor = null;

        // Releasing raises LostMouseCapture; _isDragging is already false, so it is ignored.
        if (parts.Items.IsMouseCaptured)
            parts.Items.ReleaseMouseCapture();
    }

    /// <summary>The background cell of <paramref name="grid"/> at <paramref name="column"/>, <paramref name="row"/>.</summary>
    private static FrameworkElement? CellAt(GridParts parts, BackpackViewModel grid, int column, int row) =>
        parts.Cells.ItemContainerGenerator.ContainerFromIndex(row * grid.ColumnCount + column) as FrameworkElement;

    /// <summary>
    /// Offers the item types that can be built in the empty cell <paramref name="column"/>, <paramref name="row"/>;
    /// types with no room left anywhere in the grid are disabled.
    /// </summary>
    private void ShowNewItemMenu(GridParts parts, BackpackViewModel grid, int column, int row)
    {
        var anchor = CellAt(parts, grid, column, row);
        var menu = new ContextMenu
        {
            PlacementTarget = parts.Items,
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
            var hasRoom = grid.HasRoomFor(slot);
            var menuItem = new MenuItem
            {
                Header = slot.DisplayName,
                IsEnabled = hasRoom,
                Style = itemStyle,
                ToolTip = hasRoom ? null : $"No room left in the {grid.Layout.Name} for this item type"
            };
            ToolTipService.SetShowOnDisabled(menuItem, true);

            var chosen = slot;
            // After the menu has closed, so the inspector keeps the focus it takes.
            menuItem.Click += (_, _) => Dispatcher.InvokeAsync(
                () => BuildNewItem(chosen, grid, column, row, anchor ?? parts.Items),
                DispatcherPriority.Background);
            menu.Items.Add(menuItem);
        }

        menu.IsOpen = true;
    }

    private void BuildNewItem(EquipmentSlot slot, BackpackViewModel grid, int column, int row, FrameworkElement? anchor)
    {
        if (DataContext is not InventoryViewModel inventory)
            return;

        var request = new NewBackpackItemRequest(slot, column, row, anchor, grid);
        if (inventory.BuildBackpackItemCommand.CanExecute(request))
            inventory.BuildBackpackItemCommand.Execute(request);
    }

    /// <summary>The grid item shown by <paramref name="source"/> or one of its ancestors inside the item layer <paramref name="items"/>.</summary>
    private static BackpackItemViewModel? FindItem(DependencyObject? source, ItemsControl items)
    {
        for (var node = source; node is not null && !ReferenceEquals(node, items);
             node = node is Visual ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node))
        {
            if (node is FrameworkElement { DataContext: BackpackItemViewModel item })
                return item;
        }

        return null;
    }
}

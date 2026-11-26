using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LE.SaveEditor.Presentation.ViewModels.Inventory;
using LE.SaveEditor.Services.GameData.Items;
using Material.Icons;
using LE.SaveEditor.Presentation.Controls;

namespace LE.SaveEditor.Presentation.Views.Inventory;

/// <summary>
/// "Inventory" tab of the character view. Items of the backpack and idols of the idol grid are dragged with the mouse
/// inside their own grid: pressing an item and moving it past the system drag distance starts a drag, a highlight shows
/// the cells it would take (green when it fits, red otherwise), and releasing the button over a free spot moves it
/// there. Releasing over a taken spot, pressing Escape or losing the mouse leaves the item where it was. A click
/// without moving still opens the item inspector. Right-clicking an empty backpack cell offers the item types to build
/// there and opens the item inspector for a new item of the chosen type next to that cell; right-clicking an empty
/// idol cell opens it for a new idol right away. Right-clicking an equipped item, the idol altar, a backpack item (also a
/// rune or glyph) or an idol offers to edit or remove it.
/// </summary>
public partial class InventoryView : UserControl
{
    private const string ValidDropBrushKey = "SuccessForegroundBrush";
    private const string InvalidDropBrushKey = "ErrorForegroundBrush";

    /// <summary>Opacity of the highlight's fill (its border is fully opaque).</summary>
    private const double DropFillOpacity = 0.2;

    /// <summary>
    /// Item types offered for a new backpack item, in menu order and grouped (armour, weapons, jewellery, idols and idol
    /// altars); a separator goes between groups. Rings are built as left rings.
    /// </summary>
    private static readonly EquipmentSlot[][] NewItemSlotGroups =
    [
        [EquipmentSlot.Helmet, EquipmentSlot.BodyArmour, EquipmentSlot.Gloves, EquipmentSlot.Belt, EquipmentSlot.Boots],
        [EquipmentSlot.Weapon, EquipmentSlot.OffHand],
        [EquipmentSlot.Amulet, EquipmentSlot.LeftRing, EquipmentSlot.Relic],
        [EquipmentSlot.Idol, EquipmentSlot.IdolAltar]
    ];

    /// <summary>Material icon of each item type in the menus (names of <see cref="MaterialIconKind"/>).</summary>
    private static string IconNameOf(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Helmet => "HardHat",
        EquipmentSlot.BodyArmour => "TshirtCrew",
        EquipmentSlot.Gloves => "HandBackLeft",
        EquipmentSlot.Belt => "Seatbelt",
        EquipmentSlot.Boots => "ShoeFormal",
        EquipmentSlot.Weapon => "Sword",
        EquipmentSlot.OffHand => "Shield",
        EquipmentSlot.Amulet => "Necklace",
        EquipmentSlot.LeftRing or EquipmentSlot.RightRing => "Ring",
        EquipmentSlot.Relic => "TreasureChest",
        EquipmentSlot.Idol => "DiamondStone",
        EquipmentSlot.IdolAltar => "Pillar",
        _ => "HelpCircleOutline"
    };

    /// <summary>Distance the mouse must move while pressed before an item is dragged (like the system's drag distance).</summary>
    private const double DragThreshold = 4;

    /// <summary>The controls of one grid: its item layer, its background cells and its drop highlight.</summary>
    private sealed record GridParts(ItemsControl Items, ItemsControl Cells, Border Preview)
    {
        public BackpackViewModel? Grid => Items.DataContext as BackpackViewModel;

        public double CellWidth => Grid is { ColumnCount: > 0 } grid ? Items.Bounds.Width / grid.ColumnCount : 0;

        public double CellHeight => Grid is { RowCount: > 0 } grid ? Items.Bounds.Height / grid.RowCount : 0;
    }

    /// <summary>Grid of the pressed / dragged item.</summary>
    private GridParts? _parts;
    private BackpackItemViewModel? _pressedItem;
    private IPointer? _pointer;
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
        AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);

        foreach (var items in new[] { BackpackItems, IdolItems })
        {
            // Tunnel: seen before the item buttons, which handle presses and releases themselves.
            items.AddHandler(PointerPressedEvent, GridItems_OnPointerPressed, RoutingStrategies.Tunnel);
            items.AddHandler(PointerMovedEvent, GridItems_OnPointerMoved, RoutingStrategies.Tunnel);
            items.AddHandler(PointerReleasedEvent, GridItems_OnPointerReleased, RoutingStrategies.Tunnel);
            items.AddHandler(PointerReleasedEvent, GridItems_OnRightButtonReleased, RoutingStrategies.Bubble, handledEventsToo: true);
            items.AddHandler(PointerCaptureLostEvent, GridItems_OnPointerCaptureLost, RoutingStrategies.Bubble | RoutingStrategies.Direct);
        }

        EquipmentGrid.AddHandler(PointerReleasedEvent, EquipmentGrid_OnRightButtonReleased, RoutingStrategies.Bubble, handledEventsToo: true);
        IdolAltarSlot.AddHandler(PointerReleasedEvent, EquipmentGrid_OnRightButtonReleased, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    private GridParts? PartsOf(object? sender) =>
        ReferenceEquals(sender, BackpackItems) ? new GridParts(BackpackItems, BackpackCells, DropPreview)
        : ReferenceEquals(sender, IdolItems) ? new GridParts(IdolItems, IdolCells, IdolDropPreview)
        : null;

    private void GridItems_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_isDragging
            || !e.GetCurrentPoint(sender as Visual).Properties.IsLeftButtonPressed
            || PartsOf(sender) is not { CellWidth: > 0, CellHeight: > 0 } parts)
            return;

        if (FindItem(e.Source as Visual, parts.Items) is not { } item)
            return;

        // Not handled: a plain click must still reach the item's button.
        _parts = parts;
        _pressedItem = item;
        _pointer = e.Pointer;
        _pressPoint = e.GetPosition(parts.Items);

        // The cell of the item under the mouse stays under the mouse while dragging.
        _grabColumn = Math.Clamp((int)(_pressPoint.X / parts.CellWidth) - item.Column, 0, item.Width - 1);
        _grabRow = Math.Clamp((int)(_pressPoint.Y / parts.CellHeight) - item.Row, 0, item.Height - 1);
    }

    private void GridItems_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_pressedItem is null || _parts is not { } parts || !ReferenceEquals(sender, parts.Items))
            return;

        if (!e.GetCurrentPoint(parts.Items).Properties.IsLeftButtonPressed)
        {
            EndDrag();
            return;
        }

        var position = e.GetPosition(parts.Items);
        if (!_isDragging)
        {
            if (Math.Abs(position.X - _pressPoint.X) < DragThreshold && Math.Abs(position.Y - _pressPoint.Y) < DragThreshold)
                return;

            StartDrag(parts, _pressedItem, e.Pointer);
        }

        UpdateTarget(parts, _pressedItem, position);
        e.Handled = true;
    }

    private void GridItems_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left)
            return;

        if (!_isDragging)
        {
            // A click: the button opens the inspector.
            _pressedItem = null;
            _parts = null;
            _pointer = null;
            return;
        }

        if (_pressedItem is { } item && _canDrop)
            _parts?.Grid?.MoveItem(item, _targetColumn, _targetRow);

        EndDrag();

        // The drag took the pointer from the button, but make sure the release is not seen as a click.
        e.Handled = true;
    }

    private void GridItems_OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        // The item's button losing the pointer to the drag bubbles here too; only our own capture ends a drag.
        if (_isDragging && _parts is { } parts && ReferenceEquals(e.Source, parts.Items))
            EndDrag();
    }

    /// <summary>
    /// A right-click on an empty cell builds a new item there: the backpack offers the item types, the idol grid opens
    /// the inspector for a new idol at once. A right-click on an item offers to edit or remove it.
    /// </summary>
    private void GridItems_OnRightButtonReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Right
            || _isDragging
            || PartsOf(sender) is not { CellWidth: > 0, CellHeight: > 0 } parts)
            return;

        if (FindItem(e.Source as Visual, parts.Items) is { } item)
        {
            Action? editMaterial = item.IsMaterial ? () => ExecuteInventory(i => i.EditCraftingMaterialCommand, item) : null;
            ShowItemMenu(parts.Items, item.Equipment, () => ExecuteInventory(i => i.RemoveGridItemCommand, item), editMaterial);
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
            Control anchor = CellAt(parts, grid, column, row) ?? parts.Items;
            // After the click is done, so the inspector keeps the focus it takes.
            Dispatcher.UIThread.Post(() => BuildNewItem(onlySlot, grid, column, row, anchor), DispatcherPriority.Background);
        }
        else
        {
            ShowNewItemMenu(parts, grid, column, row);
        }

        e.Handled = true;
    }

    /// <summary>A right-click on an equipment slot (or the idol altar slot) that holds an item offers to edit or remove it.</summary>
    private void EquipmentGrid_OnRightButtonReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Right)
            return;

        for (var node = e.Source as Visual; node is not null && !ReferenceEquals(node, sender); node = node.GetVisualParent())
        {
            if (node is not Button { DataContext: EquipmentSlotViewModel slot } button)
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
    /// Context menu of an item: "Edit…" (when the editor can show it: <paramref name="slot"/> is set for equipment,
    /// <paramref name="onEdit"/> for a crafting material) and "Remove".
    /// </summary>
    private void ShowItemMenu(Control target, EquipmentSlotViewModel? slot, Action remove, Action? onEdit = null)
    {
        var menu = CreateMenu();

        if (slot is not null)
        {
            var edit = CreateMenuItem("Edit…", "PencilOutline");
            // Through the slot's own button, so the inspector opens next to it.
            edit.Click += (_, _) => Dispatcher.UIThread.Post(() =>
            {
                if (FindSlotButton(target, slot) is { } button)
                {
                    button.Focus();
                    if (button.Command?.CanExecute(button.CommandParameter) == true)
                        button.Command.Execute(button.CommandParameter);
                }
            }, DispatcherPriority.Background);
            menu.Items.Add(edit);
            menu.Items.Add(MenuSeparator());
        }
        else if (onEdit is not null)
        {
            var editMaterial = CreateMenuItem("Edit…", "PencilOutline");
            // After the menu has closed, so the picker takes the focus.
            editMaterial.Click += (_, _) => Dispatcher.UIThread.Post(onEdit, DispatcherPriority.Background);
            menu.Items.Add(editMaterial);
            menu.Items.Add(MenuSeparator());
        }

        var removeItem = CreateMenuItem("Remove", "DeleteOutline");
        // After the menu has closed: removing asks for confirmation first.
        removeItem.Click += (_, _) => Dispatcher.UIThread.Post(remove, DispatcherPriority.Background);
        menu.Items.Add(removeItem);

        menu.Open(target);
    }

    /// <summary>A themed context menu opened at the mouse.</summary>
    private ContextMenu CreateMenu() => new()
    {
        Placement = PlacementMode.Pointer,
        Theme = FindTheme("ThemedContextMenuStyle")
    };

    private MenuItem CreateMenuItem(string header, string? icon) => new()
    {
        Header = header,
        Icon = icon is null ? null : MenuIcon(icon),
        Theme = FindTheme("ThemedMenuItemStyle")
    };

    private ControlTheme? FindTheme(string key) =>
        this.TryFindResource(key, ActualThemeVariant, out var value) ? value as ControlTheme : null;

    /// <summary>The button bound to <paramref name="slot"/> at or under <paramref name="root"/>.</summary>
    private static Button? FindSlotButton(Visual root, EquipmentSlotViewModel slot)
    {
        if (root is Button { DataContext: var context } self && ReferenceEquals(context, slot))
            return self;

        return root.GetVisualDescendants().OfType<Button>().FirstOrDefault(b => ReferenceEquals(b.DataContext, slot));
    }

    private void ExecuteInventory(Func<InventoryViewModel, System.Windows.Input.ICommand> command, object parameter)
    {
        if (DataContext is InventoryViewModel inventory && command(inventory) is { } c && c.CanExecute(parameter))
            c.Execute(parameter);
    }

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (!_isDragging || e.Key != Key.Escape)
            return;

        EndDrag();
        e.Handled = true;
    }

    private void StartDrag(GridParts parts, BackpackItemViewModel item, IPointer pointer)
    {
        _isDragging = true;
        _pointer = pointer;
        item.IsDragging = true;
        parts.Preview.IsVisible = true;
        if (TopLevel.GetTopLevel(this) is { } top)
            top.Cursor = new Cursor(StandardCursorType.SizeAll);

        // Keeps the pointer events coming while the pointer is over other items or outside of the grid.
        pointer.Capture(parts.Items);
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
        var targetColumn = Math.Clamp(column, 0, Math.Max(0, grid.ColumnCount - item.Width));
        var targetRow = Math.Clamp(row, 0, Math.Max(0, grid.RowCount - item.Height));

        // Pointer moves inside the same target cell change nothing: skip the placement check and the re-layout.
        if (parts.Preview.IsVisible && targetColumn == _targetColumn && targetRow == _targetRow && parts.Preview.Width > 0)
            return;

        _targetColumn = targetColumn;
        _targetRow = targetRow;
        _canDrop = grid.CanPlace(item.Footprint, _targetColumn, _targetRow, item);

        var preview = parts.Preview;
        Canvas.SetLeft(preview, _targetColumn * cellWidth + 1);
        Canvas.SetTop(preview, _targetRow * cellHeight + 1);
        preview.Width = Math.Max(0, item.Width * cellWidth - 2);
        preview.Height = Math.Max(0, item.Height * cellHeight - 2);

        var brushKey = _canDrop ? ValidDropBrushKey : InvalidDropBrushKey;
        preview.Bind(Border.BorderBrushProperty, preview.GetResourceObservable(brushKey));
        preview.Background = this.TryFindResource(brushKey, ActualThemeVariant, out var value) && value is ISolidColorBrush brush
            ? new ImmutableSolidColorBrush(brush.Color, DropFillOpacity)
            : null;
    }

    /// <summary>Ends a drag (or forgets a press); the item stays where it is unless it was moved before.</summary>
    private void EndDrag()
    {
        var wasDragging = _isDragging;
        var parts = _parts;
        var pointer = _pointer;
        _isDragging = false;
        _canDrop = false;
        _parts = null;
        _pointer = null;

        if (_pressedItem is { } item)
            item.IsDragging = false;
        _pressedItem = null;

        if (!wasDragging || parts is null)
            return;

        parts.Preview.IsVisible = false;
        parts.Preview.Width = 0;
        if (TopLevel.GetTopLevel(this) is { } top)
            top.ClearValue(CursorProperty);

        // Releasing raises PointerCaptureLost; _isDragging is already false, so it is ignored.
        if (pointer?.Captured == parts.Items)
            pointer.Capture(null);
    }

    /// <summary>The background cell of <paramref name="grid"/> at <paramref name="column"/>, <paramref name="row"/>.</summary>
    private static Control? CellAt(GridParts parts, BackpackViewModel grid, int column, int row) =>
        parts.Cells.ContainerFromIndex(row * grid.ColumnCount + column);

    /// <summary>
    /// Offers the item types that can be built in the empty cell <paramref name="column"/>, <paramref name="row"/>;
    /// types with no room left anywhere in the grid are disabled.
    /// </summary>
    private void ShowNewItemMenu(GridParts parts, BackpackViewModel grid, int column, int row)
    {
        var anchor = CellAt(parts, grid, column, row);
        var menu = CreateMenu();

        // Title: a menu item that can be neither highlighted nor clicked.
        var title = new MenuItem
        {
            Header = new TextBlock
            {
                Text = "Build a new item",
                FontWeight = FontWeight.SemiBold,
                Theme = FindTheme("SecondaryTextStyle")
            },
            Theme = FindTheme("ThemedMenuItemStyle")
        };
        title.Classes.Add("title");
        menu.Items.Add(title);

        foreach (var entry in NewItemSlotGroups.SelectMany(group => group.Select((s, i) => (Slot: s, StartsGroup: i == 0))))
        {
            // Every group, the first included (it follows the title), starts with a separator.
            if (entry.StartsGroup)
                menu.Items.Add(MenuSeparator());

            AddNewItemEntry(entry.Slot);
        }

        menu.Open(parts.Items);

        void AddNewItemEntry(EquipmentSlot slot)
        {
            var hasRoom = grid.HasRoomFor(slot);
            var menuItem = CreateMenuItem(slot.DisplayName, IconNameOf(slot));
            menuItem.IsEnabled = hasRoom;
            if (!hasRoom)
            {
                ToolTip.SetTip(menuItem, $"No room left in the {grid.Layout.Name} for this item type");
                ToolTip.SetShowOnDisabled(menuItem, true);
            }

            var chosen = slot;
            // After the menu has closed, so the inspector keeps the focus it takes.
            menuItem.Click += (_, _) => Dispatcher.UIThread.Post(
                () => BuildNewItem(chosen, grid, column, row, anchor ?? parts.Items),
                DispatcherPriority.Background);
            menu.Items.Add(menuItem);
        }
    }

    /// <summary>A 16 px Material icon for a menu entry; <c>null</c> (no icon) for an unknown name.</summary>
    private static MaterialIcon? MenuIcon(string kind) =>
        Enum.TryParse<MaterialIconKind>(kind, out var iconKind)
            ? new MaterialIcon { Kind = iconKind, Width = 16, Height = 16 }
            : null;

    /// <summary>A themed line between groups of a menu.</summary>
    private Separator MenuSeparator() => new() { Theme = FindTheme("ThemedMenuSeparatorStyle") };

    private void BuildNewItem(EquipmentSlot slot, BackpackViewModel grid, int column, int row, Control? anchor)
    {
        if (DataContext is not InventoryViewModel inventory)
            return;

        var request = new NewBackpackItemRequest(slot, column, row, anchor, grid);
        if (inventory.BuildBackpackItemCommand.CanExecute(request))
            inventory.BuildBackpackItemCommand.Execute(request);
    }

    /// <summary>The grid item shown by <paramref name="source"/> or one of its ancestors inside the item layer <paramref name="items"/>.</summary>
    private static BackpackItemViewModel? FindItem(Visual? source, ItemsControl items)
    {
        for (var node = source; node is not null && !ReferenceEquals(node, items); node = node.GetVisualParent())
        {
            if (node is StyledElement { DataContext: BackpackItemViewModel item })
                return item;
        }

        return null;
    }
}

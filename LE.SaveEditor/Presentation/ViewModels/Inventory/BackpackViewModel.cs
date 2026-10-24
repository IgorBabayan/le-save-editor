using System.Collections.ObjectModel;
using System.ComponentModel;
using LE.SaveEditor.Presentation.Services.Toasts;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Notifications;
using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// Shape of a grid container: the backpack (<see cref="Backpack"/>) or the idol grid (<see cref="Idols"/>).
/// </summary>
/// <param name="ContainerId"><c>containerID</c> of the items in <c>savedItems</c>.</param>
/// <param name="Columns">Width in cells.</param>
/// <param name="Rows">Height in cells.</param>
/// <param name="Name">Name used in messages ("backpack", "idol grid").</param>
/// <param name="BlockedCells">Cells that can never hold an item (column, row counted from the top).</param>
/// <param name="OnlySlot">The only item kind the grid takes (<see cref="EquipmentSlot.Idol"/>), or <c>null</c> for any item.</param>
/// <param name="RelocateOnLoad">
/// Items of the save whose stored cell has no room go to the first free spot (backpack); otherwise they are left out
/// and stay in the save untouched (idol grid, whose cells the game may lock).
/// </param>
public sealed record ItemGridLayout(
    int ContainerId,
    int Columns,
    int Rows,
    string Name,
    IReadOnlySet<(int Column, int Row)> BlockedCells,
    EquipmentSlot? OnlySlot,
    bool RelocateOnLoad)
{
    /// <summary>Container of idols in <c>savedItems</c>.</summary>
    public const int IdolContainerId = 29;

    /// <summary>Side length of the idol grid.</summary>
    public const int IdolGridSize = 5;

    /// <summary>The character's backpack: 14 × 8 cells, any item.</summary>
    public static ItemGridLayout Backpack { get; } = new(
        SavedItem.BackpackContainerId, BackpackViewModel.Columns, BackpackViewModel.Rows, "backpack",
        new HashSet<(int, int)>(), null, RelocateOnLoad: true);

    /// <summary>
    /// The idol grid: 5 × 5 cells without the four corners, as in the game; only idols. Idols stored over a corner are
    /// left as they are.
    /// </summary>
    public static ItemGridLayout Idols { get; } = new(
        IdolContainerId, IdolGridSize, IdolGridSize, "idol grid",
        new HashSet<(int, int)> { (0, 0), (IdolGridSize - 1, 0), (0, IdolGridSize - 1), (IdolGridSize - 1, IdolGridSize - 1) },
        EquipmentSlot.Idol, RelocateOnLoad: false);

    public bool IsBlocked(int column, int row) => BlockedCells.Contains((column, row));

    /// <summary>Whether an item of type <paramref name="baseTypeId"/> may be kept in this grid.</summary>
    public bool Accepts(int baseTypeId) => OnlySlot is not { } slot || slot.AcceptsBaseType(baseTypeId);
}

/// <summary>
/// A grid container of the character: the backpack (a <see cref="Columns"/> × <see cref="Rows"/> grid holding the items of
/// container <see cref="SavedItem.BackpackContainerId"/>) or, with <see cref="ItemGridLayout.Idols"/>, the idol grid.
/// Items cover several cells (<see cref="ItemFootprint"/>), never overlap and never cover a blocked cell.
/// </summary>
public sealed partial class BackpackViewModel : ViewModelBase
{
    /// <summary>Width of the backpack in cells.</summary>
    public const int Columns = 14;

    /// <summary>Height of the backpack in cells.</summary>
    public const int Rows = 8;

    private readonly IToastService _toastService;
    private readonly IIconProvider _iconProvider;
    private bool _isReverting;

    /// <summary>Items placed when the save was read, including the ones removed since (for <see cref="GetUpdates"/>).</summary>
    private readonly List<BackpackItemViewModel> _loadedItems = [];

    /// <summary>An item was moved, changed or removed.</summary>
    public event EventHandler? Changed;

    public BackpackViewModel(
        IEnumerable<SavedItem> savedItems,
        IIconProvider iconProvider,
        IItemBaseCatalog itemBaseCatalog,
        IToastService toastService)
        : this(savedItems, iconProvider, itemBaseCatalog, toastService, ItemGridLayout.Backpack)
    {
    }

    public BackpackViewModel(
        IEnumerable<SavedItem> savedItems,
        IIconProvider iconProvider,
        IItemBaseCatalog itemBaseCatalog,
        IToastService toastService,
        ItemGridLayout layout)
    {
        _toastService = toastService;
        _iconProvider = iconProvider;
        Layout = layout;
        Cells = CreateCells(layout);
        Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(Summary));
        Load(savedItems, iconProvider, itemBaseCatalog);
    }

    /// <summary>Shape of the grid.</summary>
    public ItemGridLayout Layout { get; }

    /// <summary>Width of the grid in cells.</summary>
    public int ColumnCount => Layout.Columns;

    /// <summary>Height of the grid in cells.</summary>
    public int RowCount => Layout.Rows;

    /// <summary>Background cells, row by row.</summary>
    public IReadOnlyList<BackpackCellViewModel> Cells { get; }

    /// <summary>Items in the grid, each placed at its own cells.</summary>
    public ObservableCollection<BackpackItemViewModel> Items { get; } = [];

    /// <summary>Items in the save that had no room in the grid (overlapping or outside of it) and are not shown.</summary>
    public int SkippedCount { get; private set; }

    /// <summary>Number of cells no item covers (blocked cells do not count).</summary>
    public int FreeCells => ColumnCount * RowCount - Layout.BlockedCells.Count - Items.Sum(i => i.Width * i.Height);

    /// <summary>Line under the grid, e.g. "5 items · 87 free cells".</summary>
    public string Summary
    {
        get
        {
            var text = $"{Items.Count} {(Items.Count == 1 ? "item" : "items")} · {FreeCells} free cells";
            return SkippedCount > 0 ? $"{text} · {SkippedCount} not shown (no room)" : text;
        }
    }

    /// <summary>
    /// Places the items of the grid's container. An item keeps its stored cell when it fits there; otherwise it goes
    /// to the first free spot (when the layout relocates items) or is counted in <see cref="SkippedCount"/>.
    /// </summary>
    private void Load(IEnumerable<SavedItem> savedItems, IIconProvider iconProvider, IItemBaseCatalog catalog)
    {
        var pending = new List<BackpackItemViewModel>();

        // Top rows first (the highest stored y), so the grid fills like the game's.
        foreach (var saved in savedItems
                     .Where(s => s.ContainerId == Layout.ContainerId)
                     .OrderByDescending(s => s.Y)
                     .ThenBy(s => s.X))
        {
            if (ItemDataReader.ReadBaseTypeId(saved.Data) is not { } baseTypeId)
                continue;

            var equipment = CreateEquipment(saved, baseTypeId, iconProvider, catalog);
            var footprint = ItemFootprint.Of(equipment?.Item?.Base.BaseTypeId ?? baseTypeId);
            var item = new BackpackItemViewModel(saved, baseTypeId, saved.X, RowFromSaved(saved.Y, footprint, RowCount), equipment,
                gridRows: RowCount);
            if (CanPlace(item.Footprint, item.Column, item.Row, ignore: null))
                Add(item);
            else
                pending.Add(item);
        }

        // Items whose stored cell is taken or outside of the grid go wherever they fit.
        foreach (var item in pending)
        {
            if (Layout.RelocateOnLoad && FindFreeSpot(item.Footprint, ignore: null) is { } spot)
            {
                item.Column = spot.Column;
                item.Row = spot.Row;
                Add(item);
            }
            else
            {
                // Not shown, so it is never changed: it stays in the save as it is.
                SkippedCount++;
            }
        }

        foreach (var item in Items)
        {
            item.MarkLoaded();
            _loadedItems.Add(item);
        }
    }

    /// <summary>
    /// Changes to write into <c>savedItems</c>: moved and edited items, items removed from the grid and items
    /// built in its empty cells. Items that were not shown (no room, see <see cref="SkippedCount"/>) are left as they are.
    /// </summary>
    /// <exception cref="ArgumentException">An edited or new item cannot be stored.</exception>
    public IEnumerable<SavedItemUpdate> GetUpdates()
    {
        foreach (var item in _loadedItems)
        {
            if (!Items.Contains(item))
                yield return SavedItemUpdate.Remove(item.Source);
            else if (item.ToSavedItem() is { } replacement)
                yield return SavedItemUpdate.Replace(item.Source, replacement);
        }

        foreach (var item in Items.Where(i => i.IsNew))
            yield return SavedItemUpdate.Add(item.ToNewSavedItem());
    }

    /// <summary>
    /// Whether no item covers the cell in <paramref name="column"/>, <paramref name="row"/> and it is inside the grid
    /// and not blocked.
    /// </summary>
    public bool IsCellFree(int column, int row) =>
        column >= 0 && column < ColumnCount && row >= 0 && row < RowCount
        && !Layout.IsBlocked(column, row)
        && !Items.Any(i => i.Covers(column, row));

    /// <summary>Whether at least one item type of <paramref name="slot"/> still fits somewhere in the grid.</summary>
    public bool HasRoomFor(EquipmentSlot slot) =>
        slot.BaseTypes.Where(Layout.Accepts).Select(ItemFootprint.Of).Distinct().Any(f => FindFreeSpot(f, ignore: null) is not null);

    /// <summary>
    /// Puts an item built in the empty cell <paramref name="column"/>, <paramref name="row"/> into the grid.
    /// It takes that cell as its top-left corner when it fits there, else the nearest spot that still covers the
    /// cell, else the first free spot; when it fits nowhere it is not added.
    /// </summary>
    /// <returns><c>true</c> if the item was added.</returns>
    public bool AddItem(BuiltItem built, int column, int row)
    {
        var baseTypeId = built.Base.BaseTypeId;
        if (!Layout.Accepts(baseTypeId))
        {
            _toastService.Show($"{built.Base.Name} cannot be put into the {Layout.Name}.", NotificationSeverity.Warning);
            return false;
        }

        var footprint = ItemFootprint.Of(baseTypeId);
        var spot = FindSpotCovering(footprint, column, row);
        var moved = false;
        if (spot is null)
        {
            spot = FindFreeSpot(footprint, ignore: null);
            moved = spot is not null;
        }

        if (spot is not { } place)
        {
            _toastService.Show(
                $"There is no room in the {Layout.Name} for {built.Base.Name} ({footprint.Width}×{footprint.Height}); the item was not added.",
                NotificationSeverity.Warning);
            return false;
        }

        var slot = EquipmentSlotExtensions.FromBaseTypeId(baseTypeId) ?? built.Slot;
        var equipment = new EquipmentSlotViewModel(slot, _iconProvider) { Item = built, IsInGrid = true };
        var source = new SavedItem(
            Layout.ContainerId,
            place.Column,
            SavedYFromRow(place.Row, footprint, RowCount),
            1,
            ItemDataReader.Version2,
            ItemDataWriter.Write(SavedItemConverter.ToItemData(built, null)));

        Add(new BackpackItemViewModel(source, baseTypeId, place.Column, place.Row, equipment, isNew: true, gridRows: RowCount));
        Changed?.Invoke(this, EventArgs.Empty);

        if (moved)
            _toastService.Show($"{built.Base.Name} did not fit at the chosen cell and was put in another spot of the {Layout.Name}.");
        return true;
    }

    /// <summary>
    /// A spot for <paramref name="footprint"/> that covers the cell <paramref name="column"/>, <paramref name="row"/>:
    /// that cell as the top-left corner first, then shifted left and up one cell at a time.
    /// </summary>
    private (int Column, int Row)? FindSpotCovering(ItemFootprint footprint, int column, int row)
    {
        for (var up = 0; up < footprint.Height; up++)
        for (var left = 0; left < footprint.Width; left++)
        {
            if (CanPlace(footprint, column - left, row - up, ignore: null))
                return (column - left, row - up);
        }

        return null;
    }

    /// <summary>
    /// Top row in a grid of <paramref name="rows"/> rows of an item stored at <paramref name="savedY"/>. The game counts
    /// rows from the bottom and stores the item's bottom-left cell, while the grid counts rows from the top.
    /// </summary>
    public static int RowFromSaved(int savedY, ItemFootprint footprint, int rows = Rows) => rows - savedY - footprint.Height;

    /// <summary>The stored y of an item whose top row in the grid is <paramref name="row"/> (inverse of <see cref="RowFromSaved"/>).</summary>
    public static int SavedYFromRow(int row, ItemFootprint footprint, int rows = Rows) => rows - row - footprint.Height;

    /// <summary>
    /// A slot holding the decoded item; <c>null</c> when it is not an equipment item or idol the editor knows, or does
    /// not belong in this grid.
    /// </summary>
    private EquipmentSlotViewModel? CreateEquipment(SavedItem saved, int baseTypeId, IIconProvider iconProvider, IItemBaseCatalog catalog)
    {
        if (!Layout.Accepts(baseTypeId)
            || EquipmentSlotExtensions.FromBaseTypeId(baseTypeId) is not { } slot
            || ItemDataReader.Read(saved.Data) is not { } data
            || SavedItemConverter.ToBuiltItem(data, slot, catalog) is not { } built)
            return null;

        return new EquipmentSlotViewModel(slot, iconProvider) { Item = built, IsInGrid = true };
    }

    private void Add(BackpackItemViewModel item)
    {
        if (item.Equipment is { } equipment)
            equipment.PropertyChanged += (_, e) => OnEquipmentChanged(item, e);

        Items.Add(item);
    }

    /// <summary>
    /// The inspector applied a new item. A different item type may need another number of cells: the item grows in
    /// place when it can, moves to the first free spot otherwise, and the change is undone when nothing fits.
    /// </summary>
    private void OnEquipmentChanged(BackpackItemViewModel item, PropertyChangedEventArgs e)
    {
        if (_isReverting || e.PropertyName != nameof(EquipmentSlotViewModel.Item) || item.Equipment is not { } equipment)
            return;

        if (equipment.Item is not { } built)
        {
            Items.Remove(item);
            Changed?.Invoke(this, EventArgs.Empty);
            return;
        }

        var footprint = ItemFootprint.Of(built.Base.BaseTypeId);
        if (footprint == item.Footprint || CanPlace(footprint, item.Column, item.Row, item))
        {
            Accept(item, built, footprint);
            return;
        }

        if (FindFreeSpot(footprint, item) is { } spot)
        {
            item.Column = spot.Column;
            item.Row = spot.Row;
            Accept(item, built, footprint);
            _toastService.Show($"{built.Base.Name} was moved to another spot in the {Layout.Name} to make room for it.");
            return;
        }

        _isReverting = true;
        try
        {
            equipment.Item = item.AcceptedItem;
        }
        finally
        {
            _isReverting = false;
        }

        _toastService.Show(
            $"There is no room in the {Layout.Name} for {built.Base.Name} ({footprint.Width}×{footprint.Height}); the item was not changed.",
            NotificationSeverity.Warning);
    }

    private void Accept(BackpackItemViewModel item, BuiltItem built, ItemFootprint footprint)
    {
        item.Footprint = footprint;
        item.AcceptedItem = built;
        OnPropertyChanged(nameof(Summary));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Takes <paramref name="item"/> out of the grid (any item, also placeholders the editor cannot show in detail).
    /// An item read from the save is deleted from it on the next save; a new item is simply dropped.
    /// </summary>
    /// <returns><c>true</c> if the item was in the grid.</returns>
    public bool RemoveItem(BackpackItemViewModel item)
    {
        if (!Items.Remove(item))
            return false;

        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Moves <paramref name="item"/> so its top-left cell is <paramref name="column"/>, <paramref name="row"/>
    /// (dropped after a drag). Nothing changes when it does not fit there.
    /// </summary>
    /// <returns><c>true</c> if the item was moved.</returns>
    public bool MoveItem(BackpackItemViewModel item, int column, int row)
    {
        if (!Items.Contains(item) || !CanPlace(item.Footprint, column, row, item))
            return false;

        if (item.Column == column && item.Row == row)
            return true;

        item.Column = column;
        item.Row = row;
        Changed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    /// Whether an item of <paramref name="footprint"/> fits with its top-left corner in <paramref name="column"/>,
    /// <paramref name="row"/>: inside the grid, over no blocked cell and over no other item but <paramref name="ignore"/>.
    /// </summary>
    public bool CanPlace(ItemFootprint footprint, int column, int row, BackpackItemViewModel? ignore)
    {
        if (column < 0 || row < 0 || column + footprint.Width > ColumnCount || row + footprint.Height > RowCount)
            return false;

        for (var r = row; r < row + footprint.Height; r++)
        for (var c = column; c < column + footprint.Width; c++)
        {
            if (Layout.IsBlocked(c, r) || Items.Any(other => !ReferenceEquals(other, ignore) && other.Covers(c, r)))
                return false;
        }

        return true;
    }

    /// <summary>The first cell (row by row, as the game fills the backpack) where <paramref name="footprint"/> fits.</summary>
    public (int Column, int Row)? FindFreeSpot(ItemFootprint footprint, BackpackItemViewModel? ignore)
    {
        for (var row = 0; row <= RowCount - footprint.Height; row++)
        for (var column = 0; column <= ColumnCount - footprint.Width; column++)
        {
            if (CanPlace(footprint, column, row, ignore))
                return (column, row);
        }

        return null;
    }

    private static IReadOnlyList<BackpackCellViewModel> CreateCells(ItemGridLayout layout)
    {
        var cells = new List<BackpackCellViewModel>(layout.Columns * layout.Rows);
        for (var row = 0; row < layout.Rows; row++)
        for (var column = 0; column < layout.Columns; column++)
            cells.Add(new BackpackCellViewModel(column, row, layout.IsBlocked(column, row)));

        return cells;
    }
}

/// <summary>One background cell of a grid; <see cref="IsBlocked"/> cells can never hold an item.</summary>
public sealed record BackpackCellViewModel(int Column, int Row, bool IsBlocked = false);

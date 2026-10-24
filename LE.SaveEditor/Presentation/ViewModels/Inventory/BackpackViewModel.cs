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
/// The character's backpack: a <see cref="Columns"/> × <see cref="Rows"/> grid holding the items of container
/// <see cref="SavedItem.BackpackContainerId"/>. Items cover several cells (<see cref="ItemFootprint"/>) and never overlap.
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
    {
        _toastService = toastService;
        _iconProvider = iconProvider;
        Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(Summary));
        Load(savedItems, iconProvider, itemBaseCatalog);
    }

    /// <summary>Background cells, row by row.</summary>
    public IReadOnlyList<BackpackCellViewModel> Cells { get; } = CreateCells();

    /// <summary>Items in the backpack, each placed at its own cells.</summary>
    public ObservableCollection<BackpackItemViewModel> Items { get; } = [];

    /// <summary>Items in the save that had no room in the grid (overlapping or outside of it) and are not shown.</summary>
    public int SkippedCount { get; private set; }

    /// <summary>Number of cells no item covers.</summary>
    public int FreeCells => Columns * Rows - Items.Sum(i => i.Width * i.Height);

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
    /// Places the backpack items of the save. An item keeps its stored cell when it fits there, otherwise it goes to
    /// the first free spot; items that fit nowhere are counted in <see cref="SkippedCount"/>.
    /// </summary>
    private void Load(IEnumerable<SavedItem> savedItems, IIconProvider iconProvider, IItemBaseCatalog catalog)
    {
        var pending = new List<BackpackItemViewModel>();

        // Top rows first (the highest stored y), so the grid fills like the game's.
        foreach (var saved in savedItems
                     .Where(s => s.ContainerId == SavedItem.BackpackContainerId)
                     .OrderByDescending(s => s.Y)
                     .ThenBy(s => s.X))
        {
            if (ItemDataReader.ReadBaseTypeId(saved.Data) is not { } baseTypeId)
                continue;

            var equipment = CreateEquipment(saved, baseTypeId, iconProvider, catalog);
            var footprint = ItemFootprint.Of(equipment?.Item?.Base.BaseTypeId ?? baseTypeId);
            var item = new BackpackItemViewModel(saved, baseTypeId, saved.X, RowFromSaved(saved.Y, footprint), equipment);
            if (CanPlace(item.Footprint, item.Column, item.Row, ignore: null))
                Add(item);
            else
                pending.Add(item);
        }

        // Items whose stored cell is taken or outside of the grid go wherever they fit.
        foreach (var item in pending)
        {
            if (FindFreeSpot(item.Footprint, ignore: null) is { } spot)
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
    /// Changes to write into <c>savedItems</c>: moved and edited items, items removed from the backpack and items
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

    /// <summary>Whether no item covers the cell in <paramref name="column"/>, <paramref name="row"/> (inside the grid).</summary>
    public bool IsCellFree(int column, int row) =>
        column is >= 0 and < Columns && row is >= 0 and < Rows && !Items.Any(i => i.Covers(column, row));

    /// <summary>Whether at least one item type of <paramref name="slot"/> still fits somewhere in the backpack.</summary>
    public bool HasRoomFor(EquipmentSlot slot) =>
        slot.BaseTypes.Select(ItemFootprint.Of).Distinct().Any(f => FindFreeSpot(f, ignore: null) is not null);

    /// <summary>
    /// Puts an item built in the empty cell <paramref name="column"/>, <paramref name="row"/> into the backpack.
    /// It takes that cell as its top-left corner when it fits there, else the nearest spot that still covers the
    /// cell, else the first free spot; when it fits nowhere it is not added.
    /// </summary>
    /// <returns><c>true</c> if the item was added.</returns>
    public bool AddItem(BuiltItem built, int column, int row)
    {
        var baseTypeId = built.Base.BaseTypeId;
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
                $"There is no room in the backpack for {built.Base.Name} ({footprint.Width}×{footprint.Height}); the item was not added.",
                NotificationSeverity.Warning);
            return false;
        }

        var slot = EquipmentSlotExtensions.FromBaseTypeId(baseTypeId) ?? built.Slot;
        var equipment = new EquipmentSlotViewModel(slot, _iconProvider) { Item = built };
        var source = new SavedItem(
            SavedItem.BackpackContainerId,
            place.Column,
            SavedYFromRow(place.Row, footprint),
            1,
            ItemDataReader.Version2,
            ItemDataWriter.Write(SavedItemConverter.ToItemData(built, null)));

        Add(new BackpackItemViewModel(source, baseTypeId, place.Column, place.Row, equipment, isNew: true));
        Changed?.Invoke(this, EventArgs.Empty);

        if (moved)
            _toastService.Show($"{built.Base.Name} did not fit at the chosen cell and was put in another spot of the backpack.");
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
    /// Top row in the grid of an item stored at <paramref name="savedY"/>. The game counts rows from the bottom of the
    /// backpack and stores the item's bottom-left cell, while the grid counts rows from the top.
    /// </summary>
    public static int RowFromSaved(int savedY, ItemFootprint footprint) => Rows - savedY - footprint.Height;

    /// <summary>The stored y of an item whose top row in the grid is <paramref name="row"/> (inverse of <see cref="RowFromSaved"/>).</summary>
    public static int SavedYFromRow(int row, ItemFootprint footprint) => Rows - row - footprint.Height;

    /// <summary>An equipment slot holding the decoded item; <c>null</c> when it is not an equipment item the editor knows.</summary>
    private static EquipmentSlotViewModel? CreateEquipment(SavedItem saved, int baseTypeId, IIconProvider iconProvider, IItemBaseCatalog catalog)
    {
        if (EquipmentSlotExtensions.FromBaseTypeId(baseTypeId) is not { } slot
            || ItemDataReader.Read(saved.Data) is not { } data
            || SavedItemConverter.ToBuiltItem(data, slot, catalog) is not { } built)
            return null;

        return new EquipmentSlotViewModel(slot, iconProvider) { Item = built };
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
            _toastService.Show($"{built.Base.Name} was moved to another spot in the backpack to make room for it.");
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
            $"There is no room in the backpack for {built.Base.Name} ({footprint.Width}×{footprint.Height}); the item was not changed.",
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
    /// <paramref name="row"/>: inside the grid and over no other item but <paramref name="ignore"/>.
    /// </summary>
    public bool CanPlace(ItemFootprint footprint, int column, int row, BackpackItemViewModel? ignore)
    {
        if (column < 0 || row < 0 || column + footprint.Width > Columns || row + footprint.Height > Rows)
            return false;

        for (var r = row; r < row + footprint.Height; r++)
        for (var c = column; c < column + footprint.Width; c++)
        {
            if (Items.Any(other => !ReferenceEquals(other, ignore) && other.Covers(c, r)))
                return false;
        }

        return true;
    }

    /// <summary>The first cell (row by row, as the game fills the backpack) where <paramref name="footprint"/> fits.</summary>
    public (int Column, int Row)? FindFreeSpot(ItemFootprint footprint, BackpackItemViewModel? ignore)
    {
        for (var row = 0; row <= Rows - footprint.Height; row++)
        for (var column = 0; column <= Columns - footprint.Width; column++)
        {
            if (CanPlace(footprint, column, row, ignore))
                return (column, row);
        }

        return null;
    }

    private static IReadOnlyList<BackpackCellViewModel> CreateCells()
    {
        var cells = new List<BackpackCellViewModel>(Columns * Rows);
        for (var row = 0; row < Rows; row++)
        for (var column = 0; column < Columns; column++)
            cells.Add(new BackpackCellViewModel(column, row));

        return cells;
    }
}

/// <summary>One background cell of the backpack grid.</summary>
public sealed record BackpackCellViewModel(int Column, int Row);

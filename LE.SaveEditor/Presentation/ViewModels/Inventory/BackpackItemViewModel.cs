using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// An item in the backpack grid: its top-left cell and the cells it covers.
/// Equipment items the editor can decode carry an <see cref="Equipment"/> slot, so they show their icon and tooltip
/// and open the item inspector like an equipped item. Anything else (idols, keys, items whose base is not in
/// <c>base.db</c>) is shown as a placeholder that keeps its cells taken.
/// </summary>
public sealed partial class BackpackItemViewModel : ViewModelBase
{
    private const int FirstIdolTypeId = 25;
    private const int LastIdolTypeId = 33;

    public BackpackItemViewModel(SavedItem source, int baseTypeId, int column, int row, EquipmentSlotViewModel? equipment)
    {
        Source = source;
        BaseTypeId = baseTypeId;
        Equipment = equipment;
        AcceptedItem = equipment?.Item;
        Column = column;
        Row = row;
        Footprint = ItemFootprint.Of(equipment?.Item?.Base.BaseTypeId ?? baseTypeId);
    }

    /// <summary>The entry of <c>savedItems</c> the item was read from.</summary>
    public SavedItem Source { get; }

    /// <summary>Item type as stored in the save (<c>baseTypeID</c>).</summary>
    public int BaseTypeId { get; }

    /// <summary>Decoded equipment item; <c>null</c> for placeholders.</summary>
    public EquipmentSlotViewModel? Equipment { get; }

    public bool IsEquipment => Equipment is not null;

    /// <summary>
    /// Item that last fitted in the backpack; restored when the inspector applies an item that has no room.
    /// </summary>
    internal BuiltItem? AcceptedItem { get; set; }

    /// <summary>Item shown when the save was read; a different <see cref="EquipmentSlotViewModel.Item"/> is written back.</summary>
    private BuiltItem? _loadedItem;

    /// <summary>Where the item was placed when the save was read (it may differ from the stored cell when that had no room).</summary>
    private (int Column, int Row, ItemFootprint Footprint) _loadedPlacement;

    /// <summary>Remembers the current item and cell as the state read from the save (see <see cref="ToSavedItem"/>).</summary>
    internal void MarkLoaded()
    {
        _loadedItem = Equipment?.Item;
        _loadedPlacement = (Column, Row, Footprint);
    }

    /// <summary>
    /// The entry to write back for this item, or <c>null</c> when neither the item nor its cell changed since the save
    /// was read. A moved item stores its new bottom-left cell; an edited item its new bytes.
    /// </summary>
    /// <exception cref="ArgumentException">The edited item cannot be stored.</exception>
    internal SavedItem? ToSavedItem()
    {
        var data = Source.Data;
        if (Equipment?.Item is { } current && !ReferenceEquals(current, _loadedItem))
            data = ItemDataWriter.Write(SavedItemConverter.ToItemData(current, ItemDataReader.Read(Source.Data)));

        var (x, y) = (Column, Row, Footprint) == _loadedPlacement
            ? (Source.X, Source.Y)
            : (Column, BackpackViewModel.SavedYFromRow(Row, Footprint));

        return data.SequenceEqual(Source.Data) && x == Source.X && y == Source.Y
            ? null
            : Source with { X = x, Y = y, Data = data };
    }

    /// <summary>Left column of the item (0-based).</summary>
    [ObservableProperty]
    private int _column;

    /// <summary>Top row of the item (0-based).</summary>
    [ObservableProperty]
    private int _row;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Width))]
    [NotifyPropertyChangedFor(nameof(Height))]
    private ItemFootprint _footprint;

    /// <summary>Columns the item covers.</summary>
    public int Width => Footprint.Width;

    /// <summary>Rows the item covers.</summary>
    public int Height => Footprint.Height;

    /// <summary>The item is being dragged to another spot of the backpack.</summary>
    [ObservableProperty]
    private bool _isDragging;

    public bool IsIdol => BaseTypeId is >= FirstIdolTypeId and <= LastIdolTypeId;

    /// <summary>Short text on a placeholder tile.</summary>
    public string PlaceholderLabel => IsIdol ? "Idol" : "?";

    /// <summary>Tooltip of a placeholder tile.</summary>
    public string PlaceholderToolTip
    {
        get
        {
            if (IsIdol)
                return "Idol — idols cannot be edited yet";

            return EquipmentSlotExtensions.FromBaseTypeId(BaseTypeId) is { } slot
                ? $"{slot.DisplayName} — its base is not in the game database (generate or update it in Settings)"
                : $"Unknown item (type {BaseTypeId})";
        }
    }

    /// <summary>Whether the item covers the cell in <paramref name="column"/>, <paramref name="row"/>.</summary>
    public bool Covers(int column, int row) =>
        column >= Column && column < Column + Width && row >= Row && row < Row + Height;
}

using LE.SaveEditor.Services.GameData.Items;
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

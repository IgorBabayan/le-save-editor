using System.Windows.Media;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// An item in a grid (backpack or idol grid): its top-left cell and the cells it covers.
/// Equipment items and idols the editor can decode carry an <see cref="Equipment"/> slot, so they show their icon and
/// tooltip and open the item inspector like an equipped item. Crafting materials (runes, glyphs, affix shards) carry a
/// <see cref="Material"/> and open the crafting material picker. Anything else (keys, items whose base is not in
/// <c>base.db</c>) is shown as a placeholder that keeps its cells taken.
/// </summary>
public sealed partial class BackpackItemViewModel : ViewModelBase
{
    /// <summary>Number of rows of the grid the item is in (stored rows count from its bottom).</summary>
    private readonly int _gridRows;

    private readonly IIconProvider? _iconProvider;

    /// <param name="isNew">The item was made in the editor and is not in the save yet.</param>
    /// <param name="gridRows">Number of rows of the grid the item is in.</param>
    /// <param name="material">The crafting material the item is, when it is one the editor knows.</param>
    /// <param name="iconProvider">Loads the icon of <paramref name="material"/>.</param>
    public BackpackItemViewModel(
        SavedItem source,
        int baseTypeId,
        int column,
        int row,
        EquipmentSlotViewModel? equipment,
        bool isNew = false,
        int gridRows = BackpackViewModel.Rows,
        CraftingMaterial? material = null,
        IIconProvider? iconProvider = null)
    {
        _gridRows = gridRows;
        _iconProvider = iconProvider;
        Material = equipment is null ? material : null;
        Source = source;
        IsNew = isNew;
        BaseTypeId = baseTypeId;
        Equipment = equipment;
        AcceptedItem = equipment?.Item;
        Column = column;
        Row = row;
        Footprint = ItemFootprint.Of(equipment?.Item?.Base.BaseTypeId ?? baseTypeId);
    }

    /// <summary>The entry of <c>savedItems</c> the item was read from (for a new item, the entry it was made with).</summary>
    public SavedItem Source { get; }

    /// <summary>The item was built in an empty cell of the backpack and is added to the save on the next save.</summary>
    public bool IsNew { get; }

    /// <summary>Item type as stored in the save (<c>baseTypeID</c>).</summary>
    public int BaseTypeId { get; }

    /// <summary>Decoded equipment item; <c>null</c> for placeholders.</summary>
    public EquipmentSlotViewModel? Equipment { get; }

    public bool IsEquipment => Equipment is not null;

    /// <summary>Crafting material (rune, glyph, affix shard) the item is; <c>null</c> for equipment and placeholders.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMaterial))]
    [NotifyPropertyChangedFor(nameof(IsPlaceholder))]
    [NotifyPropertyChangedFor(nameof(MaterialIcon))]
    [NotifyPropertyChangedFor(nameof(HasMaterialIcon))]
    [NotifyPropertyChangedFor(nameof(MaterialName))]
    [NotifyPropertyChangedFor(nameof(MaterialKindName))]
    [NotifyPropertyChangedFor(nameof(MaterialDescription))]
    [NotifyPropertyChangedFor(nameof(HasMaterialDescription))]
    [NotifyPropertyChangedFor(nameof(MaterialShortLabel))]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    private CraftingMaterial? _material;

    public bool IsMaterial => Material is not null;

    /// <summary>Neither equipment nor a known crafting material: a tile that only keeps its cells taken.</summary>
    public bool IsPlaceholder => !IsEquipment && !IsMaterial;

    public ImageSource? MaterialIcon => _iconProvider?.Get(Material?.IconPath);

    public bool HasMaterialIcon => MaterialIcon is not null;

    public string? MaterialName => Material?.Name;

    /// <summary>Kind line of the material tooltip, e.g. "Rune", "Prefix shard · Melee".</summary>
    public string? MaterialKindName => Material switch
    {
        null => null,
        { Kind: CraftingMaterialKind.AffixShard, AffixType: { } type } m =>
            m.GroupName is { Length: > 0 } group ? $"{type} shard · {group}" : $"{type} shard",
        var m => m.Kind.DisplayName
    };

    public string? MaterialDescription => Material?.Description;

    public bool HasMaterialDescription => !string.IsNullOrWhiteSpace(Material?.Description);

    /// <summary>Text on a material tile without an icon: the first letters of its name ("RoR" for Rune of Removal).</summary>
    public string MaterialShortLabel => Material is { } m
        ? new string(m.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(w => w[0]).Take(3).ToArray())
        : string.Empty;

    /// <summary>Name for messages: the equipment or material name, else "this idol" / "this item".</summary>
    public string DisplayName => Equipment?.Item?.Base.Name ?? Material?.FullName ?? (IsIdol ? "this idol" : "this item");

    /// <summary>Crafting material shown when the save was read; a different <see cref="Material"/> is written back.</summary>
    private CraftingMaterial? _loadedMaterial;

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
        _loadedMaterial = Material;
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
        else if (Material is { } material && material != _loadedMaterial)
            data = CraftingMaterialData.Write(material, Source.Data);

        var (x, y) = (Column, Row, Footprint) == _loadedPlacement
            ? (Source.X, Source.Y)
            : (Column, BackpackViewModel.SavedYFromRow(Row, Footprint, _gridRows));

        return data.SequenceEqual(Source.Data) && x == Source.X && y == Source.Y
            ? null
            : Source with { X = x, Y = y, Data = data };
    }

    /// <summary>
    /// The entry to add for a <see cref="IsNew">new</see> item: the current item at its current cell.
    /// </summary>
    /// <exception cref="ArgumentException">The item cannot be stored.</exception>
    internal SavedItem ToNewSavedItem()
    {
        var data = Equipment?.Item is { } current
            ? ItemDataWriter.Write(SavedItemConverter.ToItemData(current, null))
            : Material is { } material
                ? CraftingMaterialData.Write(material)
                : Source.Data;

        return Source with { X = Column, Y = BackpackViewModel.SavedYFromRow(Row, Footprint, _gridRows), Data = data };
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

    public bool IsIdol => EquipmentSlot.Idol.AcceptsBaseType(BaseTypeId);

    /// <summary>Kind of crafting material stored in the item, also when <c>base.db</c> does not know the material.</summary>
    private CraftingMaterialKind? StoredMaterialKind => CraftingMaterialKindExtensions.FromBaseTypeId(BaseTypeId);

    /// <summary>Short text on a placeholder tile.</summary>
    public string PlaceholderLabel => IsIdol ? "Idol" : StoredMaterialKind switch
    {
        CraftingMaterialKind.AffixShard => "Shard",
        { } kind => kind.DisplayName,
        null => "?"
    };

    /// <summary>Tooltip of a placeholder tile.</summary>
    public string PlaceholderToolTip
    {
        get
        {
            if (IsIdol)
                return "Idol — its base is not in the game database (generate or update it with the database button in the title bar)";

            if (StoredMaterialKind is { } kind)
                return $"{kind.DisplayName} — not in the game database (generate or update it with the database button in the title bar)";

            return EquipmentSlotExtensions.FromBaseTypeId(BaseTypeId) is { } slot
                ? $"{slot.DisplayName} — its base is not in the game database (generate or update it in Settings)"
                : $"Unknown item (type {BaseTypeId})";
        }
    }

    /// <summary>Whether the item covers the cell in <paramref name="column"/>, <paramref name="row"/>.</summary>
    public bool Covers(int column, int row) =>
        column >= Column && column < Column + Width && row >= Row && row < Row + Height;
}

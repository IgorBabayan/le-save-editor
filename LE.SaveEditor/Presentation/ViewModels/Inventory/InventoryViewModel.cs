using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Presentation.Services.Toasts;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// "Inventory" tab of one character: equipped items, the idol grid and crafting materials on top,
/// followed by the "Items", "Resources" and "Woven echoes" sections.
/// Equipment slots show the items the character wears and the "Items" section the backpack (both read from
/// <c>savedItems</c>); the other sections start empty. Clicking an equipment slot opens the item inspector (a compact item builder) next to it.
/// </summary>
public sealed partial class InventoryViewModel : ViewModelBase
{
    /// <summary>Side length of the idol grid.</summary>
    public const int IdolGridSize = 5;

    private readonly IDialogService _dialogService;

    public InventoryViewModel(
        CharacterSave character,
        IDialogService dialogService,
        IIconProvider iconProvider,
        IItemBaseCatalog itemBaseCatalog,
        IToastService toastService)
    {
        Character = character;
        _dialogService = dialogService;

        EquipmentSlotViewModel Create(EquipmentSlot slot) => new(slot, iconProvider);
        Helmet = Create(EquipmentSlot.Helmet);
        Amulet = Create(EquipmentSlot.Amulet);
        Weapon = Create(EquipmentSlot.Weapon);
        BodyArmour = Create(EquipmentSlot.BodyArmour);
        OffHand = Create(EquipmentSlot.OffHand);
        LeftRing = Create(EquipmentSlot.LeftRing);
        Belt = Create(EquipmentSlot.Belt);
        RightRing = Create(EquipmentSlot.RightRing);
        Gloves = Create(EquipmentSlot.Gloves);
        Boots = Create(EquipmentSlot.Boots);
        Relic = Create(EquipmentSlot.Relic);

        LoadEquippedItems(itemBaseCatalog);
        Backpack = new BackpackViewModel(character.SavedItems, iconProvider, itemBaseCatalog, toastService);
    }

    public CharacterSave Character { get; }

    // ===== Equipment slots =====
    public EquipmentSlotViewModel Helmet { get; }
    public EquipmentSlotViewModel Amulet { get; }
    public EquipmentSlotViewModel Weapon { get; }
    public EquipmentSlotViewModel BodyArmour { get; }
    public EquipmentSlotViewModel OffHand { get; }
    public EquipmentSlotViewModel LeftRing { get; }
    public EquipmentSlotViewModel Belt { get; }
    public EquipmentSlotViewModel RightRing { get; }
    public EquipmentSlotViewModel Gloves { get; }
    public EquipmentSlotViewModel Boots { get; }
    public EquipmentSlotViewModel Relic { get; }

    /// <summary>The "Items" section: the backpack grid with the items the character carries.</summary>
    public BackpackViewModel Backpack { get; }

    /// <summary>Cells of the idol grid, row by row (<see cref="IdolGridSize"/> × <see cref="IdolGridSize"/>).</summary>
    public IReadOnlyList<IdolCellViewModel> IdolCells { get; } = CreateIdolCells();

    /// <summary>Totals shown in the "Crafting materials" box.</summary>
    public IReadOnlyList<CraftingMaterialViewModel> CraftingMaterials { get; } =
    [
        new("Shards"),
        new("Glyphs"),
        new("Runes")
    ];

    /// <summary>
    /// Opens the item inspector next to <paramref name="slot"/>; an applied item replaces the one in the slot.
    /// </summary>
    [RelayCommand]
    private void OpenItemBuilder(EquipmentSlotViewModel? slot)
    {
        if (slot is null)
            return;

        _dialogService.ShowItemInspector(slot, Character.CharacterClass);
    }

    /// <summary>
    /// Puts the items in the equipment containers of the save into their slots. Items that cannot be decoded or
    /// whose base is not in <c>base.db</c> (database missing or outdated) leave their slot empty.
    /// </summary>
    private void LoadEquippedItems(IItemBaseCatalog catalog)
    {
        foreach (var saved in Character.SavedItems)
        {
            if (EquipmentSlotExtensions.FromContainerId(saved.ContainerId) is not { } slot
                || GetSlot(slot) is not { Item: null } target
                || ItemDataReader.Read(saved.Data) is not { } data)
                continue;

            target.Item = SavedItemConverter.ToBuiltItem(data, slot, catalog);
        }
    }

    private EquipmentSlotViewModel GetSlot(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Helmet => Helmet,
        EquipmentSlot.Amulet => Amulet,
        EquipmentSlot.Weapon => Weapon,
        EquipmentSlot.BodyArmour => BodyArmour,
        EquipmentSlot.OffHand => OffHand,
        EquipmentSlot.LeftRing => LeftRing,
        EquipmentSlot.RightRing => RightRing,
        EquipmentSlot.Belt => Belt,
        EquipmentSlot.Gloves => Gloves,
        EquipmentSlot.Boots => Boots,
        EquipmentSlot.Relic => Relic,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
    };

    /// <summary>The idol grid has a blocked plus shape in its middle, as in the game.</summary>
    private static IReadOnlyList<IdolCellViewModel> CreateIdolCells()
    {
        const int center = IdolGridSize / 2;
        var cells = new List<IdolCellViewModel>(IdolGridSize * IdolGridSize);

        for (var row = 0; row < IdolGridSize; row++)
        for (var column = 0; column < IdolGridSize; column++)
        {
            var isBlocked = (row == center && Math.Abs(column - center) <= 1)
                            || (column == center && Math.Abs(row - center) <= 1);
            cells.Add(new IdolCellViewModel(row, column, isBlocked));
        }

        return cells;
    }
}

/// <summary>One cell of the idol grid; <see cref="IsBlocked"/> cells can never hold an idol.</summary>
public sealed record IdolCellViewModel(int Row, int Column, bool IsBlocked);

/// <summary>A crafting material total; <see cref="Amount"/> is <c>null</c> until it is read from the save.</summary>
public sealed record CraftingMaterialViewModel(string Name, int? Amount = null);

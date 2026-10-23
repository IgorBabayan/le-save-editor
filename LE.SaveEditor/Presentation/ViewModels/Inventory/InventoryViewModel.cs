using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// "Inventory" tab of one character: equipped items, the idol grid and crafting materials on top,
/// followed by the "Items", "Resources" and "Woven echoes" sections.
/// Nothing is parsed from the save yet, so every slot and section starts empty; clicking an equipment slot
/// opens the item builder for it.
/// </summary>
public sealed partial class InventoryViewModel : ViewModelBase
{
    /// <summary>Side length of the idol grid.</summary>
    public const int IdolGridSize = 5;

    private readonly IDialogService _dialogService;

    public InventoryViewModel(CharacterSave character, IDialogService dialogService, IIconProvider iconProvider)
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

    /// <summary>Cells of the idol grid, row by row (<see cref="IdolGridSize"/> × <see cref="IdolGridSize"/>).</summary>
    public IReadOnlyList<IdolCellViewModel> IdolCells { get; } = CreateIdolCells();

    /// <summary>Totals shown in the "Crafting materials" box.</summary>
    public IReadOnlyList<CraftingMaterialViewModel> CraftingMaterials { get; } =
    [
        new("Shards"),
        new("Glyphs"),
        new("Runes")
    ];

    /// <summary>Opens the item builder for <paramref name="slot"/>; the built item replaces the one in the slot.</summary>
    [RelayCommand]
    private void OpenItemBuilder(EquipmentSlotViewModel? slot)
    {
        if (slot is null)
            return;

        var item = _dialogService.ShowItemBuilder(slot.Slot, slot.Item);
        if (item is not null)
            slot.Item = item;
    }

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

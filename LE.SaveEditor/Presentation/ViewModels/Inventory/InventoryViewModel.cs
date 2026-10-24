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
/// Equipment slots show the items the character wears, the idol grid its idols and the "Items" section the backpack
/// (all read from <c>savedItems</c>); the other sections start empty. Clicking an equipment slot, an idol or a backpack
/// item opens the item inspector (a compact item builder) next to it; right-clicking an empty backpack or idol cell
/// builds a new item there.
/// <see cref="GetUpdates"/> collects what changed since the save was read, for saving.
/// </summary>
public sealed partial class InventoryViewModel : ViewModelBase
{
    /// <summary>Side length of the idol grid.</summary>
    public const int IdolGridSize = ItemGridLayout.IdolGridSize;

    private readonly IDialogService _dialogService;

    /// <summary>The <c>savedItems</c> entry each equipment slot was read from, and the item it showed then.</summary>
    private readonly Dictionary<EquipmentSlot, (SavedItem Source, BuiltItem? Loaded)> _equipped = [];

    /// <summary>An equipped item, an idol or an item in the backpack was changed, moved or removed.</summary>
    public event EventHandler? Changed;

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
        Idols = new BackpackViewModel(character.SavedItems, iconProvider, itemBaseCatalog, toastService, ItemGridLayout.Idols);

        foreach (var slot in EquipmentSlots)
            slot.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(EquipmentSlotViewModel.Item))
                    Changed?.Invoke(this, EventArgs.Empty);
            };
        Backpack.Changed += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
        Idols.Changed += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>All equipment slots.</summary>
    public IEnumerable<EquipmentSlotViewModel> EquipmentSlots =>
        [Helmet, Amulet, Weapon, BodyArmour, OffHand, LeftRing, Belt, RightRing, Gloves, Boots, Relic];

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

    /// <summary>The idol grid (<see cref="IdolGridSize"/> × <see cref="IdolGridSize"/> without its corners) with the character's idols.</summary>
    public BackpackViewModel Idols { get; }

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

    /// <summary>Removes the item worn in <paramref name="slot"/> after asking; it is deleted from the save on the next save.</summary>
    [RelayCommand]
    private void RemoveEquippedItem(EquipmentSlotViewModel? slot)
    {
        if (slot?.Item is not { } item
            || !_dialogService.Confirm("Remove item", $"Remove {item.Base.Name} from the {slot.Label.ToLowerInvariant()} slot? It is deleted from the save when you save."))
            return;

        slot.Item = null;
    }

    /// <summary>
    /// Removes <paramref name="item"/> from the backpack or the idol grid after asking; an item read from the save is
    /// deleted from it on the next save.
    /// </summary>
    [RelayCommand]
    private void RemoveGridItem(BackpackItemViewModel? item)
    {
        if (item is null)
            return;

        var grid = Idols.Items.Contains(item) ? Idols : Backpack;
        if (!grid.Items.Contains(item))
            return;

        var name = item.Equipment?.Item?.Base.Name ?? (item.IsIdol ? "this idol" : "this item");
        if (!_dialogService.Confirm("Remove item", $"Remove {name} from the {grid.Layout.Name}? It is deleted from the save when you save."))
            return;

        grid.RemoveItem(item);
    }

    /// <summary>
    /// An empty cell of the backpack (or the idol grid) was right-clicked and an item type picked: opens the item
    /// inspector for a new item of that type next to the cell; an applied item is put into that grid at that cell (or
    /// the nearest spot).
    /// </summary>
    [RelayCommand]
    private void BuildBackpackItem(NewBackpackItemRequest? request)
    {
        var grid = request?.Grid ?? Backpack;
        if (request is null || !grid.IsCellFree(request.Column, request.Row))
            return;

        _dialogService.ShowItemInspector(
            request.Slot,
            Character.CharacterClass,
            request.Anchor,
            built => grid.AddItem(built, request.Column, request.Row));
    }

    /// <summary>
    /// Changes to write into <c>savedItems</c>: equipped items that were replaced or built in an empty slot, followed
    /// by the changes of the idol grid and the backpack. An item that could not be shown (base not in <c>base.db</c>) stays in the save
    /// unless another item was built in its slot.
    /// </summary>
    /// <exception cref="ArgumentException">An edited item cannot be stored.</exception>
    public IReadOnlyList<SavedItemUpdate> GetUpdates()
    {
        var updates = new List<SavedItemUpdate>();

        foreach (var slot in EquipmentSlots)
        {
            var current = slot.Item;
            if (_equipped.TryGetValue(slot.Slot, out var equipped))
            {
                if (ReferenceEquals(current, equipped.Loaded))
                    continue;

                if (current is null)
                {
                    updates.Add(SavedItemUpdate.Remove(equipped.Source));
                    continue;
                }

                var data = ItemDataWriter.Write(SavedItemConverter.ToItemData(current, ItemDataReader.Read(equipped.Source.Data)));
                if (!data.SequenceEqual(equipped.Source.Data))
                    updates.Add(SavedItemUpdate.Replace(equipped.Source, equipped.Source with { Data = data }));
            }
            else if (current is not null)
            {
                var data = ItemDataWriter.Write(SavedItemConverter.ToItemData(current, null));
                updates.Add(SavedItemUpdate.Add(new SavedItem(
                    EquipmentSlotExtensions.ToContainerId(slot.Slot), 0, 0, 1, ItemDataReader.Version2, data)));
            }
        }

        updates.AddRange(Idols.GetUpdates());
        updates.AddRange(Backpack.GetUpdates());
        return updates;
    }

    /// <summary>
    /// Puts the items in the equipment containers of the save into their slots. Items that cannot be decoded or
    /// whose base is not in <c>base.db</c> (database missing or outdated) leave their slot empty.
    /// </summary>
    private void LoadEquippedItems(IItemBaseCatalog catalog)
    {
        foreach (var saved in Character.SavedItems)
        {
            // The first entry of a container is the one shown and written back; anything else is left as it is.
            if (EquipmentSlotExtensions.FromContainerId(saved.ContainerId) is not { } slot
                || _equipped.ContainsKey(slot))
                continue;

            var built = ItemDataReader.Read(saved.Data) is { } data
                ? SavedItemConverter.ToBuiltItem(data, slot, catalog)
                : null;

            _equipped[slot] = (saved, built);
            GetSlot(slot).Item = built;
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
}

/// <summary>A new item of type <paramref name="Slot"/> to build in the empty grid cell <paramref name="Column"/>, <paramref name="Row"/>.</summary>
/// <param name="Anchor">The view element of the cell; the item inspector opens next to it.</param>
/// <param name="Grid">The grid to build in (backpack or idol grid); <c>null</c> for the backpack.</param>
public sealed record NewBackpackItemRequest(EquipmentSlot Slot, int Column, int Row, object? Anchor, BackpackViewModel? Grid = null);

/// <summary>A crafting material total; <see cref="Amount"/> is <c>null</c> until it is read from the save.</summary>
public sealed record CraftingMaterialViewModel(string Name, int? Amount = null);

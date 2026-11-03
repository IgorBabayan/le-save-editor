using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Presentation.Services.Toasts;
using LE.SaveEditor.Presentation.ViewModels.Blessings;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Notifications;
using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// "Inventory" tab of one character: equipped items, the idol grid and crafting materials on top,
/// followed by the "Items", "Resources", "Woven echoes" and "Blessings" sections.
/// Equipment slots show the items the character wears, the idol grid its idols and the "Items" section the backpack
/// (all read from <c>savedItems</c>), the "Resources" section the keys and consumables of the character's stash
/// (<see cref="ResourcesViewModel"/>); "Woven echoes" starts empty; "Blessings" holds the blessing picked for each
/// timeline echo (<see cref="BlessingsViewModel"/>). Clicking an equipment slot, an idol or a backpack
/// item opens the item inspector (a compact item builder) next to it; right-clicking an empty backpack or idol cell
/// builds a new item there. The idol altar slot sits by the idol grid: the altar in it gives the grid its shape and
/// refracted slots (<see cref="IdolAltarLayouts"/>), and changing or removing it reshapes the grid at once.
/// The "Crafting materials" box shows the material storage of the character's stash (affix shards, runes, glyphs) with
/// an edit action per kind; runes and glyphs lying in the backpack are changed with the crafting material picker of
/// their kind and removed like other items.
/// <see cref="GetUpdates"/> collects what changed since the save was read, for saving.
/// </summary>
public sealed partial class InventoryViewModel : ViewModelBase
{
    /// <summary>Side length of the idol grid.</summary>
    public const int IdolGridSize = ItemGridLayout.IdolGridSize;

    private readonly IDialogService _dialogService;
    private readonly IToastService _toastService;

    /// <summary>
    /// <c>subTypeID</c> of the altar the save holds in the altar slot when the editor cannot show it (its base is not in
    /// <c>base.db</c>): the idol grid still takes its shape until another altar is built in the slot.
    /// </summary>
    private readonly int? _hiddenAltarSubTypeId;

    /// <summary>The <c>savedItems</c> entry each equipment slot was read from, and the item it showed then.</summary>
    private readonly Dictionary<EquipmentSlot, (SavedItem Source, BuiltItem? Loaded)> _equipped = [];

    /// <summary>An equipped item, an idol, an item in the backpack or a blessing was changed, moved or removed.</summary>
    public event EventHandler? Changed;

    public InventoryViewModel(
        CharacterSave character,
        IDialogService dialogService,
        IIconProvider iconProvider,
        IItemBaseCatalog itemBaseCatalog,
        IToastService toastService,
        StashMaterialsViewModel? stash = null)
    {
        Character = character;
        _dialogService = dialogService;
        _toastService = toastService;
        Stash = stash;

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
        IdolAltar = Create(EquipmentSlot.IdolAltar);

        LoadEquippedItems(itemBaseCatalog);
        if (IdolAltar.Item is null && _equipped.TryGetValue(EquipmentSlot.IdolAltar, out var altar)
            && ItemDataReader.Read(altar.Source.Data) is { BaseTypeId: EquipmentSlotExtensions.IdolAltarBaseTypeId } altarData)
            _hiddenAltarSubTypeId = altarData.SubTypeId;

        Backpack = new BackpackViewModel(character.SavedItems, iconProvider, itemBaseCatalog, toastService);
        IdolUnlocks = IdolSlotUnlocks.CountUnlocks(character.SavedQuests);
        Idols = new BackpackViewModel(character.SavedItems, iconProvider, itemBaseCatalog, toastService,
            ItemGridLayout.ForIdols(IdolUnlocks, AltarSubTypeId));
        IdolAltar.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(EquipmentSlotViewModel.Item))
                OnIdolAltarChanged();
        };

        foreach (var slot in EquipmentSlots)
            slot.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(EquipmentSlotViewModel.Item))
                    Changed?.Invoke(this, EventArgs.Empty);
            };
        Backpack.Changed += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
        if (stash is not null)
            stash.Changed += (_, _) =>
            {
                UpdateCraftingMaterials();
                UpdateGold();
            };
        UpdateCraftingMaterials();
        UpdateGold();
        Idols.Changed += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
        Resources = new ResourcesViewModel(stash, itemBaseCatalog, iconProvider, dialogService);
        Blessings = new BlessingsViewModel(character, itemBaseCatalog, iconProvider);
        Blessings.Changed += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>All equipment slots, the idol altar slot included.</summary>
    public IEnumerable<EquipmentSlotViewModel> EquipmentSlots =>
        [Helmet, Amulet, Weapon, BodyArmour, OffHand, LeftRing, Belt, RightRing, Gloves, Boots, Relic, IdolAltar];

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

    /// <summary>The altar slot of the idol inventory; its altar shapes the idol grid.</summary>
    public EquipmentSlotViewModel IdolAltar { get; }

    /// <summary>The "Blessings" section: the blessing of every timeline echo.</summary>
    public BlessingsViewModel Blessings { get; }

    /// <summary>The "Items" section: the backpack grid with the items the character carries.</summary>
    public BackpackViewModel Backpack { get; }

    /// <summary>
    /// The idol grid (<see cref="IdolGridSize"/> × <see cref="IdolGridSize"/> without its corners and centre, locked slots
    /// as in game) with the character's idols.
    /// </summary>
    public BackpackViewModel Idols { get; }

    /// <summary>Idol unlocks the character has earned: completed quests that reward one.</summary>
    public int IdolUnlocks { get; }

    /// <summary>
    /// <c>subTypeID</c> of the idol altar that shapes the idol grid: the one in the altar slot, or the one the save holds
    /// there when the editor cannot show it; <c>null</c> without an altar.
    /// </summary>
    private int? AltarSubTypeId => IdolAltar.Item?.Base.SubTypeId ?? _hiddenAltarSubTypeId;

    /// <summary>
    /// Line under the idol grid: without an altar e.g. "11 of 20 idol slots unlocked (4 idol quests completed)", with
    /// one e.g. "Twisted Altar: 17 idol slots, 5 refracted".
    /// </summary>
    public string IdolSlotsText
    {
        get
        {
            var layout = Idols.Layout;
            if (layout.AltarSubTypeId is { } altar)
            {
                var name = IdolAltar.Item?.Base.Name ?? "Idol altar";
                var slots = layout.UsableCellCount;
                var text = $"{name}: {slots} idol {(slots == 1 ? "slot" : "slots")}, {layout.Refracted.Count} refracted";
                return IdolAltarLayouts.IsKnown(altar) ? text : $"{text} (layout of this altar not known, standard grid shown)";
            }

            var unlocked = ItemGridLayout.IdolSlotCount - layout.Locked.Count;
            var quests = IdolUnlocks == 1 ? "1 idol quest" : $"{IdolUnlocks} idol quests";
            return $"{unlocked} of {ItemGridLayout.IdolSlotCount} idol slots unlocked ({quests} completed)";
        }
    }

    /// <summary>
    /// The idol altar was built, changed or removed: the idol grid takes the shape of the new altar (or the standard
    /// shape with the quest locks). Idols that keep fitting stay; the others move to a free slot, then to the backpack;
    /// an idol that fits nowhere stays in the save where it was (not shown until the save is reloaded).
    /// </summary>
    private void OnIdolAltarChanged()
    {
        var layout = ItemGridLayout.ForIdols(IdolUnlocks, AltarSubTypeId);
        var before = Idols.Items.ToDictionary(i => i, i => (i.Column, i.Row));
        var displaced = Idols.ChangeLayout(layout);
        OnPropertyChanged(nameof(IdolSlotsText));

        var movedInGrid = Idols.Items.Count(i => before.TryGetValue(i, out var cell) && cell != (i.Column, i.Row));
        var toBackpack = new List<string>();
        var keptInSave = new List<string>();
        var lost = new List<string>();
        foreach (var item in displaced)
        {
            if (Backpack.AddFromGrid(item))
                toBackpack.Add(item.DisplayName);
            else if (Idols.KeepInSave(item))
                keptInSave.Add(item.DisplayName);
            else
                lost.Add(item.DisplayName);
        }

        if (movedInGrid > 0)
            _toastService.Show(movedInGrid == 1
                ? "1 idol was moved to another slot to fit the new idol grid."
                : $"{movedInGrid} idols were moved to other slots to fit the new idol grid.");
        if (toBackpack.Count > 0)
            _toastService.Show($"No room left in the idol grid, moved to the backpack: {string.Join(", ", toBackpack)}.");
        if (keptInSave.Count > 0)
            _toastService.Show(
                $"No room in the idol grid or the backpack for {string.Join(", ", keptInSave)}; left in the save where it was.",
                NotificationSeverity.Warning);
        if (lost.Count > 0)
            _toastService.Show(
                $"No room in the idol grid or the backpack for {string.Join(", ", lost)}; the new idol was dropped.",
                NotificationSeverity.Warning);
    }

    /// <summary>The "Resources" section: keys and consumables the character's stash keeps (<c>keysList</c>).</summary>
    public ResourcesViewModel Resources { get; }

    /// <summary>
    /// The material storage of the stash the character uses (the game's "Crafting Materials" panel); <c>null</c> when
    /// the save folder has no stash file for it.
    /// </summary>
    public StashMaterialsViewModel? Stash { get; }

    /// <summary>The character has a stash file, so its gold can be changed.</summary>
    public bool HasStash => Stash is not null;

    /// <summary>
    /// Gold of the character's stash (shared by every character using it); written to the stash file when the character
    /// is saved.
    /// </summary>
    [ObservableProperty]
    private int _gold;

    /// <summary>Set while <see cref="Gold"/> is filled from the stash, so it is not written back.</summary>
    private bool _updatingGold;

    /// <summary>Tooltip of the gold box: where the gold is kept.</summary>
    public string GoldToolTip => Stash is { } stash
        ? $"Gold kept in {stash.Stash.FileName}, shared by every character using that stash. Written when the character is saved."
        : "No stash file was found for this character in the save folder, so its gold cannot be edited.";

    partial void OnGoldChanged(int value)
    {
        var clamped = Math.Clamp(value, 0, StashMaterialsViewModel.MaxGold);
        if (clamped != value)
        {
            Gold = clamped;
            return;
        }

        if (!_updatingGold)
            Stash?.SetGold(value);
    }

    /// <summary>Shows the gold of the stash (0 without one).</summary>
    private void UpdateGold()
    {
        _updatingGold = true;
        try
        {
            Gold = Stash?.Gold ?? 0;
        }
        finally
        {
            _updatingGold = false;
        }
    }

    /// <summary>
    /// The "Crafting materials" box, in the game's order: per kind (shards, runes, glyphs) how many the material storage
    /// holds, and its own action to edit them.
    /// </summary>
    public IReadOnlyList<CraftingMaterialViewModel> CraftingMaterials { get; } =
    [
        new(CraftingMaterialKind.AffixShard, "Shards"),
        new(CraftingMaterialKind.Rune, "Runes"),
        new(CraftingMaterialKind.Glyph, "Glyphs")
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

        var name = item.DisplayName;
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
    /// The edit action of a kind in the "Crafting materials" box: opens the material storage dialog for that kind; the
    /// applied amounts are written to the stash file on the next save.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanEditCraftingMaterials))]
    private void EditCraftingMaterials(CraftingMaterialViewModel? kind)
    {
        if (kind is null || Stash is not { } stash)
            return;

        if (_dialogService.ShowCraftingMaterialStorage(kind.Kind, stash.GetAmounts(kind.Kind), stash.Stash.FileName) is { } amounts)
            stash.SetAmounts(kind.Kind, amounts);
    }

    private bool CanEditCraftingMaterials(CraftingMaterialViewModel? kind) => kind is not null && Stash is not null;

    /// <summary>Shows the totals of the material storage in the "Crafting materials" box.</summary>
    private void UpdateCraftingMaterials()
    {
        foreach (var material in CraftingMaterials)
        {
            material.Amount = Stash?.Total(material.Kind);
            material.StashName = Stash?.Stash.FileName;
        }
    }

    /// <summary>
    /// A crafting material in the backpack was clicked (or "Edit…" chosen): opens the crafting material picker of its
    /// kind on it; the picked material replaces it in its cell.
    /// </summary>
    [RelayCommand]
    private void EditCraftingMaterial(BackpackItemViewModel? item)
    {
        if (item?.Material is not { } current || !Backpack.Items.Contains(item))
            return;

        if (_dialogService.ShowCraftingMaterialPicker(current.Kind, current) is { } material)
            Backpack.ChangeMaterial(item, material);
    }

    /// <summary>
    /// Changes to write into <c>savedItems</c>: equipped items that were replaced or built in an empty slot, followed
    /// by the changes of the idol grid, the backpack and the blessings. An item that could not be shown (base not in <c>base.db</c>) stays in the save
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
        updates.AddRange(Blessings.GetUpdates());
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
        EquipmentSlot.IdolAltar => IdolAltar,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
    };
}

/// <summary>A new item of type <paramref name="Slot"/> to build in the empty grid cell <paramref name="Column"/>, <paramref name="Row"/>.</summary>
/// <param name="Anchor">The view element of the cell; the item inspector opens next to it.</param>
/// <param name="Grid">The grid to build in (backpack or idol grid); <c>null</c> for the backpack.</param>
public sealed record NewBackpackItemRequest(EquipmentSlot Slot, int Column, int Row, object? Anchor, BackpackViewModel? Grid = null);

/// <summary>
/// One kind in the "Crafting materials" box: its name, how many materials of the kind the material storage holds and,
/// in the view, the action that edits them.
/// </summary>
public sealed partial class CraftingMaterialViewModel(CraftingMaterialKind kind, string name) : ObservableObject
{
    public CraftingMaterialKind Kind { get; } = kind;

    /// <summary>Label of the box column, e.g. "Shards".</summary>
    public string Name { get; } = name;

    /// <summary>Materials of the kind in the material storage; <c>null</c> when the character has no stash file.</summary>
    [ObservableProperty]
    private int? _amount;

    /// <summary>File name of the stash the amounts come from; <c>null</c> without one.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EditToolTip))]
    private string? _stashName;

    /// <summary>Tooltip of the edit action.</summary>
    public string EditToolTip => StashName is { } stash
        ? $"Edit the {Kind.PluralName.ToLowerInvariant()} in the material storage ({stash})"
        : "No stash file was found for this character in the save folder";
}

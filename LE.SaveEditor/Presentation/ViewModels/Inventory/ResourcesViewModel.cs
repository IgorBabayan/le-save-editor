using System.Collections.ObjectModel;
using System.Windows.Media;
using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// The "Resources" section of the inventory, laid out like the game's Resources tab: the keys (Harbinger Eye, the
/// dungeon keys, the arena keys) and the consumables (charms, primordial materials, Temporal Keystones and Timeglass
/// Fragments) the stash keeps an amount of, each with an editable amount. Resources the game does not show in that tab
/// (lizard tails, tokens, ...) are listed under "Other" while the stash holds some, and the "Edit…" action edits every
/// resource in the material storage dialog. The amounts live in the <c>keysList</c> of the character's stash file and
/// are written with the other stash changes when the character is saved.
/// </summary>
public sealed partial class ResourcesViewModel : ViewModelBase
{
    private const CraftingMaterialKind Kind = CraftingMaterialKind.KeyItem;

    // Sub types of the "Key Items" item type (baseTypeID 104) in the places the game's Resources tab shows them.
    private const int ArenaKey = 0;
    private const int ArenaKeyOfMemory = 3;
    private const int TemporalSanctumKey = 4;
    private const int LightlessArborKey = 5;
    private const int SoulfireBastionKey = 6;
    private const int HarbingerEye = 7;
    private const int PortalCharm = 13;
    private const int ScaleCharm = 14;
    private const int PrimordialFeather = 15;
    private const int PrimordialFang = 16;
    private const int PrimordialPetal = 17;
    private const int PrimordialHorn = 18;
    private const int CrystallizedHeart = 19;
    private const int TemporalKeystone = 22;
    private const int TimeglassFragment = 23;

    private readonly IItemBaseCatalog _catalog;
    private readonly IIconProvider _iconProvider;
    private readonly IDialogService _dialogService;
    private readonly Dictionary<int, ResourceSlotViewModel> _fixed = [];

    /// <summary>Set while the slots are filled from the stash, so they do not write the amounts back.</summary>
    private bool _refreshing;

    public ResourcesViewModel(
        StashMaterialsViewModel? stash,
        IItemBaseCatalog catalog,
        IIconProvider iconProvider,
        IDialogService dialogService)
    {
        Stash = stash;
        _catalog = catalog;
        _iconProvider = iconProvider;
        _dialogService = dialogService;

        HarbingerKeys = Create(HarbingerEye);
        DungeonKeys = Create(TemporalSanctumKey, LightlessArborKey, SoulfireBastionKey);
        ArenaKeys = Create(ArenaKeyOfMemory, ArenaKey);
        Charms = Create(PortalCharm, ScaleCharm);
        PrimordialMaterials = Create(CrystallizedHeart, PrimordialHorn, PrimordialFang, PrimordialFeather, PrimordialPetal);
        TimelineMaterials = Create(TemporalKeystone, TimeglassFragment);

        if (stash is not null)
            stash.Changed += (_, _) => Refresh();
        Refresh();
    }

    /// <summary>The material storage the amounts come from; <c>null</c> when the save folder has no stash file for the character.</summary>
    public StashMaterialsViewModel? Stash { get; }

    public bool HasStash => Stash is not null;

    /// <summary>File name of the stash the resources are kept in.</summary>
    public string? StashName => Stash?.Stash.FileName;

    /// <summary>Line under the section, e.g. "Kept in STASH_CYCLE_8_0 · 2,345 in total".</summary>
    public string Summary => Stash is { } stash
        ? $"Kept in {stash.Stash.FileName} · {stash.Total(Kind):N0} in total. Changes are written to the stash file when the character is saved."
        : "No stash file was found for this character in the save folder, so its resources cannot be edited.";

    /// <summary>The base.db has no resources (missing or generated before they were read): names and icons are missing.</summary>
    public bool IsDatabaseMissing => _catalog.GetCraftingMaterials(Kind).Count == 0;

    // ===== Keys =====
    public IReadOnlyList<ResourceSlotViewModel> HarbingerKeys { get; }
    public IReadOnlyList<ResourceSlotViewModel> DungeonKeys { get; }
    public IReadOnlyList<ResourceSlotViewModel> ArenaKeys { get; }

    // ===== Consumables =====
    public IReadOnlyList<ResourceSlotViewModel> Charms { get; }
    public IReadOnlyList<ResourceSlotViewModel> PrimordialMaterials { get; }
    public IReadOnlyList<ResourceSlotViewModel> TimelineMaterials { get; }

    /// <summary>Resources the stash holds that the game's Resources tab does not show (lizard tails, tokens, ...).</summary>
    public ObservableCollection<ResourceSlotViewModel> Others { get; } = [];

    public bool HasOthers => Others.Count > 0;

    /// <summary>
    /// Opens the material storage dialog with every resource of the stash: amounts can be changed there too, resources
    /// removed and any resource of the game database added.
    /// </summary>
    [RelayCommand(CanExecute = nameof(HasStash))]
    private void EditAll()
    {
        if (Stash is not { } stash)
            return;

        if (_dialogService.ShowCraftingMaterialStorage(Kind, stash.GetAmounts(Kind), stash.Stash.FileName) is { } amounts)
            stash.SetAmounts(Kind, amounts);
    }

    private IReadOnlyList<ResourceSlotViewModel> Create(params int[] subTypeIds) =>
        subTypeIds.Select(id => _fixed[id] = CreateSlot(id)).ToList();

    private ResourceSlotViewModel CreateSlot(int subTypeId)
    {
        var slot = new ResourceSlotViewModel(subTypeId, _catalog.FindCraftingMaterial(Kind, subTypeId), _iconProvider)
        {
            IsEditable = Stash is not null
        };
        slot.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ResourceSlotViewModel.Amount) && !_refreshing)
                Stash?.SetAmount(new CraftingMaterialKey(Kind, slot.SubTypeId), slot.Amount);
        };
        return slot;
    }

    /// <summary>Shows the amounts of the material storage; resources outside the layout get a slot under "Other".</summary>
    private void Refresh()
    {
        _refreshing = true;
        try
        {
            var amounts = Stash?.GetAmounts(Kind) ?? new Dictionary<int, int>();
            foreach (var (subTypeId, slot) in _fixed)
                slot.Amount = amounts.GetValueOrDefault(subTypeId);

            // An "Other" slot set to 0 stays until the amounts change elsewhere, so it can be typed into again.
            foreach (var other in Others.ToList())
            {
                if (!amounts.ContainsKey(other.SubTypeId) && other.Amount > 0)
                    Others.Remove(other);
                else
                    other.Amount = amounts.GetValueOrDefault(other.SubTypeId);
            }

            var known = Others.Select(o => o.SubTypeId).ToHashSet();
            foreach (var (subTypeId, quantity) in amounts.OrderBy(a => a.Key))
            {
                if (_fixed.ContainsKey(subTypeId) || known.Contains(subTypeId))
                    continue;

                var slot = CreateSlot(subTypeId);
                slot.Amount = quantity;
                Others.Add(slot);
            }
        }
        finally
        {
            _refreshing = false;
        }

        OnPropertyChanged(nameof(HasOthers));
        OnPropertyChanged(nameof(Summary));
    }
}

/// <summary>One resource of the Resources section: its icon, name, description and the amount the stash holds.</summary>
public sealed partial class ResourceSlotViewModel(int subTypeId, CraftingMaterial? material, IIconProvider iconProvider) : ObservableObject
{
    public int SubTypeId { get; } = subTypeId;

    /// <summary>The resource in <c>base.db</c>; <c>null</c> when the database does not know it.</summary>
    public CraftingMaterial? Material { get; } = material;

    public string Name => Material?.Name ?? $"Resource #{SubTypeId}";

    public ImageSource? Icon => iconProvider.Get(Material?.IconPath);

    public bool HasIcon => Icon is not null;

    /// <summary>Text on a slot without an icon: the first letters of its name ("TSK" for Temporal Sanctum Key).</summary>
    public string Initials
    {
        get
        {
            var initials = string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(3).Select(w => char.ToUpperInvariant(w[0])));
            return initials.Length > 0 ? initials : "?";
        }
    }

    /// <summary>Tooltip: the name and what the resource does.</summary>
    public string ToolTip => Material is null
        ? $"{Name} — not in the game database (generate or update it with the database button in the title bar)"
        : string.IsNullOrWhiteSpace(Material.Description) ? Name : $"{Name}\n\n{Material.Description}";

    /// <summary>The amount can be changed (the character has a stash file).</summary>
    public bool IsEditable { get; init; }

    /// <summary>Amount the stash holds; 0 removes the resource on the next save.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private int _amount;

    /// <summary>The stash holds none: the slot is dimmed, like in the game.</summary>
    public bool IsEmpty => Amount == 0;

    partial void OnAmountChanged(int value)
    {
        var clamped = Math.Clamp(value, 0, CraftingMaterialStorageViewModel.MaxQuantity);
        if (clamped != value)
            Amount = clamped;
    }
}

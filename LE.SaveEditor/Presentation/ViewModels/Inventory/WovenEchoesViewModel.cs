using System.Collections.ObjectModel;
using System.Windows.Media;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// The "Woven echoes" section of the inventory, laid out like the game's "Woven Echoes" stash tab: the Encounters,
/// Bosses, Crafting and Auras panels with their echoes in the game's rows, each with the amount the character's stash
/// holds (dimmed at 0). Type an amount into an echo to add it (or 0 to remove it); echoes the tab has no slot for
/// (placeholders like "Fading Echo", which the game shows as "ERROR", echoes the game does not show yet such as Pinnacle
/// of Chaos, or echoes unknown to the database) are listed under "Other" while the stash holds some. The echoes live in
/// the <c>wovenEchoesList</c> of the stash file and are written with the other stash changes when the character is saved.
/// </summary>
public sealed partial class WovenEchoesViewModel : ViewModelBase
{
    private readonly IItemBaseCatalog _catalog;
    private readonly IIconProvider _iconProvider;
    private readonly Dictionary<int, WovenEchoSlotViewModel> _fixed = [];

    // Echoes (subTypeID of item type 107) of each panel of the game's tab, row by row, left to right.
    private static readonly int[][] EncounterRows =
    [
        [2, 11, 19, 36, 34, 38, 37, 35],  // False Temple, Soul Foundry, The Forgotten Keep, Besieged Gardens, Bountiful Wilds, Cave of Shadows, Dead Forest, Frostfire Caverns
        [4, 9, 16, 20, 3, 8, 39, 32]      // Conquered Tower, Pillar of Skulls, Overgrown Graveyard, The Grand Brewery, Unclaimed Trove, Champions' Colosseum, Bloodline Glade, Vault of the Barnacle King
    ];

    private static readonly int[][] BossRows =
    [
        [12, 10, 33, 13, 42],             // Antipode Rift, The Seeker's Summit, Defiled Fortress, Heart of the Hive, Omen's Veil
        [17, 5, 41, 43],                  // Confluence of Oblivion, Tomb of Vessels, Rampant Coast, Fractured Prison
        [29, 30, 47, 18]                  // Collapsing Horizon, The Fading Brink, Demigod's Ascendance, Threshold of Eternity
    ];

    private static readonly int[][] CraftingRows =
    [
        [26, 15],                         // Cemetery of Reweaving, The Great Bridge
        [21, 24],                         // Farsight Turtle, Rune Prison Complex
        [7, 25]                           // Gauntlet of Strife, Oerden's Watch
    ];

    private static readonly int[][] AuraRows =
    [
        [14, 31],                         // Lostwood Folly, Woven Beacon
        [40, 27, 22],                     // Nesting Grove, Sepulchre of the Lost, Woven Vessel of Chaos
        [28, 23]                          // Cenotaph of Worlds, Woven Vessel of Memory
    ];

    /// <summary>Set while the slots are filled from the stash, so they do not write the amounts back.</summary>
    private bool _refreshing;

    public WovenEchoesViewModel(StashMaterialsViewModel? stash, IItemBaseCatalog catalog, IIconProvider iconProvider)
    {
        Stash = stash;
        _catalog = catalog;
        _iconProvider = iconProvider;

        Encounters = CreateRows(EncounterRows);
        Bosses = CreateRows(BossRows);
        Crafting = CreateRows(CraftingRows);
        Auras = CreateRows(AuraRows);

        if (stash is not null)
            stash.Changed += (_, _) => Refresh();
        Refresh();
    }

    /// <summary>The stash the echoes come from; <c>null</c> when the save folder has no stash file for the character.</summary>
    public StashMaterialsViewModel? Stash { get; }

    // ===== Panels of the game's tab, as rows of echoes =====
    public IReadOnlyList<IReadOnlyList<WovenEchoSlotViewModel>> Encounters { get; }
    public IReadOnlyList<IReadOnlyList<WovenEchoSlotViewModel>> Bosses { get; }
    public IReadOnlyList<IReadOnlyList<WovenEchoSlotViewModel>> Crafting { get; }
    public IReadOnlyList<IReadOnlyList<WovenEchoSlotViewModel>> Auras { get; }

    /// <summary>Echoes the stash holds that the game's tab has no slot for, or that the database does not know.</summary>
    public ObservableCollection<WovenEchoSlotViewModel> Others { get; } = [];

    public bool HasOthers => Others.Count > 0;

    /// <summary>The base.db has no woven echoes (missing or generated without them): names and icons are missing.</summary>
    public bool IsDatabaseMissing => _catalog.GetWovenEchoes().Count == 0;

    /// <summary>The stash exists but none of its characters has unlocked the "Woven Echoes" tab in game yet.</summary>
    public bool IsTabLocked => Stash is { IsWovenEchoesTabUnlocked: false };

    /// <summary>Line under the section, e.g. "Kept in STASH_CYCLE_8_0 · 3 echoes".</summary>
    public string Summary => Stash is { } stash
        ? $"Kept in {stash.Stash.FileName} · {Count(stash.WovenEchoes.Values.Sum())}. Changes are written to the stash file when the character is saved."
        : "No stash file was found for this character in the save folder, so its woven echoes cannot be edited.";

    private static string Count(int echoes) => echoes == 1 ? "1 echo" : $"{echoes:N0} echoes";

    private IReadOnlyList<IReadOnlyList<WovenEchoSlotViewModel>> CreateRows(int[][] rows) =>
        rows.Select(row => (IReadOnlyList<WovenEchoSlotViewModel>)row
                .Select(id => _fixed[id] = CreateSlot(id, _catalog.FindWovenEcho(id), isInTab: true))
                .ToList())
            .ToList();

    private WovenEchoSlotViewModel CreateSlot(int id, WovenEcho? echo, bool isInTab)
    {
        var slot = new WovenEchoSlotViewModel(id, echo, _iconProvider) { IsEditable = Stash is not null, IsInTab = isInTab };
        slot.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(WovenEchoSlotViewModel.Amount) && !_refreshing)
                Stash?.SetWovenEchoAmount(slot.Id, slot.Amount);
        };
        return slot;
    }

    /// <summary>Shows the amounts of the stash; echoes the tab has no slot for get one under "Other".</summary>
    private void Refresh()
    {
        _refreshing = true;
        try
        {
            var amounts = Stash?.WovenEchoes ?? new Dictionary<int, int>();
            foreach (var (id, slot) in _fixed)
                slot.Amount = amounts.GetValueOrDefault(id);

            // An "Other" slot set to 0 stays until the amounts change elsewhere, so it can be typed into again.
            foreach (var other in Others.ToList())
            {
                if (!amounts.ContainsKey(other.Id) && other.Amount > 0)
                    Others.Remove(other);
                else
                    other.Amount = amounts.GetValueOrDefault(other.Id);
            }

            var known = Others.Select(o => o.Id).ToHashSet();
            foreach (var (id, quantity) in amounts.OrderBy(a => a.Key))
            {
                if (quantity <= 0 || _fixed.ContainsKey(id) || known.Contains(id))
                    continue;

                var slot = CreateSlot(id, _catalog.FindWovenEcho(id), isInTab: false);
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

/// <summary>One woven echo of the "Woven echoes" section: its icon, name, description and the amount the stash holds.</summary>
public sealed partial class WovenEchoSlotViewModel(int id, WovenEcho? echo, IIconProvider iconProvider) : ObservableObject
{
    /// <summary>The echo's <c>subTypeID</c> (item type 107).</summary>
    public int Id { get; } = id;

    /// <summary>The echo in <c>base.db</c>; <c>null</c> when the database does not know it.</summary>
    public WovenEcho? Echo { get; } = echo;

    public string Name => Echo?.Name ?? $"Woven echo #{Id}";

    /// <summary>The echo has a slot in the game's Woven Echoes tab (set for the slots of the section's panels).</summary>
    public bool IsInTab { get; init; }

    public ImageSource? Icon => iconProvider.Get(Echo?.IconPath);

    public bool HasIcon => Icon is not null;

    /// <summary>Text on a slot without an icon: the first letters of its name ("FT" for False Temple).</summary>
    public string Initials
    {
        get
        {
            var initials = string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(3).Select(w => char.ToUpperInvariant(w[0])));
            return initials.Length > 0 ? initials : "?";
        }
    }

    /// <summary>Tooltip: the name and what the echo holds.</summary>
    public string ToolTip => Echo switch
    {
        { CannotDrop: false } when !IsInTab =>
            $"{Name}\n\nThe game's Woven Echoes tab has no slot for this echo, so the game does not show it.",
        null => $"{Name} — not in the game database (generate or update it with the database button in the title bar)",
        { CannotDrop: true } => $"{Name}\n\nThe game does not drop this echo; a placeholder like this one shows as \"ERROR\" in the game. Set it to 0 to remove it.",
        { } known => string.IsNullOrWhiteSpace(known.Description) ? Name : $"{Name}\n\n{known.Description}"
    };

    /// <summary>Tooltip of the amount box.</summary>
    public string AmountToolTip => "Echoes in the stash's Woven Echoes tab (0 removes the echo when saved)";

    /// <summary>The amount can be changed (the character has a stash file).</summary>
    public bool IsEditable { get; init; }

    /// <summary>Amount the stash holds; 0 removes the echo on the next save.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private int _amount;

    /// <summary>The stash holds none: the slot is dimmed.</summary>
    public bool IsEmpty => Amount == 0;

    partial void OnAmountChanged(int value)
    {
        var clamped = Math.Clamp(value, 0, CraftingMaterialStorageViewModel.MaxQuantity);
        if (clamped != value)
            Amount = clamped;
    }
}

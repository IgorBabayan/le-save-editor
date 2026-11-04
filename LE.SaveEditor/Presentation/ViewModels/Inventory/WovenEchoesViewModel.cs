using System.Collections.ObjectModel;
using System.Windows.Media;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// The "Woven echoes" section of the inventory: every woven echo the game drops, with the amount the "Woven Echoes" tab
/// of the character's stash holds (dimmed at 0). Type an amount into an echo to add it (or 0 to remove it); echoes the
/// game does not drop (placeholders like "Fading Echo", which the game shows as "ERROR", or echoes unknown to the
/// database) are listed under "Other" while the stash holds some. The echoes live in the <c>wovenEchoesList</c> of the
/// stash file and are written with the other stash changes when the character is saved.
/// </summary>
public sealed partial class WovenEchoesViewModel : ViewModelBase
{
    private readonly IItemBaseCatalog _catalog;
    private readonly IIconProvider _iconProvider;
    private readonly Dictionary<int, WovenEchoSlotViewModel> _droppable = [];

    /// <summary>Set while the slots are filled from the stash, so they do not write the amounts back.</summary>
    private bool _refreshing;

    public WovenEchoesViewModel(StashMaterialsViewModel? stash, IItemBaseCatalog catalog, IIconProvider iconProvider)
    {
        Stash = stash;
        _catalog = catalog;
        _iconProvider = iconProvider;

        Echoes = catalog.GetWovenEchoes()
            .Where(e => !e.CannotDrop)
            .Select(e => _droppable[e.Id] = CreateSlot(e.Id, e))
            .ToList();

        if (stash is not null)
            stash.Changed += (_, _) => Refresh();
        Refresh();
    }

    /// <summary>The stash the echoes come from; <c>null</c> when the save folder has no stash file for the character.</summary>
    public StashMaterialsViewModel? Stash { get; }

    /// <summary>Every woven echo the game drops, by name.</summary>
    public IReadOnlyList<WovenEchoSlotViewModel> Echoes { get; }

    /// <summary>Echoes the stash holds that the game does not drop, or that the database does not know.</summary>
    public ObservableCollection<WovenEchoSlotViewModel> Others { get; } = [];

    public bool HasOthers => Others.Count > 0;

    /// <summary>The base.db has no woven echoes (missing or generated without them): only the echoes the stash holds are shown.</summary>
    public bool IsDatabaseMissing => Echoes.Count == 0;

    /// <summary>The stash exists but none of its characters has unlocked the "Woven Echoes" tab in game yet.</summary>
    public bool IsTabLocked => Stash is { IsWovenEchoesTabUnlocked: false };

    /// <summary>Line under the section, e.g. "Kept in STASH_CYCLE_8_0 · 3 echoes".</summary>
    public string Summary => Stash is { } stash
        ? $"Kept in {stash.Stash.FileName} · {Count(stash.WovenEchoes.Values.Sum())}. Changes are written to the stash file when the character is saved."
        : "No stash file was found for this character in the save folder, so its woven echoes cannot be edited.";

    private static string Count(int echoes) => echoes == 1 ? "1 echo" : $"{echoes:N0} echoes";

    private WovenEchoSlotViewModel CreateSlot(int id, WovenEcho? echo)
    {
        var slot = new WovenEchoSlotViewModel(id, echo, _iconProvider) { IsEditable = Stash is not null };
        slot.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(WovenEchoSlotViewModel.Amount) && !_refreshing)
                Stash?.SetWovenEchoAmount(slot.Id, slot.Amount);
        };
        return slot;
    }

    /// <summary>Shows the amounts of the stash; echoes outside the droppable list get a slot under "Other".</summary>
    private void Refresh()
    {
        _refreshing = true;
        try
        {
            var amounts = Stash?.WovenEchoes ?? new Dictionary<int, int>();
            foreach (var (id, slot) in _droppable)
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
                if (quantity <= 0 || _droppable.ContainsKey(id) || known.Contains(id))
                    continue;

                var slot = CreateSlot(id, _catalog.FindWovenEcho(id));
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

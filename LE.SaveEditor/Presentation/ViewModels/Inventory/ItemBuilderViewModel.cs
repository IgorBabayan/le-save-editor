using System.Collections.ObjectModel;
using System.Windows.Media;
using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>Which list the item builder offers; it follows the selected rarity.</summary>
public enum ItemListKind
{
    /// <summary>Plain bases: Normal, Magic, Rare and Exalted.</summary>
    Bases,

    /// <summary>Unique items: Unique and Legendary share this list.</summary>
    Uniques,

    /// <summary>Set items.</summary>
    Sets
}

/// <summary>
/// Item builder popup opened from an equipment slot: only items that fit the slot are offered, grouped by the
/// classes that can equip them (the character's own class first); the rarity tints the preview and picks the list
/// (bases, uniques or set items), and every implicit / unique modifier of the selection gets a slider.
/// </summary>
public sealed partial class ItemBuilderViewModel : ViewModelBase
{
    private readonly IItemBaseCatalog _catalog;
    private readonly IIconProvider _iconProvider;
    private readonly Func<string, string, bool> _confirm;
    private IReadOnlyList<BaseOptionViewModel> _allBases = [];
    private RarityOptionViewModel _selectedRarity;

    /// <param name="confirm">Asks a Yes/No question (title, message); <c>true</c> for Yes.</param>
    public ItemBuilderViewModel(
        IItemBaseCatalog catalog,
        IIconProvider iconProvider,
        Func<string, string, bool> confirm,
        EquipmentSlot slot,
        CharacterClass characterClass,
        BuiltItem? current)
    {
        _catalog = catalog;
        _iconProvider = iconProvider;
        _confirm = confirm;
        Slot = slot;
        CharacterClass = characterClass;
        IsDatabaseAvailable = catalog.IsAvailable;
        IsDatabaseOutdated = catalog.IsOutdated;

        Rarities = Enum.GetValues<ItemRarity>().Select(r => new RarityOptionViewModel(r)).ToList();
        _selectedRarity = Rarities.First(r => r.Rarity == (current?.Rarity ?? ItemRarity.Normal));
        _selectedRarity.IsSelected = true;
        ListKind = KindOf(_selectedRarity.Rarity);

        LoadList();

        // Editing an equipped item: select it again and keep its rolls.
        if (current is not null)
        {
            var match = _allBases.FirstOrDefault(o => o.Base.BaseTypeId == current.Base.BaseTypeId
                                                      && o.Base.SubTypeId == current.Base.SubTypeId
                                                      && o.Base.UniqueId == current.Base.UniqueId);
            if (match is not null)
            {
                SelectedBaseOption = match;
                FillStats(match.Base, current.ImplicitValues, current.ModValues);

                // An equipped item of another class stays listed even though the class filter is on.
                ApplyFilter();
            }
        }
    }

    /// <summary>Raised when the dialog should close; the argument is the built item, or <c>null</c> when cancelled.</summary>
    public event EventHandler<BuiltItem?>? CloseRequested;

    public EquipmentSlot Slot { get; }

    public string Title => $"Item builder — {Slot.DisplayName}";

    /// <summary>Class of the character whose equipment is edited; its items are listed first.</summary>
    public CharacterClass CharacterClass { get; }

    /// <summary><c>false</c> when <c>base.db</c> is missing or unreadable: the user has to generate it first.</summary>
    public bool IsDatabaseAvailable { get; }

    /// <summary><c>base.db</c> predates stored stat names, so implicits read "Property 10" until it is updated.</summary>
    public bool IsDatabaseOutdated { get; }

    // ===== Rarity and list kind =====

    public IReadOnlyList<RarityOptionViewModel> Rarities { get; }

    /// <summary>Selected rarity; changed only through <see cref="SelectRarityCommand"/>.</summary>
    public RarityOptionViewModel SelectedRarity => _selectedRarity;

    /// <summary>
    /// A rarity pill was clicked. Moving to a rarity with another list (bases ↔ uniques ↔ set items) while an item
    /// is selected asks first: "No" leaves everything as it was, "Yes" clears the selection and switches the list.
    /// </summary>
    [RelayCommand]
    private void SelectRarity(RarityOptionViewModel? option)
    {
        if (option is null || option == _selectedRarity)
            return;

        if (KindOf(option.Rarity) != ListKind && HasSelectedBase && !ConfirmListChange(option))
            return;

        ApplyRarity(option);
    }

    /// <summary>List offered for the selected rarity.</summary>
    public ItemListKind ListKind { get; private set; }

    public Brush RarityForeground => RarityPalette.Foreground(SelectedRarity.Rarity);

    public Brush RarityBackground => RarityPalette.Background(SelectedRarity.Rarity);

    private static ItemListKind KindOf(ItemRarity rarity) => rarity switch
    {
        ItemRarity.Unique or ItemRarity.Legendary => ItemListKind.Uniques,
        ItemRarity.Set => ItemListKind.Sets,
        _ => ItemListKind.Bases
    };

    private bool ConfirmListChange(RarityOptionViewModel value)
    {
        var target = KindOf(value.Rarity) switch
        {
            ItemListKind.Uniques => "unique items",
            ItemListKind.Sets => "set items",
            _ => "item bases"
        };

        return _confirm("Change rarity", $"Switching to {value.Name} shows {target} instead and clears the selected item and its stats. Continue?");
    }

    private void ApplyRarity(RarityOptionViewModel value)
    {
        _selectedRarity.IsSelected = false;
        _selectedRarity = value;
        _selectedRarity.IsSelected = true;
        OnPropertyChanged(nameof(SelectedRarity));
        OnPropertyChanged(nameof(RarityForeground));
        OnPropertyChanged(nameof(RarityBackground));
        OnPropertyChanged(nameof(Subtitle));

        var kind = KindOf(value.Rarity);
        if (kind == ListKind)
            return;

        ListKind = kind;
        SelectedBaseOption = null;
        SearchText = string.Empty;
        LoadList();

        OnPropertyChanged(nameof(ListKind));
        OnPropertyChanged(nameof(ListHeader));
        OnPropertyChanged(nameof(OnlyUsableText));
        OnPropertyChanged(nameof(ItemName));
    }

    // ===== Item list =====

    /// <summary>"Base", "Unique item" or "Set item".</summary>
    public string ListHeader => ListKind switch
    {
        ItemListKind.Uniques => "Unique item",
        ItemListKind.Sets => "Set item",
        _ => "Base"
    };

    private string ListNoun => ListKind switch
    {
        ItemListKind.Uniques => "unique items",
        ItemListKind.Sets => "set items",
        _ => "bases"
    };

    public string OnlyUsableText => $"Only {CharacterClassCatalog.GetDisplayName(CharacterClass)} {ListNoun}";

    public string EmptyBasesText =>
        !IsDatabaseAvailable ? "The game database was not found. Generate it with the database button in the title bar, then reopen the builder."
        : _allBases.Count == 0 ? $"No {Slot.DisplayName.ToLowerInvariant()} {ListNoun} were found in the game database."
        : ShowOnlyUsable && !_allBases.Any(o => o.IsUsable) ? $"No {Slot.DisplayName.ToLowerInvariant()} {ListNoun} for this class."
        : $"No {ListNoun} match the search.";

    /// <summary>Items of the list matching <see cref="SearchText"/> (and <see cref="ShowOnlyUsable"/>), in group order.</summary>
    public ObservableCollection<BaseOptionViewModel> Bases { get; } = [];

    public bool HasBases => Bases.Count > 0;

    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>Hide items the character's class cannot equip (on by default).</summary>
    [ObservableProperty]
    private bool _showOnlyUsable = true;

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnShowOnlyUsableChanged(bool value) => ApplyFilter();

    private void LoadList()
    {
        var items = ListKind switch
        {
            ItemListKind.Uniques => _catalog.GetUniques(Slot, sets: false),
            ItemListKind.Sets => _catalog.GetUniques(Slot, sets: true),
            _ => _catalog.GetBases(Slot)
        };

        _allBases = items
            .Select(b => new BaseOptionViewModel(b, CharacterClass))
            .OrderBy(o => o.GroupOrder)
            .ThenBy(o => o.GroupName, StringComparer.Ordinal)
            .ThenBy(o => o.Base.CannotDrop)
            .ThenBy(o => o.Base.LevelRequirement)
            .ThenBy(o => o.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var selected = SelectedBaseOption;
        var words = SearchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        Bases.Clear();
        foreach (var option in _allBases)
        {
            // The current choice always stays listed (the ListBox would clear it otherwise), in its own group.
            var matches = option == selected
                          || ((!ShowOnlyUsable || option.IsUsable)
                              && words.All(w => option.Name.Contains(w, StringComparison.CurrentCultureIgnoreCase)
                                                || option.Base.TypeName.Contains(w, StringComparison.CurrentCultureIgnoreCase)
                                                || (option.Base.BaseName?.Contains(w, StringComparison.CurrentCultureIgnoreCase) ?? false)
                                                || option.GroupName.Contains(w, StringComparison.CurrentCultureIgnoreCase)));
            if (matches)
                Bases.Add(option);
        }

        OnPropertyChanged(nameof(HasBases));
        OnPropertyChanged(nameof(EmptyBasesText));
    }

    // ===== Selection and stats =====

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedBase))]
    [NotifyPropertyChangedFor(nameof(HasSelectedBase))]
    [NotifyPropertyChangedFor(nameof(ItemName))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(LevelText))]
    [NotifyPropertyChangedFor(nameof(Icon))]
    [NotifyPropertyChangedFor(nameof(IsSelectedBaseForOtherClass))]
    [NotifyPropertyChangedFor(nameof(ClassWarningText))]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    private BaseOptionViewModel? _selectedBaseOption;

    public ItemBase? SelectedBase => SelectedBaseOption?.Base;

    public bool HasSelectedBase => SelectedBase is not null;

    /// <summary>The selected item cannot be equipped by the character's class (it can still be built).</summary>
    public bool IsSelectedBaseForOtherClass => SelectedBaseOption is { IsUsable: false };

    public string ClassWarningText => SelectedBaseOption is { IsUsable: false } o
        ? $"Only {o.GroupName} can equip this item."
        : string.Empty;

    /// <summary>Implicits of the selected base (for a unique: of the base it is built on).</summary>
    public ObservableCollection<ItemStatViewModel> Stats { get; } = [];

    public bool HasStats => Stats.Count > 0;

    /// <summary>Rolled modifiers of the selected unique / set item.</summary>
    public ObservableCollection<ItemStatViewModel> ModStats { get; } = [];

    public bool HasModStats => ModStats.Count > 0;

    /// <summary>Tooltip lines of the selected unique / set item that do not roll.</summary>
    public IReadOnlyList<string> Effects => SelectedBase?.Effects ?? [];

    public bool HasEffects => Effects.Count > 0;

    /// <summary>The unique / set section (modifiers or effect lines) has something to show.</summary>
    public bool HasUniqueSection => HasModStats || HasEffects;

    public string ModStatsHeader => ListKind == ItemListKind.Sets ? "Set item stats" : "Unique stats";

    public string EmptyStatsText => $"Select {(ListKind == ItemListKind.Bases ? "a base" : ListKind == ItemListKind.Sets ? "a set item" : "a unique item")} to see its stats.";

    public string ItemName => SelectedBase?.Name ?? $"Select a {Slot.DisplayName.ToLowerInvariant()} {ListHeader.ToLowerInvariant()}";

    /// <summary>E.g. "Rare Helmet", or "Unique Helmet · Jewelled Circlet" for a unique.</summary>
    public string Subtitle
    {
        get
        {
            var text = $"{SelectedRarity.Name} {(SelectedBase?.TypeName is { Length: > 0 } type ? type : Slot.DisplayName)}";
            return SelectedBase?.BaseName is { } baseName ? $"{text} · {baseName}" : text;
        }
    }

    public string LevelText => SelectedBase is { LevelRequirement: > 0 } b ? $"Requires level {b.LevelRequirement}" : string.Empty;

    public ImageSource? Icon => _iconProvider.Get(SelectedBase?.IconPath);

    partial void OnSelectedBaseOptionChanged(BaseOptionViewModel? value) => FillStats(value?.Base, null, null);

    private void FillStats(ItemBase? item, IReadOnlyList<double>? implicitValues, IReadOnlyList<double>? modValues)
    {
        Stats.Clear();
        ModStats.Clear();

        if (item is not null)
        {
            for (var i = 0; i < item.Implicits.Count; i++)
                Stats.Add(new ItemStatViewModel(item.Implicits[i], implicitValues is not null && i < implicitValues.Count ? implicitValues[i] : null));
            for (var i = 0; i < item.Mods.Count; i++)
                ModStats.Add(new ItemStatViewModel(item.Mods[i], modValues is not null && i < modValues.Count ? modValues[i] : null));
        }

        OnPropertyChanged(nameof(HasStats));
        OnPropertyChanged(nameof(HasModStats));
        OnPropertyChanged(nameof(Effects));
        OnPropertyChanged(nameof(HasEffects));
        OnPropertyChanged(nameof(HasUniqueSection));
        OnPropertyChanged(nameof(ModStatsHeader));
        OnPropertyChanged(nameof(EmptyStatsText));
    }

    [RelayCommand(CanExecute = nameof(HasSelectedBase))]
    private void Apply()
    {
        if (SelectedBase is null)
            return;

        CloseRequested?.Invoke(this, new BuiltItem(
            Slot,
            SelectedRarity.Rarity,
            SelectedBase,
            Stats.Select(s => s.Value).ToList(),
            ModStats.Select(s => s.Value).ToList()));
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(this, null);
}

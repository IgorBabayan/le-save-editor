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
/// Affix slots follow the rarity: bases take 2 prefixes, 2 suffixes, a sealed prefix, a sealed suffix and a
/// corruption; uniques / legendaries 2 prefixes, 2 suffixes and a corruption (a unique becomes legendary as soon as
/// it gets an affix); set items only a corruption. Only affixes that can roll on the item's type are offered.
/// </summary>
public sealed partial class ItemBuilderViewModel : ViewModelBase
{
    private readonly IItemBaseCatalog _catalog;
    private readonly IIconProvider _iconProvider;
    private readonly Func<string, string, bool> _confirm;
    private IReadOnlyList<BaseOptionViewModel> _allBases = [];
    private RarityOptionViewModel _selectedRarity;

    /// <param name="confirm">Asks a Yes/No question (title, message); <c>true</c> for Yes.</param>
    /// <summary>Forging potential of the item being edited, kept when it is applied again (the builder does not edit it).</summary>
    private readonly int? _forgingPotential;

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
        _forgingPotential = current?.ForgingPotential;
        Slot = slot;
        CharacterClass = characterClass;
        IsDatabaseAvailable = catalog.IsAvailable;
        IsDatabaseOutdated = catalog.IsOutdated;

        Rarities = Enum.GetValues<ItemRarity>().Select(r => new RarityOptionViewModel(r)).ToList();
        _selectedRarity = Rarities.First(r => r.Rarity == (current?.Rarity ?? ItemRarity.Normal));
        _selectedRarity.IsSelected = true;
        ListKind = KindOf(_selectedRarity.Rarity);

        LoadList();
        BuildAffixSlots();

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
                RestoreAffixes(current.Affixes);

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

        // A unique has no affixes: going back from legendary removes them (after asking).
        if (option.Rarity == ItemRarity.Unique && HasAnyAffix)
        {
            if (!_confirm("Change rarity", "A unique item has no affixes. Switching to Unique removes its prefixes, suffixes and corruption. Continue?"))
                return;
            ClearAffixes();
        }

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
        BuildAffixSlots();

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
        : !_allBases.Any(o => !o.Base.CannotDrop) ? $"No {Slot.DisplayName.ToLowerInvariant()} {ListNoun} were found in the game database."
        : ShowOnlyUsable && !_allBases.Any(o => o.IsUsable && !o.Base.CannotDrop) ? $"No {Slot.DisplayName.ToLowerInvariant()} {ListNoun} for this class."
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
            .Select(b => new BaseOptionViewModel(b, CharacterClass, _iconProvider))
            .OrderBy(o => o.GroupOrder)
            .ThenBy(o => o.GroupName, StringComparer.Ordinal)
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
            // The current choice always stays listed (the ListBox would clear it otherwise), in its own group;
            // legacy bases (no longer dropped) are hidden otherwise.
            var matches = option == selected
                          || (!option.Base.CannotDrop
                              && (!ShowOnlyUsable || option.IsUsable)
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
    [NotifyPropertyChangedFor(nameof(HasAffixSlots))]
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

    partial void OnSelectedBaseOptionChanged(BaseOptionViewModel? value)
    {
        FillStats(value?.Base, null, null);

        // Affixes depend on the item type (a wand and an axe share the weapon slot but not their affixes).
        foreach (var slot in AffixSlots)
        {
            if (slot.Affix is { } affix && (value is null || !affix.CanRollOnType(value.Base.BaseTypeId)))
                slot.Clear();
        }

        AffixPickerSlot = null;
        OnAffixesChanged();
    }

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
            ModStats.Select(s => s.Value).ToList(),
            AffixSlots.Select(a => a.ToBuiltAffix()).OfType<BuiltAffix>().ToList(),
            SelectedBase.IsUnique ? null : _forgingPotential));
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(this, null);

    // ===== Affixes =====

    /// <summary>Affix slots of the current rarity (empty until an item is selected, see <see cref="HasAffixSlots"/>).</summary>
    public ObservableCollection<AffixSlotViewModel> AffixSlots { get; } = [];

    /// <summary>An item is selected and its rarity takes affixes.</summary>
    public bool HasAffixSlots => HasSelectedBase && AffixSlots.Count > 0;

    public bool HasAnyAffix => AffixSlots.Any(s => s.HasAffix);

    /// <summary>"Affixes", or "Corruption" for set items (they take nothing else).</summary>
    public string AffixesHeader => ListKind == ItemListKind.Sets ? "Corruption" : "Affixes";

    /// <summary>Slot being filled from the affix picker; <c>null</c> while the item grid is shown.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPickingAffix))]
    [NotifyPropertyChangedFor(nameof(AffixPickerTitle))]
    private AffixSlotViewModel? _affixPickerSlot;

    public bool IsPickingAffix => AffixPickerSlot is not null;

    public string AffixPickerTitle => AffixPickerSlot is { } slot ? $"Choose {slot.Label.ToLowerInvariant()}" : string.Empty;

    /// <summary>Affixes for <see cref="AffixPickerSlot"/> matching <see cref="AffixSearchText"/>, in group order.</summary>
    public ObservableCollection<AffixOptionViewModel> AffixOptions { get; } = [];

    public bool HasAffixOptions => AffixOptions.Count > 0;

    [ObservableProperty]
    private string _affixSearchText = string.Empty;

    /// <summary>Picking an affix in the picker puts it in <see cref="AffixPickerSlot"/> and closes the picker.</summary>
    [ObservableProperty]
    private AffixOptionViewModel? _selectedAffixOption;

    partial void OnAffixSearchTextChanged(string value) => LoadAffixOptions();

    partial void OnAffixPickerSlotChanged(AffixSlotViewModel? value)
    {
        AffixSearchText = string.Empty;
        LoadAffixOptions();
    }

    partial void OnSelectedAffixOptionChanged(AffixOptionViewModel? value)
    {
        if (value is null || AffixPickerSlot is not { } slot)
            return;

        slot.Set(value.Affix);
        AffixPickerSlot = null;
        SelectedAffixOption = null;
        OnAffixesChanged();
    }

    [RelayCommand]
    private void CloseAffixPicker() => AffixPickerSlot = null;

    /// <summary>Slots for the current list: bases 2P / 2S / sealed P / sealed S / corruption, uniques 2P / 2S / corruption, sets corruption.</summary>
    private void BuildAffixSlots()
    {
        AffixSlotKind[] kinds = ListKind switch
        {
            ItemListKind.Sets => [AffixSlotKind.Corrupted],
            ItemListKind.Uniques => [AffixSlotKind.Prefix, AffixSlotKind.Prefix, AffixSlotKind.Suffix, AffixSlotKind.Suffix, AffixSlotKind.Corrupted],
            _ => [AffixSlotKind.Prefix, AffixSlotKind.Prefix, AffixSlotKind.Suffix, AffixSlotKind.Suffix,
                  AffixSlotKind.SealedPrefix, AffixSlotKind.SealedSuffix, AffixSlotKind.Corrupted]
        };

        AffixPickerSlot = null;
        AffixSlots.Clear();
        foreach (var kind in kinds)
        {
            var slot = new AffixSlotViewModel(kind, s => AffixPickerSlot = s, RemoveAffix);
            // A tier change can make the item exalted (or not).
            slot.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(AffixSlotViewModel.TierNumber))
                    OnAffixesChanged();
            };
            AffixSlots.Add(slot);
        }

        OnAffixesChanged();
    }

    /// <summary>Puts the affixes of an equipped item back into slots of the same kind, in order.</summary>
    private void RestoreAffixes(IReadOnlyList<BuiltAffix> affixes)
    {
        foreach (var affix in affixes)
        {
            var slot = AffixSlots.FirstOrDefault(s => s.Kind == affix.Slot && !s.HasAffix);
            slot?.Set(affix.Affix, affix.Tier, affix.Values);
        }

        OnAffixesChanged();
    }

    private void RemoveAffix(AffixSlotViewModel slot)
    {
        slot.Clear();
        if (AffixPickerSlot == slot)
            AffixPickerSlot = null;
        OnAffixesChanged();
    }

    private void ClearAffixes()
    {
        foreach (var slot in AffixSlots)
            slot.Clear();
        AffixPickerSlot = null;
        OnAffixesChanged();
    }

    /// <summary>Lowest tier that makes an affix exalted (T6–T8).</summary>
    private const int ExaltedTier = 6;

    private void OnAffixesChanged()
    {
        // A unique with any affix is a legendary.
        if (SelectedRarity.Rarity == ItemRarity.Unique && HasAnyAffix)
            ApplyRarity(Rarities.First(r => r.Rarity == ItemRarity.Legendary));

        // Plain bases take their rarity from their affixes, like in the game (set / unique / legendary keep theirs).
        if (ListKind == ItemListKind.Bases && HasSelectedBase)
        {
            var rarity = RarityFromAffixes();
            if (rarity != SelectedRarity.Rarity)
                ApplyRarity(Rarities.First(r => r.Rarity == rarity));
        }

        OnPropertyChanged(nameof(HasAffixSlots));
        OnPropertyChanged(nameof(HasAnyAffix));
        OnPropertyChanged(nameof(AffixesHeader));
    }

    /// <summary>
    /// Normal without affixes, Magic with 1–2, Rare with 3–4, Exalted as soon as one affix is tier 6 or higher.
    /// Prefixes, suffixes and sealed affixes count; the corruption does not change the rarity.
    /// </summary>
    private ItemRarity RarityFromAffixes()
    {
        var affixes = AffixSlots.Where(s => s.HasAffix && !s.IsCorrupted).ToList();
        if (affixes.Any(s => s.TierNumber >= ExaltedTier))
            return ItemRarity.Exalted;

        return affixes.Count switch
        {
            0 => ItemRarity.Normal,
            <= 2 => ItemRarity.Magic,
            _ => ItemRarity.Rare
        };
    }

    private void LoadAffixOptions()
    {
        AffixOptions.Clear();
        if (AffixPickerSlot is not { } slot || SelectedBase is not { } item)
        {
            OnPropertyChanged(nameof(HasAffixOptions));
            return;
        }

        var className = CharacterClassCatalog.GetDisplayName(CharacterClass);
        // The same affix cannot be on an item twice.
        var used = AffixSlots.Where(s => s != slot && s.Affix is not null).Select(s => s.Affix!.Id).ToHashSet();
        var words = AffixSearchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var options = _catalog.GetAffixes(item.BaseTypeId)
            .Where(a => slot.Accepts(a)
                        && !used.Contains(a.Id)
                        // Class affixes only for the character's own class.
                        && (a.ClassName is null || string.Equals(a.ClassName, className, StringComparison.OrdinalIgnoreCase))
                        && words.All(w => a.Name.Contains(w, StringComparison.CurrentCultureIgnoreCase)
                                          || (a.Title?.Contains(w, StringComparison.CurrentCultureIgnoreCase) ?? false)
                                          || a.GroupName.Contains(w, StringComparison.CurrentCultureIgnoreCase)))
            // Normal affixes first, then experimental / personal / set / variant; by category, then name.
            .OrderBy(a => a.Category == AffixCategory.Normal ? 0 : 1)
            .ThenBy(a => a.GroupName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase);

        foreach (var affix in options)
            AffixOptions.Add(new AffixOptionViewModel(affix));

        OnPropertyChanged(nameof(HasAffixOptions));
    }
}

using System.Collections.ObjectModel;
using System.Windows.Media;
using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// Item builder popup opened from an equipment slot: only bases that fit the slot are offered, grouped by the
/// classes that can equip them (the character's own class first); the rarity tints the preview and every implicit
/// of the selected base gets a slider.
/// </summary>
public sealed partial class ItemBuilderViewModel : ViewModelBase
{
    private readonly IIconProvider _iconProvider;
    private readonly IReadOnlyList<BaseOptionViewModel> _allBases;

    public ItemBuilderViewModel(
        IItemBaseCatalog catalog,
        IIconProvider iconProvider,
        EquipmentSlot slot,
        CharacterClass characterClass,
        BuiltItem? current)
    {
        _iconProvider = iconProvider;
        Slot = slot;
        CharacterClass = characterClass;
        IsDatabaseAvailable = catalog.IsAvailable;
        IsDatabaseOutdated = catalog.IsOutdated;

        _allBases = catalog.GetBases(slot)
            .Select(b => new BaseOptionViewModel(b, characterClass))
            .OrderBy(o => o.GroupOrder)
            .ThenBy(o => o.GroupName, StringComparer.Ordinal)
            .ThenBy(o => o.Base.CannotDrop)
            .ThenBy(o => o.Base.LevelRequirement)
            .ThenBy(o => o.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        Rarities = Enum.GetValues<ItemRarity>().Select(r => new RarityOptionViewModel(r)).ToList();
        _selectedRarity = Rarities.First(r => r.Rarity == (current?.Rarity ?? ItemRarity.Normal));

        ApplyFilter();

        // Editing an equipped item: select its base and keep its rolls.
        if (current is not null)
        {
            var match = _allBases.FirstOrDefault(o => o.Base.BaseTypeId == current.Base.BaseTypeId && o.Base.SubTypeId == current.Base.SubTypeId);
            if (match is not null)
            {
                SelectedBaseOption = match;
                Stats.Clear();
                for (var i = 0; i < match.Base.Implicits.Count; i++)
                    Stats.Add(new ItemStatViewModel(match.Base.Implicits[i], i < current.ImplicitValues.Count ? current.ImplicitValues[i] : null));
            }
        }
    }

    /// <summary>Raised when the dialog should close; the argument is the built item, or <c>null</c> when cancelled.</summary>
    public event EventHandler<BuiltItem?>? CloseRequested;

    public EquipmentSlot Slot { get; }

    public string Title => $"Item builder — {Slot.DisplayName}";

    /// <summary>Class of the character whose equipment is edited; its bases are listed first.</summary>
    public CharacterClass CharacterClass { get; }

    public string OnlyUsableText => $"Only {CharacterClassCatalog.GetDisplayName(CharacterClass)} bases";

    /// <summary><c>false</c> when <c>base.db</c> is missing or unreadable: the user has to generate it first.</summary>
    public bool IsDatabaseAvailable { get; }

    /// <summary><c>base.db</c> predates stored stat names, so implicits read "Property 10" until it is updated.</summary>
    public bool IsDatabaseOutdated { get; }

    public string EmptyBasesText =>
        !IsDatabaseAvailable ? "The game database was not found. Generate it with the database button in the title bar, then reopen the builder."
        : _allBases.Count == 0 ? $"No {Slot.DisplayName.ToLowerInvariant()} bases were found in the game database."
        : ShowOnlyUsable && !_allBases.Any(o => o.IsUsable) ? $"No {Slot.DisplayName.ToLowerInvariant()} bases for this class."
        : "No bases match the search.";

    public IReadOnlyList<RarityOptionViewModel> Rarities { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RarityForeground))]
    [NotifyPropertyChangedFor(nameof(RarityBackground))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    private RarityOptionViewModel _selectedRarity;

    public Brush RarityForeground => RarityPalette.Foreground(SelectedRarity.Rarity);

    public Brush RarityBackground => RarityPalette.Background(SelectedRarity.Rarity);

    /// <summary>Bases of the slot matching <see cref="SearchText"/> (and <see cref="ShowOnlyUsable"/>), in group order.</summary>
    public ObservableCollection<BaseOptionViewModel> Bases { get; } = [];

    public bool HasBases => Bases.Count > 0;

    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>Hide bases the character's class cannot equip.</summary>
    [ObservableProperty]
    private bool _showOnlyUsable;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedBase))]
    [NotifyPropertyChangedFor(nameof(HasSelectedBase))]
    [NotifyPropertyChangedFor(nameof(ItemName))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(LevelText))]
    [NotifyPropertyChangedFor(nameof(Icon))]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    private BaseOptionViewModel? _selectedBaseOption;

    public ItemBase? SelectedBase => SelectedBaseOption?.Base;

    /// <summary>The selected base cannot be equipped by the character's class (it can still be built).</summary>
    public bool IsSelectedBaseForOtherClass => SelectedBaseOption is { IsUsable: false };

    public string ClassWarningText => SelectedBaseOption is { IsUsable: false } o
        ? $"Only {o.GroupName} can equip this base."
        : string.Empty;

    public bool HasSelectedBase => SelectedBase is not null;

    /// <summary>Implicits of <see cref="SelectedBase"/>.</summary>
    public ObservableCollection<ItemStatViewModel> Stats { get; } = [];

    public bool HasStats => Stats.Count > 0;

    public string ItemName => SelectedBase?.Name ?? $"Select a {Slot.DisplayName.ToLowerInvariant()} base";

    public string Subtitle => $"{SelectedRarity.Name} {SelectedBase?.TypeName ?? Slot.DisplayName}";

    public string LevelText => SelectedBase is { LevelRequirement: > 0 } b ? $"Requires level {b.LevelRequirement}" : string.Empty;

    public ImageSource? Icon => _iconProvider.Get(SelectedBase?.IconPath);

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnShowOnlyUsableChanged(bool value) => ApplyFilter();

    partial void OnSelectedBaseOptionChanged(BaseOptionViewModel? value)
    {
        Stats.Clear();
        if (value is not null)
        {
            foreach (var implicitStat in value.Base.Implicits)
                Stats.Add(new ItemStatViewModel(implicitStat));
        }

        OnPropertyChanged(nameof(HasStats));
        OnPropertyChanged(nameof(IsSelectedBaseForOtherClass));
        OnPropertyChanged(nameof(ClassWarningText));
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
                                                || option.GroupName.Contains(w, StringComparison.CurrentCultureIgnoreCase)));
            if (matches)
                Bases.Add(option);
        }

        OnPropertyChanged(nameof(HasBases));
        OnPropertyChanged(nameof(EmptyBasesText));
    }

    [RelayCommand(CanExecute = nameof(HasSelectedBase))]
    private void Apply()
    {
        if (SelectedBase is null)
            return;

        CloseRequested?.Invoke(this, new BuiltItem(Slot, SelectedRarity.Rarity, SelectedBase, Stats.Select(s => s.Value).ToList()));
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(this, null);
}

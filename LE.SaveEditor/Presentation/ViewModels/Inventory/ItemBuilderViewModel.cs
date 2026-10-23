using System.Collections.ObjectModel;
using System.Windows.Media;
using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// Item builder popup opened from an equipment slot: only bases that fit the slot are offered,
/// the rarity tints the preview and every implicit of the selected base gets a slider.
/// </summary>
public sealed partial class ItemBuilderViewModel : ViewModelBase
{
    private readonly IIconProvider _iconProvider;
    private readonly IReadOnlyList<ItemBase> _allBases;

    public ItemBuilderViewModel(IItemBaseCatalog catalog, IIconProvider iconProvider, EquipmentSlot slot, BuiltItem? current)
    {
        _iconProvider = iconProvider;
        Slot = slot;
        _allBases = catalog.GetBases(slot);
        IsDatabaseAvailable = catalog.IsAvailable;

        Rarities = Enum.GetValues<ItemRarity>().Select(r => new RarityOptionViewModel(r)).ToList();
        _selectedRarity = Rarities.First(r => r.Rarity == (current?.Rarity ?? ItemRarity.Normal));

        ApplyFilter();

        // Editing an equipped item: select its base and keep its rolls.
        if (current is not null)
        {
            var match = _allBases.FirstOrDefault(b => b.BaseTypeId == current.Base.BaseTypeId && b.SubTypeId == current.Base.SubTypeId);
            if (match is not null)
            {
                SelectedBase = match;
                Stats.Clear();
                for (var i = 0; i < match.Implicits.Count; i++)
                    Stats.Add(new ItemStatViewModel(match.Implicits[i], i < current.ImplicitValues.Count ? current.ImplicitValues[i] : null));
            }
        }
    }

    /// <summary>Raised when the dialog should close; the argument is the built item, or <c>null</c> when cancelled.</summary>
    public event EventHandler<BuiltItem?>? CloseRequested;

    public EquipmentSlot Slot { get; }

    public string Title => $"Item builder — {Slot.DisplayName}";

    /// <summary><c>false</c> when <c>base.db</c> is missing or unreadable: the user has to generate it first.</summary>
    public bool IsDatabaseAvailable { get; }

    public string EmptyBasesText =>
        !IsDatabaseAvailable ? "The game database was not found. Generate it with the database button in the title bar, then reopen the builder."
        : _allBases.Count == 0 ? $"No {Slot.DisplayName.ToLowerInvariant()} bases were found in the game database."
        : "No bases match the search.";

    public IReadOnlyList<RarityOptionViewModel> Rarities { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RarityForeground))]
    [NotifyPropertyChangedFor(nameof(RarityBackground))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    private RarityOptionViewModel _selectedRarity;

    public Brush RarityForeground => RarityPalette.Foreground(SelectedRarity.Rarity);

    public Brush RarityBackground => RarityPalette.Background(SelectedRarity.Rarity);

    /// <summary>Bases of the slot matching <see cref="SearchText"/>.</summary>
    public ObservableCollection<ItemBase> Bases { get; } = [];

    public bool HasBases => Bases.Count > 0;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedBase))]
    [NotifyPropertyChangedFor(nameof(ItemName))]
    [NotifyPropertyChangedFor(nameof(Subtitle))]
    [NotifyPropertyChangedFor(nameof(LevelText))]
    [NotifyPropertyChangedFor(nameof(Icon))]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    private ItemBase? _selectedBase;

    public bool HasSelectedBase => SelectedBase is not null;

    /// <summary>Implicits of <see cref="SelectedBase"/>.</summary>
    public ObservableCollection<ItemStatViewModel> Stats { get; } = [];

    public bool HasStats => Stats.Count > 0;

    public string ItemName => SelectedBase?.Name ?? $"Select a {Slot.DisplayName.ToLowerInvariant()} base";

    public string Subtitle => $"{SelectedRarity.Name} {SelectedBase?.TypeName ?? Slot.DisplayName}";

    public string LevelText => SelectedBase is { LevelRequirement: > 0 } b ? $"Requires level {b.LevelRequirement}" : string.Empty;

    public ImageSource? Icon => _iconProvider.Get(SelectedBase?.IconPath);

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedBaseChanged(ItemBase? value)
    {
        Stats.Clear();
        if (value is not null)
        {
            foreach (var implicitStat in value.Implicits)
                Stats.Add(new ItemStatViewModel(implicitStat));
        }

        OnPropertyChanged(nameof(HasStats));
    }

    private void ApplyFilter()
    {
        var selected = SelectedBase;
        var words = SearchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        Bases.Clear();
        foreach (var itemBase in _allBases.Where(b => words.All(w =>
                     b.Name.Contains(w, StringComparison.CurrentCultureIgnoreCase)
                     || b.TypeName.Contains(w, StringComparison.CurrentCultureIgnoreCase))))
            Bases.Add(itemBase);

        // Filtering must not drop the current choice (the ListBox would clear it).
        if (selected is not null && !Bases.Contains(selected))
            Bases.Insert(0, selected);

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

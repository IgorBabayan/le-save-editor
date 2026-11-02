using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Media;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// Edits the materials of one kind (affix shards, runes or glyphs) in a stash's material storage: the amount of each
/// material, removing a material and adding one picked in the crafting material picker.
/// </summary>
public sealed partial class CraftingMaterialStorageViewModel : ViewModelBase
{
    /// <summary>Largest amount the editor writes for one material.</summary>
    public const int MaxQuantity = 99_999;

    private readonly IItemBaseCatalog _catalog;
    private readonly IIconProvider _iconProvider;
    private readonly Func<CraftingMaterialKind, CraftingMaterial?> _pickMaterial;

    /// <param name="amounts">Amount of each material of <paramref name="kind"/> in the storage, by sub type.</param>
    /// <param name="stashName">File name of the stash, shown in the window.</param>
    /// <param name="pickMaterial">Opens the crafting material picker for a kind; <c>null</c> when cancelled.</param>
    public CraftingMaterialStorageViewModel(
        IItemBaseCatalog catalog,
        IIconProvider iconProvider,
        Func<CraftingMaterialKind, CraftingMaterial?> pickMaterial,
        CraftingMaterialKind kind,
        IReadOnlyDictionary<int, int> amounts,
        string stashName)
    {
        _catalog = catalog;
        _iconProvider = iconProvider;
        _pickMaterial = pickMaterial;
        Kind = kind;
        StashName = stashName;

        foreach (var (subTypeId, quantity) in amounts.OrderBy(a => a.Key))
            AddEntry(subTypeId, quantity);

        EntriesView = CollectionViewSource.GetDefaultView(Entries);
        EntriesView.Filter = o => o is CraftingMaterialAmountViewModel entry && Matches(entry);
        EntriesView.SortDescriptions.Add(new SortDescription(nameof(CraftingMaterialAmountViewModel.Name), ListSortDirection.Ascending));
    }

    public CraftingMaterialKind Kind { get; }

    /// <summary>File name of the stash the storage belongs to.</summary>
    public string StashName { get; }

    /// <summary>Window title, e.g. "Runes in the material storage".</summary>
    public string Title => $"{Kind.PluralName} in the material storage";

    public string AddText => $"Add {Kind.DisplayName.ToLowerInvariant()}";

    /// <summary>Every material of the kind in the storage (also ones set to 0, until the dialog is applied).</summary>
    public ObservableCollection<CraftingMaterialAmountViewModel> Entries { get; } = [];

    /// <summary><see cref="Entries"/> filtered by <see cref="SearchText"/>, by name.</summary>
    public ICollectionView EntriesView { get; }

    [ObservableProperty]
    private string _searchText = string.Empty;

    public bool HasEntries => Entries.Count > 0;

    /// <summary>Line under the list, e.g. "12 runes · 34 in total".</summary>
    public string Summary
    {
        get
        {
            var kinds = Entries.Count(e => e.Quantity > 0);
            return $"{kinds} {(kinds == 1 ? Kind.DisplayName.ToLowerInvariant() : Kind.PluralName.ToLowerInvariant())} · {Entries.Sum(e => e.Quantity):N0} in total";
        }
    }

    public string EmptyText => $"The storage holds no {Kind.PluralName.ToLowerInvariant()}. Add one with the button below.";

    /// <summary>The amounts to store, by sub type (materials at 0 are left out); set when the user applies.</summary>
    public IReadOnlyDictionary<int, int>? Result { get; private set; }

    /// <summary>The user applied the amounts; the view closes.</summary>
    public event EventHandler? Applied;

    partial void OnSearchTextChanged(string value) => EntriesView.Refresh();

    /// <summary>Picks a material and adds it with an amount of 1, or selects its row when it is listed already.</summary>
    [RelayCommand]
    private void Add()
    {
        if (_pickMaterial(Kind) is not { } material)
            return;

        if (Entries.FirstOrDefault(e => e.SubTypeId == material.SubTypeId) is { } existing)
        {
            if (existing.Quantity == 0)
                existing.Quantity = 1;
            SearchText = string.Empty;
            SelectedEntry = existing;
            return;
        }

        SearchText = string.Empty;
        SelectedEntry = AddEntry(material.SubTypeId, 1);
    }

    [ObservableProperty]
    private CraftingMaterialAmountViewModel? _selectedEntry;

    [RelayCommand]
    private void Remove(CraftingMaterialAmountViewModel? entry)
    {
        if (entry is null || !Entries.Remove(entry))
            return;

        NotifyTotals();
    }

    [RelayCommand]
    private void Apply()
    {
        Result = Entries
            .Where(e => e.Quantity > 0)
            .GroupBy(e => e.SubTypeId)
            .ToDictionary(g => g.Key, g => Math.Min(MaxQuantity, g.Sum(e => e.Quantity)));
        Applied?.Invoke(this, EventArgs.Empty);
    }

    private CraftingMaterialAmountViewModel AddEntry(int subTypeId, int quantity)
    {
        var material = _catalog.FindCraftingMaterial(Kind, subTypeId);
        var entry = new CraftingMaterialAmountViewModel(Kind, subTypeId, material, _iconProvider)
        {
            Quantity = Math.Clamp(quantity, 0, MaxQuantity)
        };
        entry.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CraftingMaterialAmountViewModel.Quantity))
                OnPropertyChanged(nameof(Summary));
        };

        Entries.Add(entry);
        NotifyTotals();
        return entry;
    }

    private void NotifyTotals()
    {
        OnPropertyChanged(nameof(Summary));
        OnPropertyChanged(nameof(HasEntries));
    }

    private bool Matches(CraftingMaterialAmountViewModel entry) =>
        SearchText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .All(w => entry.SearchText.Contains(w, StringComparison.CurrentCultureIgnoreCase));
}

/// <summary>One material of the material storage dialog and its amount.</summary>
public sealed partial class CraftingMaterialAmountViewModel(
    CraftingMaterialKind kind,
    int subTypeId,
    CraftingMaterial? material,
    IIconProvider iconProvider) : ObservableObject
{
    public CraftingMaterialKind Kind { get; } = kind;

    public int SubTypeId { get; } = subTypeId;

    /// <summary>The material in <c>base.db</c>; <c>null</c> when the database does not know it.</summary>
    public CraftingMaterial? Material { get; } = material;

    public string Name => Material?.Name ?? $"{Kind.DisplayName} #{SubTypeId}";

    public ImageSource? Icon => iconProvider.Get(Material?.IconPath);

    /// <summary>Second line, e.g. "Prefix · Melee"; empty for runes and glyphs.</summary>
    public string Details => Material is null
        ? "Not in the game database"
        : string.Join(" · ", new[] { Material.AffixType?.ToString(), Material.GroupName }.Where(p => !string.IsNullOrEmpty(p)));

    public bool HasDetails => Details.Length > 0;

    public string? ToolTip => string.IsNullOrWhiteSpace(Material?.Description) ? null : Material.Description;

    public string SearchText => $"{Name} {Material?.AffixType} {Material?.GroupName}";

    /// <summary>Amount in the storage; 0 removes the material when the dialog is applied.</summary>
    [ObservableProperty]
    private int _quantity;

    partial void OnQuantityChanged(int value)
    {
        var clamped = Math.Clamp(value, 0, CraftingMaterialStorageViewModel.MaxQuantity);
        if (clamped != value)
            Quantity = clamped;
    }
}

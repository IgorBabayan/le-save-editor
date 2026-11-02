using System.Collections.ObjectModel;
using System.Windows.Media;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// The crafting material picker: chooses a material of one kind (a rune, a glyph or an affix shard) to put into the
/// backpack, or the material of that kind an existing one becomes. The list is filtered by the search text (name and,
/// for shards, prefix / suffix and affix category).
/// </summary>
public sealed partial class CraftingMaterialPickerViewModel : ViewModelBase
{
    private readonly IItemBaseCatalog _catalog;
    private readonly IIconProvider _iconProvider;

    /// <param name="kind">Kind of material to pick.</param>
    /// <param name="current">The material being edited (selected in the list), or <c>null</c> for a new one.</param>
    public CraftingMaterialPickerViewModel(
        IItemBaseCatalog catalog,
        IIconProvider iconProvider,
        CraftingMaterialKind kind,
        CraftingMaterial? current)
    {
        _catalog = catalog;
        _iconProvider = iconProvider;
        Kind = kind;
        Current = current?.Kind == kind ? current : null;
        IsEditing = Current is not null;
        Refresh();
    }

    /// <summary>Kind of material the picker lists.</summary>
    public CraftingMaterialKind Kind { get; }

    /// <summary>The material being edited; <c>null</c> when a new one is picked.</summary>
    public CraftingMaterial? Current { get; }

    public bool IsEditing { get; }

    /// <summary>Window title, e.g. "Add rune" or "Edit affix shard".</summary>
    public string Title => $"{(IsEditing ? "Edit" : "Add")} {Kind.DisplayName.ToLowerInvariant()}";

    public string ApplyText => IsEditing ? "Apply" : "Add";

    /// <summary>Hint of the search box.</summary>
    public string SearchToolTip => Kind == CraftingMaterialKind.AffixShard
        ? "Search shards by affix name, prefix / suffix or affix category"
        : $"Search {Kind.PluralName.ToLowerInvariant()} by name";

    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>Materials of <see cref="Kind"/> that match <see cref="SearchText"/>.</summary>
    public ObservableCollection<CraftingMaterialOptionViewModel> Options { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    [NotifyPropertyChangedFor(nameof(SelectionDescription))]
    [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
    private CraftingMaterialOptionViewModel? _selectedOption;

    public bool HasSelection => SelectedOption is not null;

    public string SelectionDescription => SelectedOption?.Material.Description is { Length: > 0 } description
        ? description
        : SelectedOption is null ? "Pick a material from the list." : "No description.";

    public bool IsDatabaseAvailable => _catalog.IsAvailable;

    public bool HasOptions => Options.Count > 0;

    /// <summary>Text shown over an empty list.</summary>
    public string EmptyText => !_catalog.IsAvailable
        ? "The game database is missing. Generate it with the database button in the title bar."
        : string.IsNullOrWhiteSpace(SearchText)
            ? $"The game database has no {Kind.PluralName.ToLowerInvariant()}. Update it with the database button in the title bar."
            : $"No {Kind.PluralName.ToLowerInvariant()} match \"{SearchText.Trim()}\".";

    /// <summary>The picked material, set when the user applies.</summary>
    public CraftingMaterial? Result { get; private set; }

    /// <summary>The user applied the selection; the view closes.</summary>
    public event EventHandler? Applied;

    partial void OnSearchTextChanged(string value) => Refresh();

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private void Apply()
    {
        if (SelectedOption is not { } option)
            return;

        Result = option.Material;
        Applied?.Invoke(this, EventArgs.Empty);
    }

    private void Refresh()
    {
        var words = SearchText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var previous = SelectedOption?.Material ?? Current;

        Options.Clear();
        foreach (var material in _catalog.GetCraftingMaterials(Kind))
        {
            var option = new CraftingMaterialOptionViewModel(material, _iconProvider);
            if (words.All(w => option.SearchText.Contains(w, StringComparison.CurrentCultureIgnoreCase)))
                Options.Add(option);
        }

        SelectedOption = Options.FirstOrDefault(o => o.Material == previous);
        OnPropertyChanged(nameof(HasOptions));
        OnPropertyChanged(nameof(EmptyText));
    }
}

/// <summary>One row of the crafting material picker.</summary>
public sealed class CraftingMaterialOptionViewModel(CraftingMaterial material, IIconProvider iconProvider)
{
    public CraftingMaterial Material { get; } = material;

    public string Name => Material.Name;

    public ImageSource? Icon => iconProvider.Get(Material.IconPath);

    /// <summary>Second line, e.g. "Prefix · Melee", "Lv 15" or "Does not drop".</summary>
    public string Details
    {
        get
        {
            var parts = new List<string>();
            if (Material.AffixType is { } type)
                parts.Add(type.ToString());
            if (Material.GroupName is { Length: > 0 } group)
                parts.Add(group);
            if (Material.LevelRequirement > 0)
                parts.Add($"Lv {Material.LevelRequirement}");
            if (Material.CannotDrop)
                parts.Add("Does not drop");
            return string.Join(" · ", parts);
        }
    }

    public bool HasDetails => Details.Length > 0;

    public string? ToolTip => string.IsNullOrWhiteSpace(Material.Description) ? null : Material.Description;

    /// <summary>What the search matches: name, kind, prefix / suffix and affix category.</summary>
    public string SearchText => $"{Material.Name} {Material.Kind.DisplayName} {Material.AffixType} {Material.GroupName}";
}

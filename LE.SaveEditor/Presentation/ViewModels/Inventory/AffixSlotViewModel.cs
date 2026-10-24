using System.Collections.ObjectModel;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>Kind of an affix slot on a built item.</summary>
public enum AffixSlotKind
{
    Prefix,
    Suffix,
    SealedPrefix,
    SealedSuffix,
    Corrupted
}

/// <summary>
/// One affix slot in the item builder: empty, or an affix with a tier (1 … its tier count) and a slider per stat.
/// Choosing and removing go through the builder, which knows which affixes fit the item.
/// </summary>
public sealed partial class AffixSlotViewModel : ViewModelBase
{
    private readonly Action<AffixSlotViewModel> _choose;
    private readonly Action<AffixSlotViewModel> _remove;

    public AffixSlotViewModel(AffixSlotKind kind, Action<AffixSlotViewModel> choose, Action<AffixSlotViewModel> remove)
    {
        Kind = kind;
        _choose = choose;
        _remove = remove;
    }

    public AffixSlotKind Kind { get; }

    public string Label => Kind switch
    {
        AffixSlotKind.Prefix => "Prefix",
        AffixSlotKind.Suffix => "Suffix",
        AffixSlotKind.SealedPrefix => "Sealed prefix",
        AffixSlotKind.SealedSuffix => "Sealed suffix",
        _ => "Corruption"
    };

    /// <summary>Affix type the slot takes; <c>null</c> for the corruption slot (corrupted affixes only).</summary>
    public AffixType? AcceptedType => Kind switch
    {
        AffixSlotKind.Prefix or AffixSlotKind.SealedPrefix => AffixType.Prefix,
        AffixSlotKind.Suffix or AffixSlotKind.SealedSuffix => AffixType.Suffix,
        _ => null
    };

    public bool IsSealed => Kind is AffixSlotKind.SealedPrefix or AffixSlotKind.SealedSuffix;

    public bool IsCorrupted => Kind == AffixSlotKind.Corrupted;

    /// <summary><c>true</c> when <paramref name="affix"/> may go into this slot (type and corrupted / not corrupted).</summary>
    public bool Accepts(AffixDefinition affix) =>
        IsCorrupted
            ? affix.Category == AffixCategory.Corrupted
            : affix.Category != AffixCategory.Corrupted && affix.Type == AcceptedType;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAffix))]
    [NotifyPropertyChangedFor(nameof(Name))]
    [NotifyPropertyChangedFor(nameof(MaxTier))]
    [NotifyPropertyChangedFor(nameof(TierText))]
    [NotifyPropertyChangedFor(nameof(HasTiers))]
    private AffixDefinition? _affix;

    public bool HasAffix => Affix is not null;

    public string Name => Affix?.Name ?? string.Empty;

    public string AddText => $"Add {Label.ToLowerInvariant()}";

    /// <summary>The builder's affix picker is filling this slot (the item inspector shows the picker under it).</summary>
    [ObservableProperty]
    private bool _isPicking;

    public int MaxTier => Affix?.Tiers.Count ?? 1;

    /// <summary>The affix has more than one tier, so the tier slider is shown.</summary>
    public bool HasTiers => MaxTier > 1;

    /// <summary>Selected tier, 1-based like the game's "T5".</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TierText))]
    private int _tierNumber = 1;

    public string TierText => $"Tier {TierNumber}";

    /// <summary>Stats of the selected tier, one slider each.</summary>
    public ObservableCollection<ItemStatViewModel> Stats { get; } = [];

    /// <summary>Puts <paramref name="affix"/> in the slot at <paramref name="tier"/> (highest when <c>null</c>).</summary>
    public void Set(AffixDefinition affix, int? tier = null, IReadOnlyList<double>? values = null)
    {
        Affix = affix;
        _tierNumber = Math.Clamp(tier ?? affix.Tiers.Count, 1, affix.Tiers.Count);
        OnPropertyChanged(nameof(TierNumber));
        OnPropertyChanged(nameof(TierText));
        FillStats(values, null);
    }

    public void Clear()
    {
        Affix = null;
        Stats.Clear();
    }

    partial void OnTierNumberChanged(int value)
    {
        if (Affix is null)
            return;

        // Keep each slider at the same place in the new tier's range.
        FillStats(null, Stats.Select(s => s.Roll).ToList());
    }

    private void FillStats(IReadOnlyList<double>? values, IReadOnlyList<double>? rolls)
    {
        Stats.Clear();
        if (Affix is null)
            return;

        var tier = Affix.Tiers[Math.Clamp(TierNumber, 1, Affix.Tiers.Count) - 1];
        for (var i = 0; i < tier.Stats.Count; i++)
        {
            var stat = new ItemStatViewModel(tier.Stats[i], values is not null && i < values.Count ? values[i] : null);
            if (rolls is not null && i < rolls.Count)
                stat.Roll = rolls[i];
            Stats.Add(stat);
        }
    }

    /// <summary>The slot's content for <see cref="BuiltItem"/>, or <c>null</c> when it is empty.</summary>
    public BuiltAffix? ToBuiltAffix() =>
        Affix is null ? null : new BuiltAffix(Kind, Affix, TierNumber, Stats.Select(s => s.Value).ToList());

    [RelayCommand]
    private void Choose() => _choose(this);

    [RelayCommand]
    private void Remove() => _remove(this);
}

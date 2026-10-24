using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>One affix in the affix picker, grouped by its display category ("Melee", "Set", "Corrupted", ...).</summary>
public sealed class AffixOptionViewModel(AffixDefinition affix)
{
    public AffixDefinition Affix { get; } = affix;

    public string Name => Affix.Name;

    public string GroupName => Affix.GroupName;

    /// <summary>"Deft · T1–T8 · Lv 43".</summary>
    public string Details
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Affix.Title))
                parts.Add(Affix.Title);
            parts.Add(Affix.Tiers.Count > 1 ? $"T1–T{Affix.Tiers.Count}" : "T1");
            if (Affix.LevelRequirement > 0)
                parts.Add($"Lv {Affix.LevelRequirement}");
            return string.Join(" · ", parts);
        }
    }

    /// <summary>Stat ranges of the highest tier, e.g. "+(120–150)% Fire Resistance".</summary>
    public string TopTierText => string.Join("  ·  ", Affix.Tiers[^1].Stats.Select(ItemStatViewModel.FormatRange));
}

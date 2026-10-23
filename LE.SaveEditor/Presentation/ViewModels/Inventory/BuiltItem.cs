using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// An item made in the item builder: a base (or unique / set item) with a rarity, a value for each implicit
/// and, for uniques, a value for each unique modifier.
/// </summary>
/// <param name="ImplicitValues">One value per <see cref="ItemBase.Implicits"/>, in the same order.</param>
/// <param name="ModValues">One value per <see cref="ItemBase.Mods"/>, in the same order (empty for plain bases).</param>
/// <param name="Affixes">Crafted affixes (prefixes, suffixes, sealed and corruption).</param>
public sealed record BuiltItem(
    EquipmentSlot Slot,
    ItemRarity Rarity,
    ItemBase Base,
    IReadOnlyList<double> ImplicitValues,
    IReadOnlyList<double> ModValues,
    IReadOnlyList<BuiltAffix> Affixes)
{
    /// <summary>
    /// Implicit values as the game stores them: one byte per implicit, 0 = minimum, 255 = maximum.
    /// </summary>
    public IReadOnlyList<byte> ImplicitRolls =>
        Base.Implicits.Select((implicitStat, i) => ItemStatViewModel.ToRoll(implicitStat, ImplicitValues[i])).ToList();

    /// <summary>Unique modifier values as the game stores them: one byte per modifier, 0 = minimum, 255 = maximum.</summary>
    public IReadOnlyList<byte> ModRolls =>
        Base.Mods.Select((mod, i) => ItemStatViewModel.ToRoll(mod, ModValues[i])).ToList();
}

/// <summary>A crafted affix: the slot it is in, its tier (1-based) and a value for each of the tier's stats.</summary>
public sealed record BuiltAffix(AffixSlotKind Slot, AffixDefinition Affix, int Tier, IReadOnlyList<double> Values)
{
    public AffixTier TierData => Affix.Tiers[Math.Clamp(Tier, 1, Affix.Tiers.Count) - 1];

    /// <summary>Stat values as the game stores them: one byte per stat, 0 = tier minimum, 255 = tier maximum.</summary>
    public IReadOnlyList<byte> Rolls => TierData.Stats.Select((stat, i) => ItemStatViewModel.ToRoll(stat, Values[i])).ToList();

    /// <summary>Tooltip lines, e.g. "+32% Fire Resistance".</summary>
    public IEnumerable<string> Lines => TierData.Stats.Select((stat, i) => ItemStatViewModel.Format(stat, Values[i]));
}

using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// An item made in the item builder: a base (or unique / set item) with a rarity, a value for each implicit
/// and, for uniques, a value for each unique modifier.
/// </summary>
/// <param name="ImplicitValues">One value per <see cref="ItemBase.Implicits"/>, in the same order.</param>
/// <param name="ModValues">One value per <see cref="ItemBase.Mods"/>, in the same order (empty for plain bases).</param>
public sealed record BuiltItem(
    EquipmentSlot Slot,
    ItemRarity Rarity,
    ItemBase Base,
    IReadOnlyList<double> ImplicitValues,
    IReadOnlyList<double> ModValues)
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

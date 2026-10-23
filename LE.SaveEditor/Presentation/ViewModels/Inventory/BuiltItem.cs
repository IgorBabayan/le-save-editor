using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>An item made in the item builder: a base with a rarity and a value for each implicit.</summary>
/// <param name="ImplicitValues">One value per <see cref="ItemBase.Implicits"/>, in the same order.</param>
public sealed record BuiltItem(EquipmentSlot Slot, ItemRarity Rarity, ItemBase Base, IReadOnlyList<double> ImplicitValues)
{
    /// <summary>
    /// Implicit values as the game stores them: one byte per implicit, 0 = minimum, 255 = maximum.
    /// </summary>
    public IReadOnlyList<byte> ImplicitRolls =>
        Base.Implicits.Select((implicitStat, i) => ItemStatViewModel.ToRoll(implicitStat, ImplicitValues[i])).ToList();
}

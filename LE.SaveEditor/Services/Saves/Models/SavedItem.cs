namespace LE.SaveEditor.Services.Saves.Models;

/// <summary>
/// One entry of <c>savedItems</c> in a character save, kept as the game wrote it.
/// <see cref="Data"/> is decoded with <see cref="LE.SaveEditor.Services.Saves.ItemDataReader"/>.
/// </summary>
/// <param name="ContainerId">
/// Where the item is: 1 = backpack, 2–12 = equipment slots (helmet, body armour, weapon, off-hand, gloves, belt, boots,
/// left ring, right ring, amulet, relic), 29 = idols, 33+ = blessings.
/// </param>
/// <param name="X">Column inside the container (backpack and idol grid).</param>
/// <param name="Y">Row inside the container (backpack and idol grid).</param>
/// <param name="Data">The item bytes (<c>data</c>).</param>
public sealed record SavedItem(int ContainerId, int X, int Y, int Quantity, int FormatVersion, IReadOnlyList<byte> Data)
{
    /// <summary><see cref="ContainerId"/> of the character's backpack (the "Items" grid of the inventory).</summary>
    public const int BackpackContainerId = 1;
}

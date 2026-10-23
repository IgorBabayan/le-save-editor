namespace LE.SaveEditor.Services.Saves.Models;

/// <summary>Decoded bytes of an equipment item (see <see cref="LE.SaveEditor.Services.Saves.ItemDataReader"/>).</summary>
/// <param name="BaseTypeId">Item type (<c>baseTypeID</c>): helmet = 0, body armour = 1, wand = 10, ...</param>
/// <param name="SubTypeId">Base inside the item type (<c>subTypeID</c>).</param>
/// <param name="RarityCode">
/// Rarity as stored: 0 = normal, 1–2 = magic, 3–4 = rare (the number of unsealed affixes), 7 = unique, 8 = set,
/// 9 = legendary.
/// </param>
/// <param name="ImplicitRolls">One roll per implicit of the base, 0 = minimum, 255 = maximum.</param>
/// <param name="UniqueId">Unique / set item id (<c>uniqueID</c>), <c>null</c> for a plain base.</param>
/// <param name="UniqueRolls">Rolls of the unique modifiers, in the order of the unique's mods (empty for plain bases).</param>
/// <param name="ForgingPotential">Forging potential of a plain base, <c>null</c> for uniques.</param>
/// <param name="Affixes">Affixes in the order they are stored.</param>
public sealed record ItemData(
    int BaseTypeId,
    int SubTypeId,
    int RarityCode,
    IReadOnlyList<byte> ImplicitRolls,
    int? UniqueId,
    IReadOnlyList<byte> UniqueRolls,
    int? ForgingPotential,
    IReadOnlyList<ItemAffixData> Affixes)
{
    public bool IsUnique => UniqueId is not null;
}

/// <summary>One stored affix.</summary>
/// <param name="AffixId">The game's <c>affixId</c>.</param>
/// <param name="Tier">Tier as the game shows it, 1-based ("T5").</param>
/// <param name="Roll">Roll inside the tier, 0 = tier minimum, 255 = tier maximum.</param>
/// <param name="IsSealed">The affix is sealed (glyph of insight).</param>
public sealed record ItemAffixData(int AffixId, int Tier, byte Roll, bool IsSealed);

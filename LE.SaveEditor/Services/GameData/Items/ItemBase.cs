namespace LE.SaveEditor.Services.GameData.Items;

/// <summary>How an implicit modifies its property (the game's <c>ModType</c>).</summary>
public enum ImplicitModifierType
{
    Added = 0,
    Increased = 1,
    More = 2,
    Quotient = 3
}

/// <summary>
/// One implicit of an item base: a property rolled between <see cref="Min"/> and <see cref="Max"/>
/// (a roll of 0 gives <see cref="Min"/>, 255 gives <see cref="Max"/>).
/// </summary>
/// <param name="Property">Display name of the property with its tags, e.g. "Armor", "Fire Resistance", "Spell Damage".</param>
/// <param name="IsPercent">The value is a fraction shown as a percentage (0.12 → 12%).</param>
/// <param name="PercentScale">Multiplier from the stored fraction to the shown percentage: 100, or 10 for "tenth of value" properties (leech).</param>
/// <param name="ShowPlus">Positive values are shown with a leading "+".</param>
/// <param name="NameIncludesModifier">The name already says "increased" / "more" (e.g. "Increased Stun Chance").</param>
public sealed record ItemImplicit(
    string Property,
    ImplicitModifierType Type,
    double Min,
    double Max,
    bool IsPercent,
    double PercentScale = 100,
    bool ShowPlus = true,
    bool NameIncludesModifier = false);

/// <summary>An item base (sub type) of an item type, e.g. "Iron Cap" of "Helmet".</summary>
/// <param name="BaseTypeId">Item type id, the second byte of an item in a save (helmet = 0).</param>
/// <param name="SubTypeId">Base id inside its item type, the third byte of an item in a save.</param>
/// <param name="TypeName">Item type name, e.g. "Helmet".</param>
/// <param name="ClassRequirement">
/// Classes that can equip the base as bit flags of their ids (<c>1 &lt;&lt; (int)CharacterClass</c>: 1 Primalist, 2 Mage,
/// 4 Sentinel, 8 Acolyte, 16 Rogue); 0 = any class.
/// </param>
/// <param name="CannotDrop">The game no longer drops this base (legacy bases are kept in the data).</param>
/// <param name="IconPath">Icon path for <see cref="IIconProvider"/>, or <c>null</c> when none was found.</param>
public sealed record ItemBase(
    int BaseTypeId,
    int SubTypeId,
    string Name,
    string TypeName,
    int LevelRequirement,
    int ClassRequirement,
    bool CannotDrop,
    string? IconPath,
    IReadOnlyList<ItemImplicit> Implicits)
{
    public bool IsUsableBy(LE.SaveEditor.Services.Saves.Models.CharacterClass characterClass) =>
        ClassRequirement == 0 || (ClassRequirement & (1 << (int)characterClass)) != 0;
}

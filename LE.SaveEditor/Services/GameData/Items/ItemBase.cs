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
/// <param name="Template">
/// For a unique modifier described by its tooltip text: that text with <c>{value}</c> where the rolled number goes,
/// e.g. "{value}% Chance to Ignite on Hit with Fire Skills"; <c>null</c> to use <see cref="Property"/>.
/// </param>
/// <param name="TemplateMin">Number shown in <see cref="Template"/> at <see cref="Min"/>.</param>
/// <param name="TemplateMax">Number shown in <see cref="Template"/> at <see cref="Max"/>.</param>
public sealed record ItemImplicit(
    string Property,
    ImplicitModifierType Type,
    double Min,
    double Max,
    bool IsPercent,
    double PercentScale = 100,
    bool ShowPlus = true,
    bool NameIncludesModifier = false,
    string? Template = null,
    double TemplateMin = 0,
    double TemplateMax = 0);

/// <summary>
/// An item base (sub type) of an item type, e.g. "Iron Cap" of "Helmet", or a unique / set item
/// (then <see cref="UniqueId"/> is set and <see cref="UniqueMods"/> holds its own rolled modifiers).
/// </summary>
/// <param name="BaseTypeId">Item type id, the second byte of an item in a save (helmet = 0).</param>
/// <param name="SubTypeId">Base id inside its item type, the third byte of an item in a save.</param>
/// <param name="TypeName">Item type name, e.g. "Helmet".</param>
/// <param name="ClassRequirement">
/// Classes that can equip the base as bit flags of their ids (<c>1 &lt;&lt; (int)CharacterClass</c>: 1 Primalist, 2 Mage,
/// 4 Sentinel, 8 Acolyte, 16 Rogue); 0 = any class.
/// </param>
/// <param name="CannotDrop">The game no longer drops this base (legacy bases are kept in the data).</param>
/// <param name="IconPath">Icon path for <see cref="IIconProvider"/>, or <c>null</c> when none was found.</param>
/// <param name="Implicits">Implicits of the base (for a unique: of the base it is built on).</param>
/// <param name="UniqueId">Id of the unique / set item (<c>uniqueID</c>), <c>null</c> for a plain base.</param>
/// <param name="IsSet">The unique is a set item.</param>
/// <param name="BaseName">Name of the base a unique is built on (e.g. "Jewelled Circlet"); <c>null</c> for a plain base.</param>
/// <param name="UniqueMods">Modifiers of a unique / set item, empty for a plain base.</param>
/// <param name="UniqueEffects">Tooltip lines of a unique / set item that do not roll (e.g. "You are immune to Blind and Chill").</param>
/// <param name="LegendaryAffixSlots">How many of <paramref name="LegendaryAffixIds"/> the unique can carry at once.</param>
/// <param name="LegendaryAffixIds">
/// Affixes only this unique drops as a legendary (e.g. the Blood Rage affixes of Unsated Rage); empty for most uniques.
/// </param>
public sealed record ItemBase(
    int BaseTypeId,
    int SubTypeId,
    string Name,
    string TypeName,
    int LevelRequirement,
    int ClassRequirement,
    bool CannotDrop,
    string? IconPath,
    IReadOnlyList<ItemImplicit> Implicits,
    int? UniqueId = null,
    bool IsSet = false,
    string? BaseName = null,
    IReadOnlyList<ItemImplicit>? UniqueMods = null,
    IReadOnlyList<string>? UniqueEffects = null,
    IReadOnlyList<int>? LegendaryAffixIds = null,
    int LegendaryAffixSlots = 1)
{
    public bool IsUnique => UniqueId is not null;

    public IReadOnlyList<ItemImplicit> Mods => UniqueMods ?? [];

    public IReadOnlyList<string> Effects => UniqueEffects ?? [];

    public IReadOnlyList<int> LegendaryAffixes => LegendaryAffixIds ?? [];

    /// <summary>Unique affix slots in the item builder: none without own legendary affixes, else at least one.</summary>
    public int LegendaryAffixCount => LegendaryAffixes.Count == 0 ? 0 : Math.Clamp(LegendaryAffixSlots, 1, LegendaryAffixes.Count);

    public bool IsUsableBy(LE.SaveEditor.Services.Saves.Models.CharacterClass characterClass) =>
        ClassRequirement == 0 || (ClassRequirement & (1 << (int)characterClass)) != 0;
}

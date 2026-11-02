namespace LE.SaveEditor.Services.GameData.Items;

/// <summary>
/// Kind of a stackable material the character can carry in the backpack and the stash keeps an amount of; the value is
/// the item type (<c>baseTypeID</c>) the game stores it with.
/// </summary>
public enum CraftingMaterialKind
{
    /// <summary>Affix shard (<c>baseTypeID</c> 101); its sub type is the <c>affixId</c> of the affix it adds.</summary>
    AffixShard = 101,

    /// <summary>Rune (<c>baseTypeID</c> 102, "Crafting Modifier Item"): Rune of Removal, of Shattering, ...</summary>
    Rune = 102,

    /// <summary>Glyph (<c>baseTypeID</c> 103, "Crafting Support Item"): Glyph of Hope, of Chaos, ...</summary>
    Glyph = 103,

    /// <summary>
    /// Resource (<c>baseTypeID</c> 104, "Key Items"): dungeon and arena keys, the Harbinger Eye, charms, primordial
    /// materials, Temporal Keystones, Timeglass Fragments, ... — what the game's "Resources" inventory tab shows.
    /// </summary>
    KeyItem = 104
}

/// <summary>
/// One crafting material from the <c>itemTypes</c> node of <c>base.db</c>: a rune, a glyph, an affix shard or a resource
/// (key item).
/// Crafting materials take one backpack cell and have no rolls; the item type and the sub type are all the game stores.
/// </summary>
/// <param name="Kind">Rune, glyph or affix shard.</param>
/// <param name="SubTypeId">The game's <c>subTypeID</c>; for an affix shard the <c>affixId</c> of its affix.</param>
/// <param name="Name">Shown name, e.g. "Rune of Removal", "Glyph of Hope" or, for a shard, the affix ("Void Penetration").</param>
/// <param name="Description">What the material does, without the game's rich-text tags; empty when the data has none.</param>
/// <param name="LevelRequirement">Level the material starts to drop at.</param>
/// <param name="CannotDrop">The game does not drop the material (it is made by crafting or is a legacy entry).</param>
/// <param name="IconPath">Icon path for <see cref="IIconProvider"/>, or <c>null</c> when none was found.</param>
/// <param name="AffixType">Prefix or suffix of a shard's affix; <c>null</c> for runes, glyphs and unknown affixes.</param>
/// <param name="GroupName">Display category of a shard's affix ("Melee", "Idol", ...); <c>null</c> for runes and glyphs.</param>
public sealed record CraftingMaterial(
    CraftingMaterialKind Kind,
    int SubTypeId,
    string Name,
    string Description,
    int LevelRequirement,
    bool CannotDrop,
    string? IconPath,
    AffixType? AffixType = null,
    string? GroupName = null)
{
    /// <summary>Item type (<c>baseTypeID</c>) of the material.</summary>
    public int BaseTypeId => (int)Kind;

    /// <summary>Kind and name for messages, e.g. "Void Penetration shard" or "Rune of Removal".</summary>
    public string FullName => Kind == CraftingMaterialKind.AffixShard ? $"{Name} shard" : Name;
}

public static class CraftingMaterialKindExtensions
{
    /// <summary>The material kind stored with item type <paramref name="baseTypeId"/>; <c>null</c> for any other item.</summary>
    public static CraftingMaterialKind? FromBaseTypeId(int baseTypeId) =>
        Enum.IsDefined(typeof(CraftingMaterialKind), baseTypeId) ? (CraftingMaterialKind)baseTypeId : null;

    extension(CraftingMaterialKind kind)
    {
        /// <summary>Name of one material of the kind ("Rune", "Glyph", "Affix shard", "Resource").</summary>
        public string DisplayName => kind switch
        {
            CraftingMaterialKind.AffixShard => "Affix shard",
            CraftingMaterialKind.KeyItem => "Resource",
            _ => kind.ToString()
        };

        /// <summary>Name of several materials of the kind ("Runes", "Glyphs", "Affix shards", "Resources").</summary>
        public string PluralName => $"{kind.DisplayName}s";
    }
}

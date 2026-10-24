namespace LE.SaveEditor.Services.GameData.Items;

/// <summary>Prefix or suffix (the game's affix <c>type</c>: 0 / 1).</summary>
public enum AffixType
{
    Prefix = 0,
    Suffix = 1
}

/// <summary>Where an affix comes from (the game's <c>specialAffixType</c>; idol affixes are <see cref="Normal"/>).</summary>
public enum AffixCategory
{
    Normal = 0,
    Experimental = 1,
    Personal = 2,
    Set = 3,
    Corrupted = 6,
    Variant = 7
}

/// <summary>One tier of an affix: a roll range for each of its stats.</summary>
/// <param name="Number">Tier as the game shows it, 1-based ("T5").</param>
/// <param name="Stats">One modifier per affix stat with this tier's range.</param>
public sealed record AffixTier(int Number, IReadOnlyList<ItemImplicit> Stats);

/// <summary>A craftable equipment or idol affix from the <c>affixes</c> node.</summary>
/// <param name="Id">The game's <c>affixId</c> (stored in item saves).</param>
/// <param name="Name">Shown name, e.g. "Increased Melee Attack Speed".</param>
/// <param name="Title">Item name part, e.g. "Deft" or "of Embers".</param>
/// <param name="GroupName">Display category, e.g. "Melee", "Mage", "Set", "Corrupted".</param>
/// <param name="ClassName">Class that the affix belongs to ("Mage"), or <c>null</c> for any class.</param>
/// <param name="CanRollOn">Item type ids (<c>baseTypeID</c>) the affix can be crafted on.</param>
public sealed record AffixDefinition(
    int Id,
    string Name,
    string? Title,
    AffixType Type,
    AffixCategory Category,
    string GroupName,
    string? ClassName,
    int LevelRequirement,
    IReadOnlySet<int> CanRollOn,
    IReadOnlyList<AffixTier> Tiers)
{
    public bool CanRollOnType(int baseTypeId) => CanRollOn.Contains(baseTypeId);

    /// <summary>
    /// <c>uniqueID</c>s of the uniques that alone can have this affix (their own legendary affixes, e.g. the Blood Rage
    /// affixes of Unsated Rage); empty when any item of a fitting type can.
    /// </summary>
    public IReadOnlySet<int> OnlyOnUniques { get; init; } = new HashSet<int>();

    /// <summary>Whether the affix may go on <paramref name="item"/> as far as unique-only affixes are concerned.</summary>
    public bool IsAllowedOn(ItemBase item) =>
        OnlyOnUniques.Count == 0 || (item.UniqueId is { } uniqueId && OnlyOnUniques.Contains(uniqueId));
}

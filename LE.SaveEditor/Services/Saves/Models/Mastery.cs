namespace LE.SaveEditor.Services.Saves.Models;

/// <summary>
/// Mastery (sub-class) of a character. In the save file <c>chosenMastery</c> is only an index
/// within the base class (0 = none, 1..3 = mastery); use <see cref="CharacterClassCatalog.GetMastery"/> to resolve it.
/// </summary>
public enum Mastery
{
    /// <summary>The save holds a class / mastery index this editor does not know.</summary>
    Unknown = -1,

    /// <summary>No mastery chosen yet (<c>chosenMastery</c> = 0).</summary>
    None = 0,

    // Primalist
    Beastmaster,
    Shaman,
    Druid,

    // Mage
    Sorcerer,
    Spellblade,
    Runemaster,

    // Sentinel
    VoidKnight,
    ForgeGuard,
    Paladin,

    // Acolyte
    Necromancer,
    Lich,
    Warlock,

    // Rogue
    Bladedancer,
    Marksman,
    Falconer
}

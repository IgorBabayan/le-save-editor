namespace LE.SaveEditor.Services.Saves.Models;

/// <summary>Known Last Epoch classes and their masteries, in the order the game stores them.</summary>
public static class CharacterClassCatalog
{
    private static readonly IReadOnlyDictionary<CharacterClass, Mastery[]> Masteries =
        new Dictionary<CharacterClass, Mastery[]>
        {
            [CharacterClass.Primalist] = [Mastery.Beastmaster, Mastery.Shaman, Mastery.Druid],
            [CharacterClass.Mage] = [Mastery.Sorcerer, Mastery.Spellblade, Mastery.Runemaster],
            [CharacterClass.Sentinel] = [Mastery.VoidKnight, Mastery.ForgeGuard, Mastery.Paladin],
            [CharacterClass.Acolyte] = [Mastery.Necromancer, Mastery.Lich, Mastery.Warlock],
            [CharacterClass.Rogue] = [Mastery.Bladedancer, Mastery.Marksman, Mastery.Falconer]
        };

    /// <summary>Masteries available to <paramref name="characterClass"/> (empty for an unknown class).</summary>
    public static IReadOnlyList<Mastery> GetMasteries(CharacterClass characterClass) =>
        Masteries.TryGetValue(characterClass, out var masteries) ? masteries : [];

    /// <summary>Resolves the raw <c>chosenMastery</c> index (0 = none, 1..3) of a class to a <see cref="Mastery"/>.</summary>
    public static Mastery GetMastery(CharacterClass characterClass, int chosenMastery)
    {
        if (chosenMastery == 0)
            return Mastery.None;

        var masteries = GetMasteries(characterClass);
        return chosenMastery >= 1 && chosenMastery <= masteries.Count
            ? masteries[chosenMastery - 1]
            : Mastery.Unknown;
    }

    /// <summary>Inverse of <see cref="GetMastery"/>: the index to write back into <c>chosenMastery</c>, or -1 if the mastery does not belong to the class.</summary>
    public static int GetMasteryIndex(CharacterClass characterClass, Mastery mastery)
    {
        if (mastery == Mastery.None)
            return 0;

        var index = GetMasteries(characterClass).ToList().IndexOf(mastery);
        return index < 0 ? -1 : index + 1;
    }

    public static bool IsKnown(CharacterClass characterClass) => Masteries.ContainsKey(characterClass);

    public static string GetDisplayName(CharacterClass characterClass) =>
        IsKnown(characterClass) ? characterClass.ToString() : $"Unknown ({(int)characterClass})";

    public static string GetDisplayName(Mastery mastery) => mastery switch
    {
        Mastery.None => "No mastery",
        Mastery.Unknown => "Unknown",
        Mastery.VoidKnight => "Void Knight",
        Mastery.ForgeGuard => "Forge Guard",
        _ => mastery.ToString()
    };
}

namespace LE.SaveEditor.Services.GameData.Factions;

/// <summary>
/// A faction the "Factions" section edits: its key in the stash's <c>factions</c> object, its name and its highest rank.
/// </summary>
/// <param name="Id">Key of the faction in <c>factions</c> (<c>{"0": {"rank": 1, "reputation": 8, ...}}</c>).</param>
/// <param name="Name">Name the game shows.</param>
/// <param name="MaxRank">Highest rank of the faction.</param>
public sealed record Faction(string Id, string Name, int MaxRank);

/// <summary>
/// The factions the "Factions" section lists, in display order. A faction is only editable when the stash already has an
/// entry for it (a character of the stash has unlocked it in game): the editor never creates faction entries.
/// </summary>
public static class FactionCatalog
{
    /// <summary>Circle of Fortune: key <c>"0"</c> (its entry holds the prophecies and the favor).</summary>
    public static readonly Faction CircleOfFortune = new("0", "Circle of Fortune", 12);

    /// <summary>Forgotten Knights.</summary>
    public static readonly Faction ForgottenKnights = new("2", "Forgotten Knights", 10);

    /// <summary>The Woven.</summary>
    public static readonly Faction TheWoven = new("3", "The Woven", 10);

    /// <summary>Every faction of the section, in display order.</summary>
    public static IReadOnlyList<Faction> All { get; } = [CircleOfFortune, ForgottenKnights, TheWoven];
}

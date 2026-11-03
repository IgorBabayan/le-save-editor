namespace LE.SaveEditor.Services.GameData.Waypoints;

/// <summary>A waypoint the game can unlock: the scene it stands in and where it belongs on the map.</summary>
/// <param name="Scene">Scene name the save stores in <c>unlockedWaypointScenes</c> (e.g. <c>Z32</c>).</param>
/// <param name="Name">Zone name the game shows.</param>
/// <param name="Chapter">Group the waypoint is listed under.</param>
public sealed record Waypoint(string Scene, string Name, string Chapter);

/// <summary>
/// Every waypoint of the campaign and the End of Time, in the order of the game's map. A character save lists the
/// scenes of the waypoints it has unlocked in <c>unlockedWaypointScenes</c>; scenes the save names that are not in this
/// list are kept as they are.
/// </summary>
public static class WaypointCatalog
{
    /// <summary>Group of the waypoints outside the campaign chapters (End of Time, Monolith, arena, dungeons).</summary>
    public const string OtherGroup = "End of Time, Monolith, Arena and Dungeons";

    private const string Chapter1 = "Chapter 1";
    private const string Chapter2 = "Chapter 2";
    private const string Chapter3 = "Chapter 3";
    private const string Chapter4 = "Chapter 4";
    private const string Chapter5 = "Chapter 5";
    private const string Chapter6 = "Chapter 6";
    private const string Chapter7 = "Chapter 7";
    private const string Chapter8 = "Chapter 8";
    private const string Chapter9 = "Chapter 9";
    private const string Chapter10 = "Chapter 10";

    public static IReadOnlyList<Waypoint> All { get; } =
    [
        new("Z22", "The Old Road", Chapter1),
        new("Z32", "The Keepers' Camp", Chapter1),
        new("Z52", "The Fortress Walls", Chapter1),
        new("Z72", "The Northern Road", Chapter1),
        new("Z82", "Ulatri Highlands", Chapter1),
        new("Z92", "The Osprix Warcamp", Chapter1),
        new("Z102", "The Summit", Chapter1),

        new("A06", "The Crumbling Ruins", Chapter2),
        new("A10", "Last Refuge Outskirts", Chapter2),
        new("A30", "The Council Chambers", Chapter2),
        new("A45", "Pannion's Study", Chapter2),
        new("A50", "The Upper District", Chapter2),
        new("A60", "The Precipice", Chapter2),
        new("A60TR", "The Ancient Cavern", Chapter2),
        new("A70", "The Armory", Chapter2),
        new("A90", "The Sheltered Wood", Chapter2),

        new("B10", "The Surface", Chapter3),
        new("B20", "The Forsaken Trail", Chapter3),
        new("B25", "Cultist Camp", Chapter3),
        new("B30", "The Ruins of Welryn", Chapter3),
        new("B7S10", "Welryn Docks", Chapter3),
        new("B33", "Welryn Undercity", Chapter3),
        new("B40", "The Shattered Valley", Chapter3),
        new("B40TR", "The Ancient Forest", Chapter3),
        new("B1S40", "The Lost Refuge", Chapter3),
        new("B50", "The Courtyard", Chapter3),
        new("B60", "The Temple of Eterra", Chapter3),
        new("B80", "The Lotus Halls", Chapter3),

        new("C10", "The Outcast Camp", Chapter4),
        new("C20", "Welryn Outskirts", Chapter4),
        new("C30", "Imperial Welryn", Chapter4),
        new("C35", "The Soul Wardens' Road", Chapter4),
        new("C2S10", "The Ruins of Etendell", Chapter4),
        new("C5S10", "The Felled Wood", Chapter4),
        new("C40", "The Risen Lake", Chapter4),
        new("C40TR", "The Corrupted Lake", Chapter4),
        new("C50", "The Fallen Tower", Chapter4),
        new("C60", "Imperial Thetima", Chapter4),
        new("C70", "The Imperial Dreadnought", Chapter4),

        new("D05", "The Shining Cove", Chapter5),
        new("D05TR", "The Ruined Coast", Chapter5),
        new("D20", "The Wraith Dunes", Chapter5),
        new("D30", "Maj'elka", Chapter5),
        new("D40", "The Oracle's Abode", Chapter5),
        new("D60", "Titan's Canyon", Chapter5),
        new("D80", "The Maj'elka Waystation", Chapter5),

        new("E10", "The Desert Waystation", Chapter6),
        new("E20", "The Rust Lands", Chapter6),
        new("E20TR", "The Verdant Lakes", Chapter6),
        new("E30", "The Lower Sewers", Chapter6),
        new("E40", "The Barren Aqueduct", Chapter6),
        new("E50", "Necropolis of the Deep", Chapter6),
        new("E60", "Yulia's Haven", Chapter6),
        new("E80", "The Citadel Sewers", Chapter6),
        new("E90", "The Immortal Summit", Chapter6),

        new("F10", "The Gates of Solarum", Chapter7),
        new("F40", "Heoborea", Chapter7),
        new("F50", "The Heoborean Forest", Chapter7),
        new("F70", "The Wengari Fortress", Chapter7),
        new("F80", "The Tundra", Chapter7),
        new("F1S10", "The Ice Caverns", Chapter7),
        new("F90", "The Temple of Heorot", Chapter7),
        new("F5S10", "Kolheim Pass", Chapter7),
        new("F100", "Farwood", Chapter7),
        new("F110", "The Frozen Roots", Chapter7),

        new("G40", "Deep Harbor", Chapter8),
        new("G60", "Lake Liath", Chapter8),
        new("G70", "Liath's Road", Chapter8),
        new("G80", "Thetima", Chapter8),
        new("G90", "Lagon's Isle", Chapter8),
        new("G1S10", "Etendell", Chapter8),
        new("G2S10", "Liath's Tower", Chapter8),
        new("G93", "The Strand of Storms", Chapter8),
        new("G96", "The Coral Pools", Chapter8),
        new("G110", "The Temple of Lagon", Chapter8),

        new("H10", "Soreth'ka", Chapter9),
        new("H40", "The Radiant Dunes", Chapter9),
        new("H50", "Maj'elka Upper District", Chapter9),
        new("H70", "Maj'elka Slums", Chapter9),
        new("H80", "The Oasis", Chapter9),
        new("Observatory", "The Observatory", Chapter9),
        new("Bazaar", "The Bazaar", Chapter9),
        new("H100", "The Aerie", Chapter9),
        new("H110", "The Temple Rooftops", Chapter9),
        new("H120", "The Upper Temple", Chapter9),

        new("I10", "The Ancient Oasis", Chapter10),
        new("I20", "The Crystal Cave", Chapter10),
        new("I40", "The Mountain Ascent", Chapter10),
        new("I50", "The Draal Hive", Chapter10),
        new("I70", "The Silent Coast", Chapter10),
        new("I80", "The Garden", Chapter10),
        new("I100", "The Dragon Grotto", Chapter10),
        new("I120", "The Temple in the Sky", Chapter10),
        new("I130", "Eterra's Arboretum", Chapter10),
        new("I140", "The Halls of Preservation", Chapter10),

        new("EoT", "The End of Time", OtherGroup),
        new("MonolithHub", "Monolith of Fate", OtherGroup),
        new("ArenaLobby", "Champions' Gate", OtherGroup),
        new("Arena_1_Forest", "The Arena", OtherGroup),
        new("Dun1Q10", "Temporal Sanctum", OtherGroup),
        new("Dun2Q10", "Lightless Arbor", OtherGroup),
        new("Dun3Q10", "Soulfire Bastion", OtherGroup)
    ];

    private static readonly Dictionary<string, Waypoint> ByScene = All.ToDictionary(w => w.Scene, StringComparer.Ordinal);

    /// <summary>The waypoint standing in <paramref name="scene"/>; <c>null</c> for a scene the editor does not know.</summary>
    public static Waypoint? Find(string scene) => ByScene.GetValueOrDefault(scene);
}

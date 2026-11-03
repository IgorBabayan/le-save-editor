using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Services.GameData.Items;

/// <summary>
/// Which slots of the idol grid a character has unlocked. The save has no field for it: the game opens idol slots as
/// rewards of certain quests (<c>idolUnlockReward</c> in the game's quest data), so the unlocked slots follow from the
/// character's completed quests in <c>savedQuests</c>. Every such quest gives one unlock; each unlock opens the next
/// group of cells of <see cref="Stages"/>.
/// </summary>
public static class IdolSlotUnlocks
{
    /// <summary>
    /// Quests that reward an idol unlock (<c>questID</c> → <c>questStepID</c> the quest has when it is completed),
    /// from the game's quest data.
    /// </summary>
    public static IReadOnlyDictionary<int, int> RewardQuests { get; } = new Dictionary<int, int>
    {
        [1] = 4,      // The Void Assault
        [20] = 106,   // The Admiral's Dreadnought
        [35] = 202,   // An Ancient Hunt
        [36] = 209,   // The Sapphire Tablet
        [37] = 217,   // The Corrupted Lake
        [45] = 261,   // The Lance of Heorot
        [46] = 266,   // Liberating the Nomads
        [47] = 270,   // A Heoborean Cure
        [58] = 339,   // Liath's Tower
        [117] = 601,  // Arjani, the Ruby Commander
        [119] = 615,  // Desert Treasure
        [121] = 626,  // Harton's Idol
        [122] = 632,  // Too Greedily, Too Deep
        [158] = 853,  // Temporal Sanctum
        [159] = 855,  // Lightless Arbor
        [160] = 857   // Soulfire Bastion
    };

    /// <summary>
    /// Cells (column, row counted from the top) each unlock opens, in order. The grid opens from the bottom up: after
    /// four unlocks the 3 × 4 block under the top row (without the centre) is open, as the game shows it; then the
    /// left column, the right column and the top row. Unlocks past the last stage open nothing more.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<(int Column, int Row)>> Stages { get; } =
    [
        [(1, 4), (2, 4), (3, 4)],
        [(1, 3), (2, 3), (3, 3)],
        [(1, 2), (3, 2)],
        [(1, 1), (2, 1), (3, 1)],
        [(0, 3), (0, 2), (0, 1)],
        [(4, 3), (4, 2), (4, 1)],
        [(1, 0), (2, 0), (3, 0)]
    ];

    /// <summary>Every idol slot of the grid (the 5 × 5 grid without its corners and centre).</summary>
    public static IReadOnlySet<(int Column, int Row)> AllSlots { get; } =
        Stages.SelectMany(s => s).ToHashSet();

    /// <summary>Number of idol unlocks earned: completed quests of <see cref="RewardQuests"/>.</summary>
    public static int CountUnlocks(IEnumerable<SavedQuest> quests) =>
        quests.Where(q => RewardQuests.TryGetValue(q.QuestId, out var completedStep) && q.QuestStepId == completedStep)
              .Select(q => q.QuestId)
              .Distinct()
              .Count();

    /// <summary>Slots open after <paramref name="unlocks"/> unlocks.</summary>
    public static IReadOnlySet<(int Column, int Row)> UnlockedSlots(int unlocks) =>
        Stages.Take(Math.Max(0, unlocks)).SelectMany(s => s).ToHashSet();

    /// <summary>Slots still locked after <paramref name="unlocks"/> unlocks.</summary>
    public static IReadOnlySet<(int Column, int Row)> LockedSlots(int unlocks)
    {
        var unlocked = UnlockedSlots(unlocks);
        return AllSlots.Where(c => !unlocked.Contains(c)).ToHashSet();
    }
}

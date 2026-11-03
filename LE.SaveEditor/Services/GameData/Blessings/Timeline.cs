using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Services.GameData.Blessings;

/// <summary>A blessing a timeline grants: a base of item type <see cref="Timeline.BlessingBaseTypeId"/>.</summary>
/// <param name="Base">The blessing with its implicits (usually one rolled stat).</param>
/// <param name="IsGrand">Granted by the empowered (level 100) difficulty of the timeline ("Grand …").</param>
public sealed record TimelineBlessing(ItemBase Base, bool IsGrand);

/// <summary>
/// A Monolith timeline (an echo the character completes for a blessing) and the blessings it can grant, read from the
/// <c>Timelines</c> table of <c>base.db</c>. A character keeps the blessing it picked for a timeline in
/// <c>savedItems</c>, in container <see cref="ContainerId"/> (33 = Fall of the Outcasts … 42 = Spirits of Fire).
/// </summary>
/// <param name="TimelineId">The game's <c>timelineID</c> (1-based).</param>
/// <param name="Name">E.g. "Fall of the Outcasts".</param>
/// <param name="Level">Area level of the normal difficulty.</param>
/// <param name="Blessings">Normal blessings first, then the Grand ones, each in the order of the game data.</param>
public sealed record Timeline(int TimelineId, string Name, int Level, IReadOnlyList<TimelineBlessing> Blessings)
{
    /// <summary>Item type (<c>baseTypeID</c>) of blessings.</summary>
    public const int BlessingBaseTypeId = 34;

    /// <summary><c>savedItems</c> container of the blessing of timeline 1; timeline n uses 32 + n.</summary>
    public const int FirstBlessingContainerId = 33;

    /// <summary><c>savedItems</c> container of this timeline's blessing.</summary>
    public int ContainerId => ContainerIdOf(TimelineId);

    public static int ContainerIdOf(int timelineId) => FirstBlessingContainerId - 1 + timelineId;
}

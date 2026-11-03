namespace LE.SaveEditor.Services.GameData.Items;

/// <summary>
/// Shape of the idol grid: the cells that can never hold an idol and the refracted cells (slots that modify the effects
/// of the idols in them). Cells are (column, row counted from the top) of the 5 × 5 grid.
/// </summary>
/// <param name="Blocked">Cells that take no idol.</param>
/// <param name="Refracted">Refracted cells (always usable).</param>
public sealed record IdolGridShape(IReadOnlySet<(int Column, int Row)> Blocked, IReadOnlySet<(int Column, int Row)> Refracted)
{
    /// <summary>Cells that can hold an idol: neither blocked nor outside the grid.</summary>
    public int SlotCount => IdolAltarLayouts.GridSize * IdolAltarLayouts.GridSize - Blocked.Count;
}

/// <summary>
/// The idol grid layout of each idol altar (<c>baseTypeID</c> 41). An equipped altar replaces the standard grid (5 × 5
/// without its corners and centre, opened by quests, see <see cref="IdolSlotUnlocks"/>) with its own shape, in which
/// every slot is open and some are refracted. The game data has no layouts; they are the ones maxroll's planner uses.
/// </summary>
public static class IdolAltarLayouts
{
    /// <summary>Side length of the idol grid.</summary>
    public const int GridSize = 5;

    private const char BlockedCell = 'X';
    private const char RefractedCell = 'R';

    /// <summary>
    /// Layout of each altar by its <c>subTypeID</c>, row by row from the top: <c>X</c> = blocked, <c>R</c> = refracted,
    /// a space = an ordinary slot.
    /// </summary>
    private static readonly IReadOnlyDictionary<int, (string Name, string[] Rows)> Layouts =
        new Dictionary<int, (string Name, string[] Rows)>
        {
            [0] = ("Twisted Altar", ["XR   ", "  XX ", " RXR ", " XX  ", "   RX"]),
            [1] = ("Jagged Altar", ["X   X", "R   R", "X   X", "R   R", "X   X"]),
            [2] = ("Skyward Altar", ["XXRXX", "X   X", "R   R", "  R  ", "  X  "]),
            [3] = ("Spire Altar", ["RXX  ", "  X  ", "  R  ", "  X  ", "  XXR"]),
            [4] = ("Carcinised Altar", ["  X  ", " XXX ", " R R ", "X   X", "     "]),
            [5] = ("Visage Altar", ["X   X", "RR RR", " X X ", "     ", "X   X"]),
            [6] = ("Lunar Altar", ["X    ", "    X", "RRRXX", "    X", "X    "]),
            [7] = ("Ocular Altar", ["R   R", "  X  ", " XXX ", "  X  ", "R   R"]),
            [8] = ("Archaic Altar", ["X R X", "     ", "  X  ", "     ", "X R X"]),
            [9] = ("Impervious Altar", ["X   X", "X X X", " RRR ", " RRR ", "     "]),
            [10] = ("Prophesied Altar", ["X   X", "     ", "  R  ", "     ", "X   X"]),
            [11] = ("Pyramidal Altar", ["XX XX", "X R X", "  R  ", " R R ", "R   R"]),
            [12] = ("Auric Altar", ["  R  ", "  X  ", "X R X", "  X  ", "  R  "])
        };

    private static readonly Dictionary<int, IdolGridShape> Shapes = Layouts.ToDictionary(p => p.Key, p => Parse(p.Value.Rows));

    /// <summary>
    /// The standard idol grid without an altar: every cell but the four corners and the centre (whether a slot is open
    /// depends on the quests the character completed).
    /// </summary>
    public static IdolGridShape Standard { get; } = new(
        new HashSet<(int, int)>
        {
            (0, 0), (GridSize - 1, 0), (0, GridSize - 1), (GridSize - 1, GridSize - 1), (GridSize / 2, GridSize / 2)
        },
        new HashSet<(int, int)>());

    /// <summary>
    /// The grid an altar of <paramref name="subTypeId"/> gives. An altar this list does not know (added to the game
    /// later) gets the standard shape with every slot open and none refracted.
    /// </summary>
    public static IdolGridShape ForAltar(int subTypeId) => Shapes.GetValueOrDefault(subTypeId) ?? Standard;

    /// <summary>Whether the layout of the altar with <paramref name="subTypeId"/> is known.</summary>
    public static bool IsKnown(int subTypeId) => Shapes.ContainsKey(subTypeId);

    private static IdolGridShape Parse(string[] rows)
    {
        var blocked = new HashSet<(int, int)>();
        var refracted = new HashSet<(int, int)>();
        for (var row = 0; row < GridSize; row++)
        for (var column = 0; column < GridSize; column++)
        {
            var cell = column < rows[row].Length ? rows[row][column] : ' ';
            if (cell == BlockedCell)
                blocked.Add((column, row));
            else if (cell == RefractedCell)
                refracted.Add((column, row));
        }

        return new IdolGridShape(blocked, refracted);
    }
}

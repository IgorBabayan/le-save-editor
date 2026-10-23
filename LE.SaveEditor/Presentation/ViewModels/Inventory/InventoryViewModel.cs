using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// "Inventory" tab of one character: equipped items, the idol grid and crafting materials on top,
/// followed by the "Items", "Resources" and "Woven echoes" sections.
/// Nothing is parsed from the save yet, so every slot and section is empty for now.
/// </summary>
public sealed class InventoryViewModel(CharacterSave character) : ViewModelBase
{
    /// <summary>Side length of the idol grid.</summary>
    public const int IdolGridSize = 5;

    public CharacterSave Character { get; } = character;

    /// <summary>Cells of the idol grid, row by row (<see cref="IdolGridSize"/> × <see cref="IdolGridSize"/>).</summary>
    public IReadOnlyList<IdolCellViewModel> IdolCells { get; } = CreateIdolCells();

    /// <summary>Totals shown in the "Crafting materials" box.</summary>
    public IReadOnlyList<CraftingMaterialViewModel> CraftingMaterials { get; } =
    [
        new("Shards"),
        new("Glyphs"),
        new("Runes")
    ];

    /// <summary>The idol grid has a blocked plus shape in its middle, as in the game.</summary>
    private static IReadOnlyList<IdolCellViewModel> CreateIdolCells()
    {
        const int center = IdolGridSize / 2;
        var cells = new List<IdolCellViewModel>(IdolGridSize * IdolGridSize);

        for (var row = 0; row < IdolGridSize; row++)
        for (var column = 0; column < IdolGridSize; column++)
        {
            var isBlocked = (row == center && Math.Abs(column - center) <= 1)
                            || (column == center && Math.Abs(row - center) <= 1);
            cells.Add(new IdolCellViewModel(row, column, isBlocked));
        }

        return cells;
    }
}

/// <summary>One cell of the idol grid; <see cref="IsBlocked"/> cells can never hold an idol.</summary>
public sealed record IdolCellViewModel(int Row, int Column, bool IsBlocked);

/// <summary>A crafting material total; <see cref="Amount"/> is <c>null</c> until it is read from the save.</summary>
public sealed record CraftingMaterialViewModel(string Name, int? Amount = null);

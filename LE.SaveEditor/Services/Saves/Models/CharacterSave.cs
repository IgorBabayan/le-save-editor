namespace LE.SaveEditor.Services.Saves.Models;

/// <summary>Summary of one <c>1CHARACTERSLOT_*</c> save file.</summary>
public sealed record CharacterSave
{
    /// <summary>Full path of the save file this character was read from.</summary>
    public required string FilePath { get; init; }

    public required string CharacterName { get; init; }
    public required int Level { get; init; }
    public required int Slot { get; init; }
    public required bool Hardcore { get; init; }
    public required CharacterClass CharacterClass { get; init; }

    /// <summary>Raw <c>chosenMastery</c> value from the file (index within the class).</summary>
    public required int ChosenMastery { get; init; }

    public Mastery Mastery => CharacterClassCatalog.GetMastery(CharacterClass, ChosenMastery);

    public DateTimeOffset? LastPlayed { get; init; }

    /// <summary>Everything in <c>savedItems</c>: equipped items, backpack, idols and blessings.</summary>
    public IReadOnlyList<SavedItem> SavedItems { get; init; } = [];

    /// <summary><see cref="LastPlayed"/> in the user's time zone, for display.</summary>
    public DateTime? LastPlayedLocal => LastPlayed?.LocalDateTime;

    public string ClassDisplayName => CharacterClassCatalog.GetDisplayName(CharacterClass);
    public string MasteryDisplayName => CharacterClassCatalog.GetDisplayName(Mastery);
}

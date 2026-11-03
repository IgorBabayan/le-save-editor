using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Blessings;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Blessings;

/// <summary>
/// "Blessings" section of the inventory: one entry per Monolith timeline (echo) with the blessing the character holds
/// for it. Selecting an echo lists the blessings that timeline grants (normal and Grand); picking one puts it in the
/// echo's slot with its stat at the best roll, which a slider then adjusts, and "Remove" empties the slot.
/// The timelines and their blessings come from the <c>Timelines</c> table of <c>base.db</c>; a blessing is stored in
/// <c>savedItems</c> like a plain item of type <see cref="Timeline.BlessingBaseTypeId"/> in the timeline's container
/// (<see cref="Timeline.ContainerId"/>). <see cref="GetUpdates"/> collects what changed for saving.
/// </summary>
public sealed partial class BlessingsViewModel : ViewModelBase
{
    public BlessingsViewModel(CharacterSave character, IItemBaseCatalog catalog, IIconProvider iconProvider)
    {
        Character = character;

        var timelines = catalog.GetTimelines();
        IsDatabaseMissing = timelines.Count == 0;

        Echoes = timelines
            .Select(t => new BlessingEchoViewModel(t, FindSaved(character, t.ContainerId), catalog, iconProvider))
            .ToList();
        foreach (var echo in Echoes)
            echo.Changed += (_, _) =>
            {
                OnPropertyChanged(nameof(Summary));
                Changed?.Invoke(this, EventArgs.Empty);
            };

        SelectedEcho = Echoes.FirstOrDefault();
    }

    public CharacterSave Character { get; }

    /// <summary>One entry per timeline that grants blessings, in timeline order.</summary>
    public IReadOnlyList<BlessingEchoViewModel> Echoes { get; }

    /// <summary>The echo whose blessings are listed.</summary>
    [ObservableProperty]
    private BlessingEchoViewModel? _selectedEcho;

    /// <summary>
    /// <c>base.db</c> is missing or was generated before it held the timelines: nothing can be listed, and the blessings
    /// in the save stay as they are.
    /// </summary>
    public bool IsDatabaseMissing { get; }

    public bool HasEchoes => Echoes.Count > 0;

    /// <summary>"7 of 10 echoes have a blessing".</summary>
    public string Summary => $"{Echoes.Count(e => e.Selected is not null)} of {Echoes.Count} echoes have a blessing";

    /// <summary>A blessing was picked, removed or its roll changed.</summary>
    public event EventHandler? Changed;

    /// <summary>Changes to write into <c>savedItems</c>: added, replaced and removed blessings.</summary>
    /// <exception cref="ArgumentException">A blessing cannot be stored.</exception>
    public IReadOnlyList<SavedItemUpdate> GetUpdates() =>
        Echoes.Select(e => e.GetUpdate()).OfType<SavedItemUpdate>().ToList();

    /// <summary>The first blessing the save holds in <paramref name="containerId"/> (the one the game shows).</summary>
    private static SavedItem? FindSaved(CharacterSave character, int containerId) =>
        character.SavedItems.FirstOrDefault(i =>
            i.ContainerId == containerId && ItemDataReader.ReadBaseTypeId(i.Data) == Timeline.BlessingBaseTypeId);
}

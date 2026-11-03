using LE.SaveEditor.Presentation.ViewModels.Inventory;
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
/// The game shows a blessing as "Undiscovered" unless the character's stash lists it in <c>unlockedBlessings</c>;
/// <see cref="GetUndiscoveredBlessingIds"/> gives the blessings held that the stash does not list, which saving adds.
/// </summary>
public sealed partial class BlessingsViewModel : ViewModelBase
{
    private readonly StashMaterialsViewModel? _stash;

    public BlessingsViewModel(
        CharacterSave character,
        IItemBaseCatalog catalog,
        IIconProvider iconProvider,
        StashMaterialsViewModel? stash = null)
    {
        Character = character;
        _stash = stash;

        var timelines = catalog.GetTimelines();
        IsDatabaseMissing = timelines.Count == 0;

        Echoes = timelines
            .Select(t => new BlessingEchoViewModel(t, FindSaved(character, t.ContainerId), catalog, iconProvider))
            .ToList();
        foreach (var echo in Echoes)
            echo.Changed += (_, _) =>
            {
                OnPropertyChanged(nameof(Summary));
                RefreshDiscovery();
                Changed?.Invoke(this, EventArgs.Empty);
            };

        // Saving the stash (by this or another character of it) changes what it has discovered.
        if (stash is not null)
            stash.Changed += (_, _) => RefreshDiscovery();

        SelectedEcho = Echoes.FirstOrDefault();
        RefreshDiscovery();
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

    /// <summary>Shows the blessings of <paramref name="echo"/> (one of <see cref="Echoes"/>).</summary>
    [RelayCommand]
    private void SelectEcho(BlessingEchoViewModel? echo)
    {
        if (echo is not null && Echoes.Contains(echo))
            SelectedEcho = echo;
    }

    partial void OnSelectedEchoChanged(BlessingEchoViewModel? oldValue, BlessingEchoViewModel? newValue)
    {
        // The highlighted row of the echo list.
        if (oldValue is not null)
            oldValue.IsSelected = false;
        if (newValue is not null)
            newValue.IsSelected = true;
    }

    /// <summary>A blessing was picked, removed or its roll changed.</summary>
    public event EventHandler? Changed;

    /// <summary>
    /// Blessings the character holds (with the changes made here) that its stash has not discovered; saving adds them to
    /// the stash's <c>unlockedBlessings</c>. Empty without a stash file.
    /// </summary>
    public IReadOnlyList<int> GetUndiscoveredBlessingIds()
    {
        if (_stash is null)
            return [];

        var unlocked = _stash.Stash.UnlockedBlessings.ToHashSet();
        return HeldBlessingIds().Where(id => !unlocked.Contains(id)).Distinct().ToList();
    }

    /// <summary>Why held blessings may show as "Undiscovered" in the game, and what saving does about it; <c>null</c> when all are discovered.</summary>
    public string? DiscoveryHint
    {
        get
        {
            if (!HeldBlessingIds().Any())
                return null;

            if (_stash is null)
                return "No stash file was found for this character, so the game shows these blessings as Undiscovered.";

            var count = GetUndiscoveredBlessingIds().Count;
            return count == 0
                ? null
                : $"{count} {(count == 1 ? "blessing is" : "blessings are")} not discovered in {_stash.Stash.FileName} yet; " +
                  "saving marks them as discovered, otherwise the game shows them as Undiscovered.";
        }
    }

    public bool HasDiscoveryHint => DiscoveryHint is not null;

    /// <summary>Blessing ids in the slots (blessings the editor cannot read have none).</summary>
    private IEnumerable<int> HeldBlessingIds() =>
        Echoes.Select(e => e.Selected?.Base.SubTypeId ?? -1).Where(id => id >= 0);

    private void RefreshDiscovery()
    {
        var unlocked = _stash?.Stash.UnlockedBlessings.ToHashSet();
        foreach (var echo in Echoes)
            echo.IsUndiscovered = unlocked is not null && echo.Selected is { Base.SubTypeId: >= 0 } held
                                  && !unlocked.Contains(held.Base.SubTypeId);

        OnPropertyChanged(nameof(DiscoveryHint));
        OnPropertyChanged(nameof(HasDiscoveryHint));
    }

    /// <summary>Changes to write into <c>savedItems</c>: added, replaced and removed blessings.</summary>
    /// <exception cref="ArgumentException">A blessing cannot be stored.</exception>
    public IReadOnlyList<SavedItemUpdate> GetUpdates() =>
        Echoes.Select(e => e.GetUpdate()).OfType<SavedItemUpdate>().ToList();

    /// <summary>The first blessing the save holds in <paramref name="containerId"/> (the one the game shows).</summary>
    private static SavedItem? FindSaved(CharacterSave character, int containerId) =>
        character.SavedItems.FirstOrDefault(i =>
            i.ContainerId == containerId && ItemDataReader.ReadBaseTypeId(i.Data) == Timeline.BlessingBaseTypeId);
}

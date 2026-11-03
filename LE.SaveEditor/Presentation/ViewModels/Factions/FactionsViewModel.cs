using LE.SaveEditor.Presentation.ViewModels.Inventory;
using LE.SaveEditor.Services.GameData.Factions;

namespace LE.SaveEditor.Presentation.ViewModels.Factions;

/// <summary>
/// "Factions" section of the inventory: Circle of Fortune, Forgotten Knights and The Woven (<see cref="FactionCatalog"/>).
/// The factions and their ranks live in the stash file (<c>"factions": {"0": {"rank": 1, "reputation": 8, ...}}</c>), so
/// they are shared by every character using that stash. A faction the stash has no entry for is not unlocked in game
/// and stays disabled: the editor never creates faction entries.
/// </summary>
public sealed class FactionsViewModel : ViewModelBase
{
    public FactionsViewModel(StashMaterialsViewModel? stash)
    {
        Factions = FactionCatalog.All.Select(f => new FactionViewModel(f, stash)).ToList();

        // Another character of the stash changed a rank, or the stash was saved.
        if (stash is not null)
            stash.Changed += (_, _) =>
            {
                foreach (var faction in Factions)
                    faction.Refresh();
                OnPropertyChanged(nameof(Summary));
            };
    }

    /// <summary>Every faction of the section, in display order.</summary>
    public IReadOnlyList<FactionViewModel> Factions { get; }

    /// <summary>"2 of 3 factions unlocked".</summary>
    public string Summary => $"{Factions.Count(f => f.IsUnlocked)} of {Factions.Count} factions unlocked";
}

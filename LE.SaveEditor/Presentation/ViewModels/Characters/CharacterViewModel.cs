using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Presentation.Services.Toasts;
using LE.SaveEditor.Presentation.ViewModels.Inventory;
using LE.SaveEditor.Presentation.ViewModels.Waypoints;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Characters;

/// <summary>
/// Details of the character selected in the character list, split into the
/// "Inventory" (blessings included) and "Waypoints" tabs of the main window.
/// </summary>
public sealed partial class CharacterViewModel : ViewModelBase
{
    public CharacterViewModel(
        CharacterSave character,
        IDialogService dialogService,
        IIconProvider iconProvider,
        IItemBaseCatalog itemBaseCatalog,
        IToastService toastService,
        StashMaterialsViewModel? stash = null)
    {
        Character = character;
        Stash = stash;
        Inventory = new InventoryViewModel(character, dialogService, iconProvider, itemBaseCatalog, toastService, stash);
        Inventory.Changed += (_, _) => RefreshHasUnsavedChanges();
        Waypoints = new WaypointsViewModel(character);
        Waypoints.Changed += (_, _) => RefreshHasUnsavedChanges();
        if (stash is not null)
        {
            stash.Changed += (_, _) => RefreshHasUnsavedChanges();
            RefreshHasUnsavedChanges();
        }
    }

    /// <summary>Material storage of the stash the character uses (shared with the other characters of that stash).</summary>
    public StashMaterialsViewModel? Stash { get; }

    public CharacterSave Character { get; }

    /// <summary>"Inventory" tab.</summary>
    public InventoryViewModel Inventory { get; }

    /// <summary>"Waypoints" tab.</summary>
    public WaypointsViewModel Waypoints { get; }

    /// <summary>Something was changed that is not written to the save file or the character's stash file yet.</summary>
    [ObservableProperty]
    private bool _hasUnsavedChanges;

    /// <summary>Everything to write into <c>savedItems</c> to save the changes.</summary>
    /// <exception cref="ArgumentException">An edited item cannot be stored.</exception>
    public IReadOnlyList<SavedItemUpdate> GetUpdates() => Inventory.GetUpdates();

    /// <summary>
    /// Blessings the character holds that its stash has not discovered: written into the stash's <c>unlockedBlessings</c>
    /// on save, so the game does not show them as "Undiscovered".
    /// </summary>
    public IReadOnlyList<int> GetBlessingsToDiscover() => Inventory.Blessings.GetUndiscoveredBlessingIds();

    /// <summary>The waypoints to write into <c>unlockedWaypointScenes</c>; <c>null</c> when they did not change.</summary>
    public IReadOnlyList<string>? GetWaypointScenes() => Waypoints.GetChangedScenes();

    /// <summary>
    /// An item applied again without changes, or moved back to its cell, leaves nothing to save,
    /// so the flag follows the actual difference to the file rather than "something was touched".
    /// </summary>
    private void RefreshHasUnsavedChanges()
    {
        try
        {
            HasUnsavedChanges = GetUpdates().Count > 0 || Waypoints.HasChanges || Stash?.HasChanges == true
                                || GetBlessingsToDiscover().Count > 0;
        }
        catch (ArgumentException)
        {
            // Saving reports why the item cannot be stored.
            HasUnsavedChanges = true;
        }
    }
}

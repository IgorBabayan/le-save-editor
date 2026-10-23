using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Presentation.Services.Toasts;
using LE.SaveEditor.Presentation.ViewModels.Blessings;
using LE.SaveEditor.Presentation.ViewModels.Inventory;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.ViewModels.Characters;

/// <summary>
/// Details of the character selected in the character list, split into the
/// "Blessings" and "Inventory" tabs of the main window.
/// </summary>
public sealed partial class CharacterViewModel : ViewModelBase
{
    public CharacterViewModel(
        CharacterSave character,
        IDialogService dialogService,
        IIconProvider iconProvider,
        IItemBaseCatalog itemBaseCatalog,
        IToastService toastService)
    {
        Character = character;
        Blessings = new BlessingsViewModel(character);
        Inventory = new InventoryViewModel(character, dialogService, iconProvider, itemBaseCatalog, toastService);
        Inventory.Changed += (_, _) => RefreshHasUnsavedChanges();
    }

    public CharacterSave Character { get; }

    /// <summary>"Blessings" tab.</summary>
    public BlessingsViewModel Blessings { get; }

    /// <summary>"Inventory" tab.</summary>
    public InventoryViewModel Inventory { get; }

    /// <summary>Something was changed that is not written to the save file yet.</summary>
    [ObservableProperty]
    private bool _hasUnsavedChanges;

    /// <summary>Everything to write into <c>savedItems</c> to save the changes.</summary>
    /// <exception cref="ArgumentException">An edited item cannot be stored.</exception>
    public IReadOnlyList<SavedItemUpdate> GetUpdates() => Inventory.GetUpdates();

    /// <summary>
    /// An item applied again without changes, or moved back to its cell, leaves nothing to save,
    /// so the flag follows the actual difference to the file rather than "something was touched".
    /// </summary>
    private void RefreshHasUnsavedChanges()
    {
        try
        {
            HasUnsavedChanges = GetUpdates().Count > 0;
        }
        catch (ArgumentException)
        {
            // Saving reports why the item cannot be stored.
            HasUnsavedChanges = true;
        }
    }
}

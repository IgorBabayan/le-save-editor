using LE.SaveEditor.Presentation.Services;
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
public sealed class CharacterViewModel(
    CharacterSave character,
    IDialogService dialogService,
    IIconProvider iconProvider,
    IItemBaseCatalog itemBaseCatalog) : ViewModelBase
{
    public CharacterSave Character { get; } = character;

    /// <summary>"Blessings" tab.</summary>
    public BlessingsViewModel Blessings { get; } = new(character);

    /// <summary>"Inventory" tab.</summary>
    public InventoryViewModel Inventory { get; } = new(character, dialogService, iconProvider, itemBaseCatalog);
}

using LE.SaveEditor.Presentation.ViewModels.Inventory;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.Services;

public interface IDialogService
{
    /// <summary>Shows the modal Settings dialog.</summary>
    /// <returns><c>true</c> if the user saved the settings.</returns>
    bool ShowSettings();

    /// <summary>Shows a modal Yes/No question.</summary>
    /// <returns><c>true</c> if the user answered Yes.</returns>
    bool Confirm(string title, string message);

    /// <summary>
    /// Shows the modal database generator dialog: runs <c>LE.SaveEditor.DB</c> in the background,
    /// shows its progress bars and returns after the user pressed OK.
    /// </summary>
    /// <returns>The generator's outcome, or <c>null</c> if it did not finish.</returns>
    DatabaseGenerationResult? ShowDatabaseGenerator();

    /// <summary>
    /// Shows the modal item builder for <paramref name="slot"/>, starting from <paramref name="current"/> when the slot holds an item.
    /// </summary>
    /// <returns>The built item, or <c>null</c> if the user cancelled.</returns>
    BuiltItem? ShowItemBuilder(EquipmentSlot slot, BuiltItem? current);
}

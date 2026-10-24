using LE.SaveEditor.Presentation.ViewModels.Inventory;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.GameData.Items;
using LE.SaveEditor.Services.Saves.Models;

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
    /// Bases of <paramref name="characterClass"/> are listed first.
    /// </summary>
    /// <returns>The built item, or <c>null</c> if the user cancelled.</returns>
    BuiltItem? ShowItemBuilder(EquipmentSlot slot, CharacterClass characterClass, BuiltItem? current);

    /// <summary>
    /// Opens the item inspector, a compact item builder, next to the control showing <paramref name="slot"/>
    /// (falls back to the modal builder when that control cannot be found). Returns at once; when the user applies
    /// the item it replaces <see cref="EquipmentSlotViewModel.Item"/>.
    /// </summary>
    void ShowItemInspector(EquipmentSlotViewModel slot, CharacterClass characterClass);

    /// <summary>
    /// Opens the item inspector for a new item of <paramref name="slot"/> next to <paramref name="anchor"/> (a view
    /// element; falls back to the modal builder when it is missing or cannot host the inspector). Returns at once;
    /// <paramref name="applied"/> is called with the item when the user applies it.
    /// </summary>
    void ShowItemInspector(EquipmentSlot slot, CharacterClass characterClass, object? anchor, Action<BuiltItem> applied);
}

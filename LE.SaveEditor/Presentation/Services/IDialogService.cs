using LE.SaveEditor.Services.GameData;

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
}

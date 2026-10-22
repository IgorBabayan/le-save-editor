namespace LE.SaveEditor.Presentation.Services;

public interface IDialogService
{
    /// <summary>Shows the modal Settings dialog.</summary>
    /// <returns><c>true</c> if the user saved the settings.</returns>
    bool ShowSettings();
}

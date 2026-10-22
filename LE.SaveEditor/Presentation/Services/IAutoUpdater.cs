namespace LE.SaveEditor.Presentation.Services;

public interface IAutoUpdater
{
    /// <summary>
    /// If auto-update is enabled and a newer release exists, downloads it (showing progress)
    /// and starts the installer.
    /// </summary>
    /// <returns><c>true</c> if the installer was started and the application must shut down now.</returns>
    Task<bool> TryUpdateOnStartupAsync();
}

using LE.SaveEditor.Presentation.Views.Settings;

namespace LE.SaveEditor.Presentation.Services;

public sealed class DialogService(IServiceProvider services) : IDialogService
{
    public bool ShowSettings()
    {
        var window = services.GetRequiredService<SettingsWindow>();
        window.Owner = WindowHelper.GetActiveWindow();
        return window.ShowDialog() == true;
    }
}

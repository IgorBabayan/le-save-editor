using System.Windows;
using LE.SaveEditor.Extensions;
using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Presentation.Views;
using LE.SaveEditor.Services.Settings;
using LE.SaveEditor.Services.Theming;

namespace LE.SaveEditor;

/// <summary>
/// Interaction logic for App.xaml.
/// Composition root: builds the DI container and creates the main window from it.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _services;

    /// <summary>Root service provider. Prefer constructor injection; use this only where WPF creates objects for you.</summary>
    public static IServiceProvider Services =>
        ((App)Current)._services ?? throw new InvalidOperationException("Services are not initialized yet.");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // The update window may open and close before the main window exists;
        // that must not shut the app down.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _services = new ServiceCollection()
            .AddApplicationServices()
            .BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        // Apply the saved theme before any window is created.
        var settings = _services.GetRequiredService<ISettingsService>().Current;
        _services.GetRequiredService<IThemeService>().Apply(settings.Theme);

        // Auto-update: if a newer release was downloaded and its installer started,
        // exit so the installer can replace the files. It starts the new version afterwards.
        if (await TryAutoUpdateAsync(_services))
        {
            Shutdown();
            return;
        }

        var mainWindow = _services.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();

        ShutdownMode = ShutdownMode.OnLastWindowClose;
    }

    private static async Task<bool> TryAutoUpdateAsync(IServiceProvider services)
    {
        try
        {
            return await services.GetRequiredService<IAutoUpdater>().TryUpdateOnStartupAsync();
        }
        catch (Exception ex)
        {
            // An update problem must never prevent the editor from starting.
            System.Diagnostics.Debug.WriteLine($"Auto-update failed: {ex}");
            return false;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}

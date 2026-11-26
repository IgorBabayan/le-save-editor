using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LE.SaveEditor.Extensions;
using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Presentation.Views;
using LE.SaveEditor.Services.Settings;
using LE.SaveEditor.Services.Theming;

namespace LE.SaveEditor;

/// <summary>
/// Composition root: builds the DI container and creates the main window from it.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _services;

    /// <summary>Root service provider. Prefer constructor injection; use this only where XAML creates objects for you.</summary>
    public static IServiceProvider Services =>
        (Current as App)?._services ?? throw new InvalidOperationException("Services are not initialized yet.");

    /// <summary>Main window of the editor; <c>null</c> before startup has finished.</summary>
    public static Avalonia.Controls.Window? MainWindow =>
        (Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

    /// <summary>Closes every window and ends the process with exit code 0.</summary>
    public static void Shutdown() =>
        (Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
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

            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;
            desktop.MainWindow = _services.GetRequiredService<MainWindow>();
            desktop.Exit += (_, _) => _services?.Dispose();

            // Auto-update runs in the background and reports through toasts:
            // download progress, then a countdown before the editor restarts into the new version.
            _ = RunAutoUpdateAsync(_services);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task RunAutoUpdateAsync(IServiceProvider services)
    {
        try
        {
            await services.GetRequiredService<IAutoUpdater>().RunOnStartupAsync();
        }
        catch (Exception ex)
        {
            // An update problem must never take the editor down.
            System.Diagnostics.Debug.WriteLine($"Auto-update failed: {ex}");
        }
    }
}

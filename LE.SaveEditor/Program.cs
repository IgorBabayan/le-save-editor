using Avalonia;

namespace LE.SaveEditor;

internal static class Program
{
    /// <summary>
    /// Entry point. Nothing that uses Avalonia may run before <see cref="BuildAvaloniaApp"/> has started the app:
    /// the platform is initialized there.
    /// </summary>
    [STAThread]
    public static int Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    /// <summary>Also used by the XAML previewer.</summary>
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}

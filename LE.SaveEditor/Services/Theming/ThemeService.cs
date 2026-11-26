using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace LE.SaveEditor.Services.Theming;

public sealed class ThemeService : IThemeService
{
    /// <summary>Folder (inside the assembly) that holds one palette dictionary per theme.</summary>
    private const string PaletteFolder = "avares://LE.SaveEditor/Themes/Colors/";

    private readonly Dictionary<AppTheme, IResourceDictionary> _cache = new();

    public AppTheme CurrentTheme { get; private set; } = AppTheme.DefaultNight;

    public void Apply(AppTheme theme)
    {
        if (!Enum.IsDefined(theme))
            theme = AppTheme.DefaultNight;

        var app = Application.Current ?? throw new InvalidOperationException("The application is not running.");
        var dictionaries = app.Resources.MergedDictionaries;
        var palette = GetPalette(theme);

        var index = -1;
        for (var i = 0; i < dictionaries.Count; i++)
        {
            if (IsPalette(dictionaries[i]))
            {
                index = i;
                break;
            }
        }

        if (index >= 0)
        {
            if (!ReferenceEquals(dictionaries[index], palette))
                dictionaries[index] = palette;
        }
        else
        {
            // Palette must come before the dictionaries that reference its brushes.
            dictionaries.Insert(0, palette);
        }

        // Fluent parts that are not restyled (text selection, menus) follow the palette's brightness.
        app.RequestedThemeVariant = theme == AppTheme.DefaultLight ? ThemeVariant.Light : ThemeVariant.Dark;
        CurrentTheme = theme;
    }

    public IResourceDictionary GetPalette(AppTheme theme)
    {
        if (!_cache.TryGetValue(theme, out var palette))
        {
            // Loaded once per theme; every later switch reuses the same dictionary.
            palette = (IResourceDictionary)AvaloniaXamlLoader.Load(new Uri($"{PaletteFolder}{theme}.axaml"));
            _cache[theme] = palette;
        }

        return palette;
    }

    private bool IsPalette(IResourceProvider dictionary) =>
        dictionary is ResourceInclude { Source: { } source } && source.OriginalString.StartsWith(PaletteFolder, StringComparison.OrdinalIgnoreCase)
        || dictionary is IResourceDictionary d && _cache.ContainsValue(d);
}

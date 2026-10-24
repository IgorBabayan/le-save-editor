using System.Globalization;
using System.Windows;
using System.Windows.Data;
using LE.SaveEditor.Services.GameData;
using LE.SaveEditor.Services.Saves.Models;

namespace LE.SaveEditor.Presentation.Converters;

/// <summary>
/// <see cref="CharacterSave"/> → its mastery icon, or its class icon when it has no mastery
/// (<c>null</c> when the image is missing, so the view can show a placeholder).
/// </summary>
public sealed class CharacterIconConverter : IValueConverter
{
    // WPF creates converters from XAML, so the provider comes from the root container
    // (not available in the XAML designer, where the placeholder is shown instead).
    private static ICharacterIconProvider? Provider =>
        Application.Current is App ? App.Services.GetService<ICharacterIconProvider>() : null;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is CharacterSave character
            ? Provider?.Get(character.CharacterClass, character.Mastery)
            : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

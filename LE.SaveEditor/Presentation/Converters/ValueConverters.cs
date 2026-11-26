using System.Globalization;
using Avalonia.Data.Converters;

namespace LE.SaveEditor.Presentation.Converters;

/// <summary>Small converters used from XAML through <c>{x:Static}</c>.</summary>
public static class ValueConverters
{
    /// <summary>
    /// <c>true</c> when the value's text equals the converter parameter (enum states as style classes,
    /// e.g. <c>Classes.error="{Binding Severity, Converter=..., ConverterParameter=Error}"</c>).
    /// </summary>
    public static readonly IValueConverter EqualsParameter = new FuncValueConverter<object?, object?, bool>(
        (value, parameter) => value is not null && string.Equals(value.ToString(), parameter?.ToString(), StringComparison.Ordinal));

    /// <summary>Width minus the converter parameter (never below 0): full-row group headers of wrapping lists.</summary>
    public static readonly IValueConverter Subtract = new FuncValueConverter<double, object?, double>(
        (value, parameter) => Math.Max(0, value - (double.TryParse(parameter?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var amount) ? amount : 0)));

    /// <summary><c>true</c> for list rows that are items, <c>false</c> for group titles (they stay unselectable).</summary>
    public static readonly IValueConverter IsNotGroupHeader = new FuncValueConverter<object?, bool>(
        value => value is not ViewModels.ListGroupHeader);
}

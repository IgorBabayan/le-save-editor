using System.Globalization;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.ViewModels.Inventory;

/// <summary>
/// One implicit of the base selected in the item builder, edited with a slider.
/// The slider moves the roll the game stores (0 = minimum, 255 = maximum); <see cref="Value"/> follows from it.
/// </summary>
public sealed partial class ItemStatViewModel : ViewModelBase
{
    /// <summary>Highest roll; the slider range is 0 … <see cref="MaxRoll"/>.</summary>
    public const double MaxRoll = byte.MaxValue;

    public ItemStatViewModel(ItemImplicit implicitStat, double? value = null)
    {
        Implicit = implicitStat;
        // A new base starts at its best roll.
        _roll = value is null ? MaxRoll : ToRoll(implicitStat, value.Value);
    }

    public ItemImplicit Implicit { get; }

    /// <summary>The implicit has a single possible value, so its slider is disabled.</summary>
    public bool IsFixed => Implicit.Max - Implicit.Min <= double.Epsilon;

    public bool IsEditable => !IsFixed;

    /// <summary>Roll as the game stores it, 0 … 255.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Value))]
    [NotifyPropertyChangedFor(nameof(DisplayText))]
    private double _roll;

    /// <summary>Value of the implicit for <see cref="Roll"/>.</summary>
    public double Value => FromRoll(Implicit, Roll);

    /// <summary>E.g. "+42 Armor", "+12% Fire Resistance", "+8% increased Movement Speed".</summary>
    public string DisplayText => Format(Implicit, Value);

    /// <summary>E.g. "(30 – 45)".</summary>
    public string RangeText => IsFixed ? string.Empty : $"({FormatNumber(Implicit, Implicit.Min)} – {FormatNumber(Implicit, Implicit.Max)})";

    partial void OnRollChanged(double value)
    {
        // Keep whole rolls even when set from code (the slider snaps to whole ticks itself).
        var snapped = Math.Clamp(Math.Round(value), 0, MaxRoll);
        if (snapped != value)
            Roll = snapped;
    }

    public static byte ToRoll(ItemImplicit implicitStat, double value)
    {
        var range = implicitStat.Max - implicitStat.Min;
        if (range <= double.Epsilon)
            return byte.MaxValue;
        return (byte)Math.Clamp(Math.Round((value - implicitStat.Min) / range * MaxRoll), 0, MaxRoll);
    }

    public static double FromRoll(ItemImplicit implicitStat, double roll)
    {
        var value = implicitStat.Min + (implicitStat.Max - implicitStat.Min) * Math.Clamp(roll, 0, MaxRoll) / MaxRoll;
        // Whole-number stats (armour, health, ...) only take whole values in the game.
        return IsWholeRange(implicitStat) ? Math.Round(value) : value;
    }

    /// <summary>
    /// Tooltip line as the game writes it: "+14 Armor", "+21% Fire Resistance", "+5% increased Spell Damage",
    /// "13% more Damage"; negative increased / more read "reduced" / "less".
    /// </summary>
    public static string Format(ItemImplicit implicitStat, double value)
    {
        var name = implicitStat.Property;

        if (implicitStat.Type is ImplicitModifierType.Increased or ImplicitModifierType.More)
        {
            var number = FormatNumber(implicitStat, Math.Abs(value));
            if (implicitStat.NameIncludesModifier)
                return $"{(value < 0 ? "-" : implicitStat.ShowPlus ? "+" : string.Empty)}{number} {name}";

            var word = implicitStat.Type == ImplicitModifierType.Increased
                ? (value < 0 ? "reduced" : "increased")
                : (value < 0 ? "less" : "more");
            return implicitStat.Type == ImplicitModifierType.More
                ? $"{number} {word} {name}"
                : $"{(value < 0 || !implicitStat.ShowPlus ? string.Empty : "+")}{number} {word} {name}";
        }

        var sign = value < 0 ? "-" : implicitStat.ShowPlus ? "+" : string.Empty;
        return $"{sign}{FormatNumber(implicitStat, Math.Abs(value))} {name}";
    }

    private static bool IsWholeRange(ItemImplicit implicitStat) =>
        !implicitStat.IsPercent && implicitStat.Min % 1 == 0 && implicitStat.Max % 1 == 0;

    private static string FormatNumber(ItemImplicit implicitStat, double value) =>
        implicitStat.IsPercent
            ? (value * implicitStat.PercentScale).ToString("0.#", CultureInfo.CurrentCulture) + "%"
            : value.ToString(IsWholeRange(implicitStat) ? "0" : "0.##", CultureInfo.CurrentCulture);
}

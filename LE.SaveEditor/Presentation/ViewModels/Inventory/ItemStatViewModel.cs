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
    public string RangeText => IsFixed ? string.Empty
        : Implicit.Template is not null ? $"({FormatTemplateNumber(Implicit.TemplateMin)} – {FormatTemplateNumber(Implicit.TemplateMax)})"
        : $"({FormatNumber(Implicit, Implicit.Min)} – {FormatNumber(Implicit, Implicit.Max)})";

    private static string FormatTemplateNumber(double value) => value.ToString("0.#", CultureInfo.CurrentCulture);

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
        // A unique modifier labelled by its tooltip line: the line's own number follows the roll.
        if (implicitStat.Template is not null)
            return Compose(implicitStat, FormatTemplateValue(implicitStat, TemplateValue(implicitStat, value)), negative: false);

        return Compose(implicitStat, FormatNumber(implicitStat, Math.Abs(value)), value < 0);
    }

    /// <summary>
    /// Tooltip line with the whole roll range, as an item card shows it: "+(1–4) Attunement", "+(10–15)% Fire Resistance";
    /// a fixed value is shown as it is ("+38 Armor").
    /// </summary>
    public static string FormatRange(ItemImplicit implicitStat)
    {
        if (implicitStat.Max - implicitStat.Min <= double.Epsilon)
            return Format(implicitStat, implicitStat.Max);

        if (implicitStat.Template is not null)
        {
            var from = FormatTemplateValue(implicitStat, implicitStat.TemplateMin);
            var to = FormatTemplateValue(implicitStat, implicitStat.TemplateMax);
            return Compose(implicitStat, $"({from}–{to})", negative: false);
        }

        // Both ends negative: show the sizes ("-(5–10)%") and let the sign / reduced / less wording carry the direction.
        var negative = implicitStat.Max < 0;
        var low = negative ? Math.Abs(implicitStat.Max) : implicitStat.Min;
        var high = negative ? Math.Abs(implicitStat.Min) : implicitStat.Max;
        var suffix = implicitStat.IsPercent ? "%" : string.Empty;
        return Compose(implicitStat, $"({FormatPlainNumber(implicitStat, low)}–{FormatPlainNumber(implicitStat, high)}){suffix}", negative);
    }

    /// <summary>Puts an already formatted number into the property's tooltip wording.</summary>
    private static string Compose(ItemImplicit implicitStat, string number, bool negative)
    {
        if (implicitStat.Template is { } template)
            return template.Replace("{value}", number);

        var name = implicitStat.Property;

        if (implicitStat.Type is ImplicitModifierType.Increased or ImplicitModifierType.More)
        {
            if (implicitStat.NameIncludesModifier)
                return $"{(negative ? "-" : implicitStat.ShowPlus ? "+" : string.Empty)}{number} {name}";

            var word = implicitStat.Type == ImplicitModifierType.Increased
                ? (negative ? "reduced" : "increased")
                : (negative ? "less" : "more");
            return implicitStat.Type == ImplicitModifierType.More
                ? $"{number} {word} {name}"
                : $"{(negative || !implicitStat.ShowPlus ? string.Empty : "+")}{number} {word} {name}";
        }

        var sign = negative ? "-" : implicitStat.ShowPlus ? "+" : string.Empty;
        return $"{sign}{number} {name}";
    }

    /// <summary>The number of a template line for <paramref name="value"/> (its own scale, e.g. 100–150 for 1–1.5).</summary>
    private static double TemplateValue(ItemImplicit implicitStat, double value)
    {
        var range = implicitStat.Max - implicitStat.Min;
        var t = range <= double.Epsilon ? 1 : (value - implicitStat.Min) / range;
        return implicitStat.TemplateMin + (implicitStat.TemplateMax - implicitStat.TemplateMin) * t;
    }

    private static string FormatTemplateValue(ItemImplicit implicitStat, double shown)
    {
        var whole = implicitStat.TemplateMin % 1 == 0 && implicitStat.TemplateMax % 1 == 0;
        return shown.ToString(whole ? "0" : "0.#", CultureInfo.CurrentCulture);
    }

    /// <summary>The number without its "%" sign (the range adds it once after the brackets).</summary>
    private static string FormatPlainNumber(ItemImplicit implicitStat, double value) =>
        implicitStat.IsPercent
            ? (value * implicitStat.PercentScale).ToString("0.#", CultureInfo.CurrentCulture)
            : value.ToString(IsWholeRange(implicitStat) ? "0" : "0.##", CultureInfo.CurrentCulture);

    private static bool IsWholeRange(ItemImplicit implicitStat) =>
        !implicitStat.IsPercent && implicitStat.Min % 1 == 0 && implicitStat.Max % 1 == 0;

    private static string FormatNumber(ItemImplicit implicitStat, double value) =>
        implicitStat.IsPercent
            ? (value * implicitStat.PercentScale).ToString("0.#", CultureInfo.CurrentCulture) + "%"
            : value.ToString(IsWholeRange(implicitStat) ? "0" : "0.##", CultureInfo.CurrentCulture);
}

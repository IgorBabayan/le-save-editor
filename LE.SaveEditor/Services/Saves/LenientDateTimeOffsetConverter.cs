using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace LE.SaveEditor.Services.Saves;

/// <summary>
/// Reads an optional timestamp written by the game (e.g. <c>lastPlayed</c>) without ever failing the whole save:
/// ISO 8601 (any number of fraction digits), culture formatted dates, Unix seconds / milliseconds or .NET ticks,
/// as a string or a number. Empty, unknown or out-of-range values become <c>null</c>.
/// </summary>
public sealed partial class LenientDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
{
    // Anything above these is milliseconds / ticks rather than seconds / milliseconds.
    private const long MaxUnixSeconds = 100_000_000_000;          // year 5138
    private const long MaxUnixMilliseconds = 100_000_000_000_000; // year 5138

    private static readonly CultureInfo[] Cultures = [CultureInfo.InvariantCulture, CultureInfo.CurrentCulture];

    /// <summary>.NET allows at most 7 fraction digits ("…:30.7188329"); longer fractions are cut to 7.</summary>
    [GeneratedRegex(@"(\.\d{7})\d+")]
    private static partial Regex ExtraFractionDigits();

    public override bool HandleNull => true;

    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return ParseText(reader.GetString());

            case JsonTokenType.Number:
                return reader.TryGetInt64(out var number)
                    ? FromNumber(number)
                    : reader.TryGetDouble(out var real) && real is >= -9.2e18 and <= 9.2e18
                        ? FromNumber((long)real)
                        : null;

            case JsonTokenType.StartObject or JsonTokenType.StartArray:
                // Not a timestamp at all: skip the whole value.
                reader.Skip();
                return null;

            default:
                // null, true, false
                return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value is { } date)
            writer.WriteStringValue(date.ToString("O", CultureInfo.InvariantCulture));
        else
            writer.WriteNullValue();
    }

    private static DateTimeOffset? ParseText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var value = ExtraFractionDigits().Replace(text.Trim(), "$1");

        foreach (var culture in Cultures)
        {
            if (DateTimeOffset.TryParse(value, culture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal, out var date))
                return IsUnset(date) ? null : date;
        }

        return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            ? FromNumber(number)
            : null;
    }

    private static DateTimeOffset? FromNumber(long number)
    {
        try
        {
            var date = number switch
            {
                <= 0 => (DateTimeOffset?)null,
                < MaxUnixSeconds => DateTimeOffset.FromUnixTimeSeconds(number),
                < MaxUnixMilliseconds => DateTimeOffset.FromUnixTimeMilliseconds(number),
                _ => new DateTimeOffset(number, TimeSpan.Zero) // .NET ticks
            };

            return date is { } d && !IsUnset(d) ? d : null;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    /// <summary><c>0001-01-01</c> is what an unset C# DateTime serializes to.</summary>
    private static bool IsUnset(DateTimeOffset date) => date.Year <= 1;
}

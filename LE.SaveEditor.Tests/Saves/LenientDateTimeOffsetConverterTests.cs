using System.Text.Json;
using LE.SaveEditor.Services.Saves;

namespace LE.SaveEditor.Tests.Saves;

public class LenientDateTimeOffsetConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new LenientDateTimeOffsetConverter() } };

    private static DateTimeOffset? Read(string json) => JsonSerializer.Deserialize<DateTimeOffset?>(json, Options);

    [Theory]
    [InlineData("\"2026-11-02T10:52:37Z\"")]
    [InlineData("\"2026-11-02T10:52:37.718832912345Z\"")] // more than 7 fraction digits
    [InlineData("1793616757")] // Unix seconds
    [InlineData("1793616757000")] // Unix milliseconds
    [InlineData("\"1793616757\"")] // seconds as text
    public void Read_KnownFormats_GiveTheDate(string json)
    {
        var date = Read(json);

        Assert.NotNull(date);
        Assert.Equal(new DateTime(2026, 11, 2), date.Value.UtcDateTime.Date);
    }

    [Fact]
    public void Read_Ticks_GiveTheDate()
    {
        var ticks = new DateTimeOffset(2026, 11, 2, 0, 0, 0, TimeSpan.Zero).Ticks;
        Assert.Equal(2026, Read(ticks.ToString())!.Value.Year);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("\"\"")]
    [InlineData("\"   \"")]
    [InlineData("\"not a date\"")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("true")]
    [InlineData("{\"a\":[1,2]}")]
    [InlineData("[1,2,3]")]
    [InlineData("\"0001-01-01T00:00:00\"")]
    [InlineData("1e300")]
    public void Read_UnknownValues_GiveNull(string json) => Assert.Null(Read(json));

    [Fact]
    public void Read_InsideObject_SkipsComplexValueAndKeepsReading()
    {
        var result = JsonSerializer.Deserialize<Holder>("""{"When":{"x":[1]},"Name":"ok"}""");
        Assert.Null(result!.When);
        Assert.Equal("ok", result.Name);
    }

    [Fact]
    public void Write_RoundTrips()
    {
        var date = new DateTimeOffset(2026, 11, 2, 10, 52, 37, TimeSpan.Zero);
        Assert.Equal(date, Read(JsonSerializer.Serialize<DateTimeOffset?>(date, Options)));
        Assert.Equal("null", JsonSerializer.Serialize<DateTimeOffset?>(null, Options));
    }

    private sealed class Holder
    {
        [System.Text.Json.Serialization.JsonConverter(typeof(LenientDateTimeOffsetConverter))]
        public DateTimeOffset? When { get; init; }

        public string? Name { get; init; }
    }
}

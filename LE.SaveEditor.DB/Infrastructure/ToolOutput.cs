using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LE.SaveEditor.DB.Infrastructure;

/// <summary>Kind of a text line written by the generator; drives its console color.</summary>
internal enum OutputLevel
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// Everything the generator prints goes through here, so the same run can be shown either as
/// colored console text (interactive use) or as JSON lines read by the editor (<c>--machine</c>).
/// </summary>
/// <remarks>
/// Machine protocol: one JSON object per stdout line, see <see cref="ToolEvent"/>.
/// <list type="bullet">
/// <item><c>{"type":"bar-start","id":1,"title":"Downloading game data"}</c></item>
/// <item><c>{"type":"bar","id":1,"fraction":0.45,"detail":"1.2 MB / 2.7 MB"}</c></item>
/// <item><c>{"type":"bar-end","id":1,"fraction":1,"detail":"2.7 MB","completed":true}</c></item>
/// <item><c>{"type":"log","level":"success","text":"Database created: ..."}</c></item>
/// <item><c>{"type":"done","operation":"Update","status":"succeeded"}</c></item>
/// </list>
/// </remarks>
internal static class ToolOutput
{
    private static readonly Lock Sync = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary><c>true</c> when started by the editor with <c>--machine</c>: JSON lines instead of console text.</summary>
    public static bool IsMachine { get; private set; }

    /// <summary>
    /// Switches to JSON output. Started by the editor the process has no console window,
    /// so stdout is wrapped in a UTF-8 writer instead of setting <see cref="Console.OutputEncoding"/>
    /// (which needs a console and throws without one).
    /// </summary>
    public static void EnableMachineMode()
    {
        IsMachine = true;
        var writer = new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
        {
            AutoFlush = true
        };
        Console.SetOut(TextWriter.Synchronized(writer));
    }

    /// <summary>Writes a line of text (console) or a <c>log</c> event (machine).</summary>
    public static void Line(string text, OutputLevel level = OutputLevel.Info)
    {
        if (IsMachine)
        {
            Event(new ToolEvent("log") { Level = level, Text = text });
            return;
        }

        lock (Sync)
        {
            var previous = Console.ForegroundColor;
            var color = ColorOf(level);
            if (color is { } c)
                Console.ForegroundColor = c;

            Console.WriteLine(text);
            Console.ForegroundColor = previous;
        }
    }

    /// <summary>Writes one JSON event line. Only used in machine mode.</summary>
    public static void Event(ToolEvent toolEvent)
    {
        var json = JsonSerializer.Serialize(toolEvent, JsonOptions);
        lock (Sync)
            Console.Out.WriteLine(json);
    }

    private static ConsoleColor? ColorOf(OutputLevel level) => level switch
    {
        OutputLevel.Success => ConsoleColor.Green,
        OutputLevel.Warning => ConsoleColor.Yellow,
        OutputLevel.Error => ConsoleColor.Red,
        _ => null
    };
}

/// <summary>One line of the machine protocol (see <see cref="ToolOutput"/>). Unused members are omitted.</summary>
internal sealed record ToolEvent(string Type)
{
    /// <summary>Progress bar id, unique within one run.</summary>
    public int? Id { get; init; }

    public string? Title { get; init; }

    /// <summary>Progress between 0 and 1.</summary>
    public double? Fraction { get; init; }

    public string? Detail { get; init; }

    /// <summary><c>bar-end</c>: <c>true</c> when the bar reached 100%, <c>false</c> on failure or cancellation.</summary>
    public bool? Completed { get; init; }

    public OutputLevel? Level { get; init; }

    public string? Text { get; init; }

    /// <summary><c>done</c>: "Build" or "Update".</summary>
    public string? Operation { get; init; }

    /// <summary><c>done</c>: <c>succeeded</c>, <c>failed</c> or <c>cancelled</c>.</summary>
    public RunStatus? Status { get; init; }
}

/// <summary>Outcome of a Build/Update run; also the process exit code in <c>--run</c> mode.</summary>
internal enum RunStatus
{
    Succeeded = 0,
    Failed = 1,
    Cancelled = 2
}

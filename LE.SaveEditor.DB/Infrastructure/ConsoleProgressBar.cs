using System.Diagnostics;
using System.Globalization;

namespace LE.SaveEditor.DB.Infrastructure;

/// <summary>
/// One-line console progress bar: <c>[██████░░░░░░]  45.0%  Title - detail</c>.
/// Thread-safe, so parallel workers (icon downloads) can report into the same bar.
/// In machine mode (<see cref="ToolOutput.IsMachine"/>) the same states are sent to the editor
/// as <c>bar-start</c> / <c>bar</c> / <c>bar-end</c> JSON events instead of being drawn.
/// </summary>
internal sealed class ConsoleProgressBar : IDisposable
{
    private const int BarWidth = 30;
    private static readonly TimeSpan RenderInterval = TimeSpan.FromMilliseconds(80);
    private static int _nextId;

    private readonly Lock _sync = new();
    private readonly Stopwatch _sinceRender = Stopwatch.StartNew();
    private readonly bool _interactive = !Console.IsOutputRedirected;
    private readonly string _title;
    private readonly int _id = Interlocked.Increment(ref _nextId);

    private double _fraction;
    private string? _detail;
    private int _lastLength;
    private bool _finished;

    public ConsoleProgressBar(string title)
    {
        _title = title;
        lock (_sync)
        {
            if (ToolOutput.IsMachine)
                ToolOutput.Event(new ToolEvent("bar-start") { Id = _id, Title = _title });
            else
                Render(final: false);
        }
    }

    /// <summary>Reports <paramref name="done"/> of <paramref name="total"/> work items.</summary>
    public void Report(long done, long total, string? detail = null) =>
        Report(total <= 0 ? 0 : (double)done / total, detail);

    /// <summary>Reports progress as a fraction between 0 and 1.</summary>
    public void Report(double fraction, string? detail = null)
    {
        lock (_sync)
        {
            if (_finished)
                return;

            _fraction = Math.Clamp(fraction, 0, 1);
            _detail = detail;

            if (_sinceRender.Elapsed >= RenderInterval)
                Render(final: false);
        }
    }

    /// <summary>Draws the bar at 100% and moves to the next line.</summary>
    public void Complete(string? detail = null)
    {
        lock (_sync)
        {
            if (_finished)
                return;

            _fraction = 1;
            _detail = detail;
            Render(final: true);
            Finish(completed: true);
        }
    }

    /// <summary>Leaves the bar where it stopped (failure or cancellation) and moves to the next line.</summary>
    public void Dispose()
    {
        lock (_sync)
        {
            if (_finished)
                return;

            Render(final: true);
            Finish(completed: false);
        }
    }

    private void Finish(bool completed)
    {
        _finished = true;

        if (ToolOutput.IsMachine)
        {
            ToolOutput.Event(new ToolEvent("bar-end")
            {
                Id = _id,
                Fraction = _fraction,
                Detail = _detail,
                Completed = completed
            });
            return;
        }

        Console.WriteLine();
    }

    private void Render(bool final)
    {
        if (ToolOutput.IsMachine)
        {
            // The final state is sent by Finish as "bar-end".
            if (!final)
            {
                ToolOutput.Event(new ToolEvent("bar") { Id = _id, Fraction = _fraction, Detail = _detail });
                _sinceRender.Restart();
            }

            return;
        }

        // Redirected output (CI, logs): no carriage-return animation, only the final state.
        if (!_interactive && !final)
            return;

        var filled = (int)Math.Round(_fraction * BarWidth);
        var percent = (_fraction * 100).ToString("0.0", CultureInfo.InvariantCulture).PadLeft(5);
        var line = $"[{new string('█', filled)}{new string('░', BarWidth - filled)}] {percent}%  {_title}";
        if (!string.IsNullOrEmpty(_detail))
            line += $" - {_detail}";

        var width = ConsoleWidth();
        if (line.Length > width)
            line = line[..(width - 1)] + "…";

        if (_interactive)
        {
            // Overwrite the previous state; pad so a shorter line fully hides a longer one.
            Console.Write('\r' + line.PadRight(_lastLength));
            _lastLength = line.Length;
        }
        else
        {
            Console.Write(line);
        }

        _sinceRender.Restart();
    }

    private static int ConsoleWidth()
    {
        try
        {
            return Math.Max(40, Console.BufferWidth - 1);
        }
        catch (IOException)
        {
            return 119;
        }
    }

    public static string FormatBytes(long bytes) => bytes switch
    {
        >= 1024 * 1024 => $"{bytes / 1024d / 1024d:0.0} MB",
        >= 1024 => $"{bytes / 1024d:0.0} KB",
        _ => $"{bytes} B"
    };
}

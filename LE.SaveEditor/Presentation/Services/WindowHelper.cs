using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace LE.SaveEditor.Presentation.Services;

internal static class WindowHelper
{
    /// <summary>The currently active window, falling back to the main window. Used as dialog owner.</summary>
    public static Window? GetActiveWindow()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        return desktop.Windows.FirstOrDefault(w => w.IsActive) ?? desktop.MainWindow;
    }

    /// <summary>
    /// Shows <paramref name="dialog"/> modally over the active window and returns once it is closed, with the value
    /// it was closed with (<see cref="Window.Close(object?)"/>). The view models ask their questions synchronously
    /// ("Discard changes?"), so the UI thread keeps processing input and rendering in a nested loop meanwhile.
    /// </summary>
    public static object? ShowModal(Window dialog)
    {
        var owner = GetActiveWindow();
        if (owner is null || ReferenceEquals(owner, dialog) || !owner.IsVisible)
        {
            // No window to be modal to (startup / shutdown): show it on its own and wait until it is closed.
            var closed = new TaskCompletionSource<object?>();
            dialog.Closed += (_, _) => closed.TrySetResult(null);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            dialog.Show();
            return Wait(closed.Task);
        }

        return Wait(dialog.ShowDialog<object?>(owner));
    }

    /// <summary>
    /// Waits for <paramref name="task"/> on the UI thread without blocking it: runs a nested dispatcher loop until the
    /// task has finished. Used for native dialogs (folder picker) that are asynchronous in Avalonia.
    /// </summary>
    public static T Wait<T>(Task<T> task)
    {
        if (!task.IsCompleted)
        {
            using var done = new CancellationTokenSource();
            task.ContinueWith(
                _ => Dispatcher.UIThread.Post(() => done.Cancel()),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
            Dispatcher.UIThread.MainLoop(done.Token);
        }

        return task.GetAwaiter().GetResult();
    }
}

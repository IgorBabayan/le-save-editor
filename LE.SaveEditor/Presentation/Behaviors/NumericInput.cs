using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace LE.SaveEditor.Presentation.Behaviors;

/// <summary>
/// Attached behavior that turns a <see cref="TextBox"/> into a non-negative integer box:
/// only digits can be typed or pasted, and Up/Down arrow keys change the value by one.
/// </summary>
public static class NumericInput
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<TextBox, bool>("IsEnabled", typeof(NumericInput));

    /// <summary>Upper limit used by the Up arrow key.</summary>
    public static readonly AttachedProperty<int> MaximumProperty =
        AvaloniaProperty.RegisterAttached<TextBox, int>("Maximum", typeof(NumericInput), int.MaxValue);

    static NumericInput()
    {
        IsEnabledProperty.Changed.AddClassHandler<TextBox>(OnIsEnabledChanged);
    }

    public static bool GetIsEnabled(AvaloniaObject element) => element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(AvaloniaObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    public static int GetMaximum(AvaloniaObject element) => element.GetValue(MaximumProperty);

    public static void SetMaximum(AvaloniaObject element, int value) => element.SetValue(MaximumProperty, value);

    private static void OnIsEnabledChanged(TextBox textBox, AvaloniaPropertyChangedEventArgs e)
    {
        textBox.RemoveHandler(InputElement.TextInputEvent, OnTextInput);
        textBox.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        textBox.RemoveHandler(TextBox.PastingFromClipboardEvent, OnPasting);

        if (e.NewValue is true)
        {
            textBox.AddHandler(InputElement.TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
            textBox.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
            textBox.AddHandler(TextBox.PastingFromClipboardEvent, OnPasting, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            InputMethod.SetIsInputMethodEnabled(textBox, false);
        }
        else
        {
            textBox.ClearValue(InputMethod.IsInputMethodEnabledProperty);
        }
    }

    private static void OnTextInput(object? sender, TextInputEventArgs e) =>
        e.Handled = !IsDigits(e.Text);

    private static void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var textBox = (TextBox)sender!;

        switch (e.Key)
        {
            case Key.Space:
                e.Handled = true;
                break;

            case Key.Up or Key.Down:
                var current = int.TryParse(textBox.Text, out var value) ? value : 0;
                var next = Math.Clamp(e.Key == Key.Up ? current + 1 : current - 1, 0, GetMaximum(textBox));
                textBox.Text = next.ToString(CultureInfo.InvariantCulture);
                textBox.CaretIndex = textBox.Text.Length;
                e.Handled = true;
                break;
        }
    }

    /// <summary>
    /// The clipboard is read asynchronously in Avalonia, so the paste is taken over: digits-only text replaces the
    /// selection, anything else is ignored.
    /// </summary>
    private static async void OnPasting(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox textBox || TopLevel.GetTopLevel(textBox)?.Clipboard is not { } clipboard)
            return;

        e.Handled = true;
        string? text;
        try
        {
            text = (await clipboard.GetTextAsync())?.Trim();
        }
        catch (Exception)
        {
            return;
        }

        if (text is null || !IsDigits(text))
            return;

        var current = textBox.Text ?? string.Empty;
        var start = Math.Min(textBox.SelectionStart, textBox.SelectionEnd);
        var end = Math.Max(textBox.SelectionStart, textBox.SelectionEnd);
        start = Math.Clamp(start, 0, current.Length);
        end = Math.Clamp(end, start, current.Length);
        textBox.Text = current[..start] + text + current[end..];
        textBox.CaretIndex = start + text.Length;
    }

    private static bool IsDigits(string? text) => !string.IsNullOrEmpty(text) && text.All(char.IsAsciiDigit);
}

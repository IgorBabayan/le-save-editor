using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace LE.SaveEditor.Presentation.Behaviors;

/// <summary>
/// Attached behavior for a single-selection <see cref="ListBox"/>: pressing the left mouse button on a row selects
/// exactly that row and nothing else.
/// </summary>
/// <remarks>
/// When selecting a row changes the layout of the list (badges appear or disappear, rows change their height),
/// another row may slide under the cursor before the press is processed. Handling the press here selects the clicked
/// row right away; the release still bubbles as usual.
/// </remarks>
public static class SelectOnClick
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("IsEnabled", typeof(SelectOnClick));

    static SelectOnClick()
    {
        IsEnabledProperty.Changed.AddClassHandler<Control>(OnIsEnabledChanged);
    }

    public static bool GetIsEnabled(AvaloniaObject element) => element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(AvaloniaObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        // Only lists select rows; on anything else (e.g. a TextBox in a list row) the property does nothing.
        if (control is not ListBox listBox)
            return;

        listBox.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        if (e.NewValue is true)
            listBox.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
    }

    private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not ListBox listBox
            || !e.GetCurrentPoint(listBox).Properties.IsLeftButtonPressed
            || e.Source is not Visual source)
            return;

        var container = source as ListBoxItem ?? source.FindAncestorOfType<ListBoxItem>();
        if (container is null || !container.IsEnabled || container.FindAncestorOfType<ListBox>() != listBox)
            return;

        // Read the row's item before anything changes: selecting it may re-layout or recycle the containers.
        var index = listBox.IndexFromContainer(container);
        if (index < 0)
            return;

        container.Focus(NavigationMethod.Pointer);
        listBox.SelectedIndex = index;
        e.Handled = true;
    }
}

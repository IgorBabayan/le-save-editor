using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LE.SaveEditor.Presentation.Behaviors;

/// <summary>
/// Attached behavior for a single-selection <see cref="ListBox"/>: pressing the left mouse button on a row selects
/// exactly that row and nothing else.
/// </summary>
/// <remarks>
/// A plain <see cref="ListBox"/> captures the mouse on press and selects every row that comes under the pressed
/// mouse ("drag selection"). When selecting a row changes the layout of the list (badges appear or disappear, rows
/// change their height), another row slides under the cursor and gets selected instead of the clicked one.
/// Handling the press here selects the clicked row without capturing the mouse; the release still bubbles as usual.
/// </remarks>
public static class SelectOnClick
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(SelectOnClick), new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ListBox listBox)
            return;

        listBox.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        if ((bool)e.NewValue)
            listBox.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBox listBox
            || e.OriginalSource is not DependencyObject source
            || ItemsControl.ContainerFromElement(listBox, source) is not ListBoxItem container
            || !container.IsEnabled)
            return;

        // Read the row's item before anything changes: selecting it may re-layout or recycle the containers.
        var item = listBox.ItemContainerGenerator.ItemFromContainer(container);
        if (ReferenceEquals(item, DependencyProperty.UnsetValue))
            return;

        container.Focus();
        listBox.SelectedItem = item;
        e.Handled = true;
    }
}

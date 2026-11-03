using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LE.SaveEditor.Presentation.ViewModels.Blessings;

namespace LE.SaveEditor.Presentation.Views.Blessings;

/// <summary>"Blessings" section of the inventory: the blessing of every timeline echo.</summary>
public partial class BlessingsView : UserControl
{
    public BlessingsView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// An echo picked in the list. The lists bind their selection one way: when the inventory switches to another
    /// character the list gets new items, and a two-way binding could write that reset (no selection) into the new view model.
    /// </summary>
    private void EchoList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox { DataContext: BlessingsViewModel blessings }
            && e.AddedItems.Count > 0
            && e.AddedItems[0] is BlessingEchoViewModel echo
            && blessings.Echoes.Contains(echo))
            blessings.SelectedEcho = echo;
    }

    /// <summary>A blessing picked for the selected echo (only one of that echo's own options is taken).</summary>
    private void OptionList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox { DataContext: BlessingEchoViewModel echo }
            && e.AddedItems.Count > 0
            && e.AddedItems[0] is BlessingOptionViewModel option
            && echo.Options.Contains(option))
            echo.Selected = option;
    }

    /// <summary>
    /// The lists show all their rows inside the inventory's scroll viewer; their own scroll viewers would swallow the
    /// mouse wheel, so it is passed on to the page.
    /// </summary>
    private void List_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled || sender is not UIElement list)
            return;

        e.Handled = true;
        var forwarded = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = MouseWheelEvent,
            Source = list
        };
        (VisualTreeHelper.GetParent(list) as UIElement)?.RaiseEvent(forwarded);
    }
}

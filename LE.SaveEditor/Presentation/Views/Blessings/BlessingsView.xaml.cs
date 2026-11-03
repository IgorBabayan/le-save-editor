using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace LE.SaveEditor.Presentation.Views.Blessings;

/// <summary>"Blessings" section of the inventory: the blessing of every timeline echo.</summary>
public partial class BlessingsView : UserControl
{
    public BlessingsView()
    {
        InitializeComponent();
    }

    /// <summary>Blessing groups the user collapsed; kept while switching echoes, so a collapsed group stays collapsed.</summary>
    private readonly HashSet<string> _collapsedGroups = new(StringComparer.Ordinal);

    /// <summary>A group header appeared (another echo was selected): restores whether the group was collapsed.</summary>
    private void GroupToggle_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton { Tag: string group } toggle)
            toggle.IsChecked = !_collapsedGroups.Contains(group);
    }

    /// <summary>A group header was clicked: remembers the group's new state.</summary>
    private void GroupToggle_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { Tag: string group } toggle)
            return;

        if (toggle.IsChecked == true)
            _collapsedGroups.Remove(group);
        else
            _collapsedGroups.Add(group);
    }
}

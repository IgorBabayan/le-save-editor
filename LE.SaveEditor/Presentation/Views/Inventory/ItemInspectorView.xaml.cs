using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using LE.SaveEditor.Presentation.ViewModels.Inventory;

namespace LE.SaveEditor.Presentation.Views.Inventory;

/// <summary>
/// Compact item builder shown next to an equipment slot (see <see cref="ItemInspectorAdorner"/>): an object
/// inspector over <see cref="ItemBuilderViewModel"/> with expandable rows instead of the full builder window.
/// Which rows are expanded is view state and lives here.
/// </summary>
public partial class ItemInspectorView : UserControl
{
    private readonly ItemBuilderViewModel _viewModel;

    public ItemInspectorView(ItemBuilderViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _viewModel = viewModel;

        // An empty slot starts with the item list open; an equipped item starts with it closed.
        ItemToggle.IsChecked = !viewModel.HasSelectedBase && viewModel.IsDatabaseAvailable;

        // Loaded / Unloaded pair up: the inspector is taken off screen while the full builder window is open.
        Loaded += (_, _) => viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Unloaded += (_, _) => viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    /// <summary>The "open the full item builder" button was clicked.</summary>
    public event EventHandler? ExpandRequested;

    /// <summary>Puts the keyboard focus into the inspector: the item search when the list is open.</summary>
    public void FocusFirst()
    {
        if (ItemToggle.IsChecked == true && ItemSearch.IsEnabled)
            ItemSearch.Focus();
        else
            Focus();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Another list (bases ↔ uniques ↔ set items) cleared the selection: show the new list.
        if (e.PropertyName == nameof(ItemBuilderViewModel.ListKind) && !_viewModel.HasSelectedBase)
            ItemToggle.IsChecked = true;
    }

    private void OnExpandClick(object sender, RoutedEventArgs e) => ExpandRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>A rarity was picked: fold the pills away like a closed drop-down.</summary>
    private void OnRarityPillClick(object sender, RoutedEventArgs e) => RarityToggle.IsChecked = false;

    /// <summary>Clicking an item selects it (on mouse down) and folds the list away.</summary>
    private void OnItemListPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject source
            && ItemsControl.ContainerFromElement(ItemList, source) is ListBoxItem)
            CollapseItemList();
    }

    private void OnItemListKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && ItemList.SelectedItem is not null)
        {
            CollapseItemList();
            e.Handled = true;
        }
    }

    private void CollapseItemList()
    {
        // After the click is done, so the list is not hidden under the mouse mid-gesture.
        Dispatcher.InvokeAsync(() =>
        {
            if (_viewModel.HasSelectedBase)
                ItemToggle.IsChecked = false;
        }, DispatcherPriority.Input);
    }

    /// <summary>The affix picker opened under a slot: type to filter right away.</summary>
    private void OnAffixSearchLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox box)
            Dispatcher.InvokeAsync(() => box.Focus(), DispatcherPriority.Input);
    }
}

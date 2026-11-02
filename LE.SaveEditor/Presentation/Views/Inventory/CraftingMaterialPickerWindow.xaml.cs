using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using LE.SaveEditor.Presentation.ViewModels.Inventory;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.Views.Inventory;

/// <summary>
/// Picks a rune, a glyph or an affix shard for the backpack. <see cref="Result"/> is the picked material, or
/// <c>null</c> when cancelled. The search box takes the focus; Down moves into the list, Enter or a double click applies.
/// </summary>
public partial class CraftingMaterialPickerWindow : Window
{
    private readonly CraftingMaterialPickerViewModel _viewModel;

    public CraftingMaterialPickerWindow(CraftingMaterialPickerViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _viewModel = viewModel;

        _viewModel.Applied += OnApplied;
        Closed += (_, _) => _viewModel.Applied -= OnApplied;
        Loaded += OnLoaded;
    }

    public CraftingMaterial? Result { get; private set; }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        SearchBox.Focus();
        // The edited material is selected: show it.
        if (_viewModel.SelectedOption is { } selected)
            Dispatcher.InvokeAsync(() => MaterialList.ScrollIntoView(selected), DispatcherPriority.Background);
    }

    private void OnApplied(object? sender, EventArgs e)
    {
        Result = _viewModel.Result;
        if (IsVisible)
            DialogResult = Result is not null;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => DialogResult = false;

    private void OnMaterialListMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Only a double click on a row applies, not one on the scroll bar.
        if (ItemsControl.ContainerFromElement(MaterialList, e.OriginalSource as DependencyObject) is not ListBoxItem)
            return;

        Apply();
        e.Handled = true;
    }

    private void OnMaterialListKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        Apply();
        e.Handled = true;
    }

    /// <summary>Down in the search box moves to the selected (or first) row of the list.</summary>
    private void OnSearchBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Down || MaterialList.Items.Count == 0)
            return;

        if (MaterialList.SelectedIndex < 0)
            MaterialList.SelectedIndex = 0;

        MaterialList.ScrollIntoView(MaterialList.SelectedItem);
        if (MaterialList.ItemContainerGenerator.ContainerFromIndex(MaterialList.SelectedIndex) is ListBoxItem row)
            row.Focus();
        e.Handled = true;
    }

    private void Apply()
    {
        if (_viewModel.ApplyCommand.CanExecute(null))
            _viewModel.ApplyCommand.Execute(null);
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LE.SaveEditor.Presentation.ViewModels.Inventory;
using LE.SaveEditor.Services.GameData.Items;

namespace LE.SaveEditor.Presentation.Views.Inventory;

/// <summary>
/// Picks a material of one kind (a rune, a glyph or an affix shard) for the backpack. <see cref="Result"/> is the picked material, or
/// <c>null</c> when cancelled. The search box takes the focus; Down moves into the list, Enter or a double click applies.
/// </summary>
public partial class CraftingMaterialPickerWindow : ChromeWindow
{
    private readonly CraftingMaterialPickerViewModel? _viewModel;

    /// <summary>For the XAML previewer.</summary>
    public CraftingMaterialPickerWindow()
    {
        InitializeComponent();
    }

    public CraftingMaterialPickerWindow(CraftingMaterialPickerViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _viewModel = viewModel;

        _viewModel.Applied += OnApplied;
        Closed += (_, _) => _viewModel.Applied -= OnApplied;
        Loaded += OnLoaded;

        SearchBox.AddHandler(KeyDownEvent, OnSearchBoxPreviewKeyDown, RoutingStrategies.Tunnel);
        MaterialList.DoubleTapped += OnMaterialListDoubleTapped;
        MaterialList.KeyDown += OnMaterialListKeyDown;
    }

    public CraftingMaterial? Result { get; private set; }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        SearchBox.Focus();
        // The edited material is selected: show it.
        if (_viewModel?.SelectedOption is { } selected)
            Dispatcher.UIThread.Post(() => MaterialList.ScrollIntoView(selected), DispatcherPriority.Background);
    }

    private void OnApplied(object? sender, EventArgs e)
    {
        Result = _viewModel?.Result;
        if (IsVisible)
            Close(Result is not null);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close(false);

    private void OnMaterialListDoubleTapped(object? sender, TappedEventArgs e)
    {
        // Only a double click on a row applies, not one on the scroll bar.
        if ((e.Source as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true) is null)
            return;

        Apply();
        e.Handled = true;
    }

    private void OnMaterialListKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        Apply();
        e.Handled = true;
    }

    /// <summary>Down in the search box moves to the selected (or first) row of the list.</summary>
    private void OnSearchBoxPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Down || MaterialList.ItemCount == 0)
            return;

        if (MaterialList.SelectedIndex < 0)
            MaterialList.SelectedIndex = 0;
        if (MaterialList.SelectedItem is { } selected)
            MaterialList.ScrollIntoView(selected);
        MaterialList.ContainerFromIndex(MaterialList.SelectedIndex)?.Focus(NavigationMethod.Directional);
        e.Handled = true;
    }

    private void Apply()
    {
        if (_viewModel?.ApplyCommand.CanExecute(null) == true)
            _viewModel.ApplyCommand.Execute(null);
    }
}

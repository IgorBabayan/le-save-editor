using System.ComponentModel;
using Avalonia.Interactivity;
using LE.SaveEditor.Presentation.ViewModels.Inventory;

namespace LE.SaveEditor.Presentation.Views.Inventory;

/// <summary>
/// Edits the amounts of one kind of crafting material in a stash's material storage. <see cref="Result"/> holds the
/// amounts to store by sub type, or <c>null</c> when cancelled.
/// </summary>
public partial class CraftingMaterialStorageWindow : ChromeWindow
{
    private readonly CraftingMaterialStorageViewModel? _viewModel;

    /// <summary>For the XAML previewer.</summary>
    public CraftingMaterialStorageWindow()
    {
        InitializeComponent();
    }

    public CraftingMaterialStorageWindow(CraftingMaterialStorageViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _viewModel = viewModel;

        _viewModel.Applied += OnApplied;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Closed += (_, _) =>
        {
            _viewModel.Applied -= OnApplied;
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        };
    }

    public IReadOnlyDictionary<int, int>? Result { get; private set; }

    private void OnApplied(object? sender, EventArgs e)
    {
        Result = _viewModel?.Result;
        if (IsVisible)
            Close(Result is not null);
    }

    /// <summary>A material that was just added (or picked again) is scrolled into view.</summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CraftingMaterialStorageViewModel.SelectedEntry) && _viewModel?.SelectedEntry is { } entry)
            EntryList.ScrollIntoView(entry);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e) => Close(false);
}

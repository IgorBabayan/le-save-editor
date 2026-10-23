using System.Windows;
using LE.SaveEditor.Presentation.ViewModels.Inventory;

namespace LE.SaveEditor.Presentation.Views.Inventory;

/// <summary>Item builder popup of an equipment slot. <see cref="Result"/> is the built item, or <c>null</c> when cancelled.</summary>
public partial class ItemBuilderWindow : Window
{
    private readonly ItemBuilderViewModel _viewModel;

    public ItemBuilderWindow(ItemBuilderViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _viewModel = viewModel;

        _viewModel.CloseRequested += OnCloseRequested;
        Closed += (_, _) => _viewModel.CloseRequested -= OnCloseRequested;
    }

    public BuiltItem? Result { get; private set; }

    private void OnCloseRequested(object? sender, BuiltItem? item)
    {
        Result = item;
        // Setting DialogResult on a modal window closes it.
        if (IsVisible)
            DialogResult = item is not null;
    }
}

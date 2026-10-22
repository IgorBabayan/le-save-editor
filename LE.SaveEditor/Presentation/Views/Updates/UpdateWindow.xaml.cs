using System.ComponentModel;
using System.Windows;
using LE.SaveEditor.Presentation.ViewModels.Updates;

namespace LE.SaveEditor.Presentation.Views.Updates;

/// <summary>
/// Interaction logic for UpdateWindow.xaml
/// </summary>
public partial class UpdateWindow : Window
{
    public UpdateWindow(UpdateViewModel viewModel)
    {
        InitializeComponent();
        DataContext = ViewModel = viewModel;
    }

    public UpdateViewModel ViewModel { get; }

    protected override void OnClosing(CancelEventArgs e)
    {
        // Closing the window (×, Alt+F4) means "skip this time".
        ViewModel.OnClosing();
        base.OnClosing(e);
    }
}

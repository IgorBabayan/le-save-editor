using LE.SaveEditor.Presentation.Services;

namespace LE.SaveEditor.Presentation.ViewModels.Shell;

public partial class MainViewModel(IDialogService dialogService) : ViewModelBase
{
    [RelayCommand]
    private void OpenSettings() => dialogService.ShowSettings();
}

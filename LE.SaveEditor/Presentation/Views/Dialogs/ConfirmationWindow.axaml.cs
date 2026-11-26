using Avalonia.Interactivity;

namespace LE.SaveEditor.Presentation.Views.Dialogs;

/// <summary>Themed Yes/No question. Closes with <c>true</c> for Yes, <c>false</c> for No.</summary>
public partial class ConfirmationWindow : ChromeWindow
{
    /// <summary>For the XAML previewer.</summary>
    public ConfirmationWindow() : this("Confirm", string.Empty)
    {
    }

    public ConfirmationWindow(string title, string message)
    {
        InitializeComponent();
        Title = title;
        MessageText.Text = message;
    }

    private void OnYesClick(object? sender, RoutedEventArgs e) => Close(true);

    private void OnNoClick(object? sender, RoutedEventArgs e) => Close(false);
}

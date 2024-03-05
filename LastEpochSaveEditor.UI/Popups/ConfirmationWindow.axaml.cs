namespace LastEpochSaveEditor.Popups;

public partial class ConfirmationWindow : Window, IConfirmationView
{
	public ConfirmationWindow() => InitializeComponent();

	public WindowBase? OwnerWindow { get; set; }

	async Task IView.CloseDialog() => await Task.Run(() => Hide());

	async Task IView.ShowDialog() => await ShowDialog(null);
}
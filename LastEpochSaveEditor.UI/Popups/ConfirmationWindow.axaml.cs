namespace LastEpochSaveEditor.Popups;

public partial class ConfirmationWindow : Window, IConfirmationView
{
	public ConfirmationWindow() => InitializeComponent();

	public WindowBase? OwnerWindow { get; set; }

	void IView.CloseDialog() => Hide();

	async Task IView.ShowDialog() => await ShowDialog(null);
}
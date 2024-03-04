namespace LastEpochSaveEditor.Popups;

public partial class ConfirmationWindow : Window, IConfirmationView
{
	public ConfirmationWindow() => InitializeComponent();

	async Task IView.CloseDialog() => await Application.Current.Dispatcher.InvokeAsync(() => Hide());

	async Task IView.ShowDialog() => await Application.Current.Dispatcher.InvokeAsync(() => ShowDialog());
}

namespace LastEpochSaveEditor.Popups;

public partial class ErrorWindow : Window, IErrorView
{
	public ErrorWindow() => InitializeComponent();

	void IView.CloseDialog() => Hide();

	async Task IView.ShowDialog() => throw new NotImplementedException();

	/*async Task IView.CloseDialog() => await Application.Current.Dispatcher.InvokeAsync(() => Hide());

	async Task IView.ShowDialog() => await Application.Current.Dispatcher.InvokeAsync(() => ShowDialog());*/
}
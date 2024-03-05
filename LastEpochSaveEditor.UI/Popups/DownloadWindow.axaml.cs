namespace LastEpochSaveEditor.Popups;

public partial class DownloadWindow : Window, IDownloadView
{
	public DownloadWindow() => InitializeComponent();

	async Task IView.CloseDialog() => throw new NotImplementedException();

	async Task IView.ShowDialog() => throw new NotImplementedException();

	/*async Task IView.CloseDialog() => await Application.Current.Dispatcher.InvokeAsync(() => Hide());

	async Task IView.ShowDialog() => await Application.Current.Dispatcher.InvokeAsync(() => ShowDialog());*/
}
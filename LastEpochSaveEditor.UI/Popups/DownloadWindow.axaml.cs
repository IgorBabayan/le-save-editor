namespace LastEpochSaveEditor.Popups;

public partial class DownloadWindow : Window, IDownloadView
{
	public DownloadWindow() => InitializeComponent();

	/*async Task IView.CloseDialog() => throw new NotImplementedException();

	async Task IView.ShowDialog() => throw new NotImplementedException();*/

	void IView.CloseDialog() =>  Hide();

	async Task IView.ShowDialog() => await ShowDialog(App.GetMainWindow());
}
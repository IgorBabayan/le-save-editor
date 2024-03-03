namespace LastEpochSaveEditor.Popups;

public partial class DownloadWindow : IDownloadView
{
	public DownloadWindow() => InitializeComponent();

	public async Task ShowDialog() => await Task.Run(() => Application.Current.Dispatcher.Invoke(() =>
		((MainWindow)App.Current.MainWindow).MainGrid.Children.Add(this)));
}
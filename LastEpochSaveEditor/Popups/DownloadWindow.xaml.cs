namespace LastEpochSaveEditor.Popups;

public partial class DownloadWindow
{
	public DownloadWindow()
	{
		InitializeComponent();
		DataContext = App.GetService<DownloadViewModel>();
	}
}
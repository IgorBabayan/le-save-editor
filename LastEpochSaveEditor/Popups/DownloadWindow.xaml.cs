using LastEpochSaveEditor.ViewModels;
using System.Windows.Controls;

namespace LastEpochSaveEditor.Popups
{
	public partial class DownloadWindow : UserControl
	{
		public DownloadWindow()
		{
			InitializeComponent();
			DataContext = App.GetService<DownloadViewModel>();
		}

		private void OnCloseClick(object sender, System.Windows.RoutedEventArgs e)
		{
			var grid = ((MainWindow)App.Current.MainWindow).MainGrid;
			grid.Children.Remove(this);
		}
	}
}

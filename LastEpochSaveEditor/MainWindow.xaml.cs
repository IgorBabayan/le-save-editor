using LastEpochSaveEditor.Utils;
using System.Windows;
using System.Windows.Input;

namespace LastEpochSaveEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			AppTheme.ChangeTheme(false);

			SaveFileLoader.Load();
		}

		private void OnMinimizeClick(object sender, RoutedEventArgs e) => Application.Current.MainWindow.WindowState = WindowState.Minimized;

		private void OnCloseClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

		private void OnMaximizedClick(object sender, RoutedEventArgs e)
		{
			MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
			Application.Current.MainWindow.WindowState = Application.Current.MainWindow.WindowState != WindowState.Maximized
				? WindowState.Maximized
				: WindowState.Normal;
		}

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				DragMove();
		}
	}
}

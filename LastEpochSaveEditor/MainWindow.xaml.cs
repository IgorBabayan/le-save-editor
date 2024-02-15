using System.Windows;
using System.Windows.Input;

namespace LastEpochSaveEditor
{

	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
#if DEBUG
			ResizeMode = ResizeMode.CanResize;
#else
			minimizeButton.Visibility = Visibility.Collapsed;
#endif
		}

		private void MinimizeOrMaximizeWindow()
		{
			MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
			Application.Current.MainWindow.WindowState = Application.Current.MainWindow.WindowState != WindowState.Maximized
				? WindowState.Maximized
				: WindowState.Normal;
		}

		private void OnMinimizeClick(object sender, RoutedEventArgs e) => Application.Current.MainWindow.WindowState = WindowState.Minimized;

		private void OnCloseClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

		private void OnMaximizedClick(object sender, RoutedEventArgs e) => MinimizeOrMaximizeWindow();

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				DragMove();
		}
	}
}

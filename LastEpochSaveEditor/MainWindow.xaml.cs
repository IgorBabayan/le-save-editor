using LastEpochSaveEditor.Utils;
using LastEpochSaveEditor.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace LastEpochSaveEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow(MainViewModel dataContext)
		{
			InitializeComponent();
			AppTheme.ChangeTheme(false);

			DataContext = dataContext;
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

		private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
				MinimizeOrMaximizeWindow();

		}
    }
}

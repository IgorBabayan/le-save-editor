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

#if DEBUG
			ResizeMode = ResizeMode.CanResize;
#else
			minimizeButton.Visibility = Visibility.Hidden;
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

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			btn1.IsDefault = true;
			btn2.IsDefault = false;
			btn3.IsDefault = false;
			btn4.IsDefault = false;
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			btn1.IsDefault = false;
			btn2.IsDefault = true;
			btn3.IsDefault = false;
			btn4.IsDefault = false;
		}

		private void Button_Click_3(object sender, RoutedEventArgs e)
		{
			btn1.IsDefault = false;
			btn2.IsDefault = false;
			btn3.IsDefault = true;
			btn4.IsDefault = false;
		}

		private void Button_Click_4(object sender, RoutedEventArgs e)
		{
			btn1.IsDefault = false;
			btn2.IsDefault = false;
			btn3.IsDefault = false;
			btn4.IsDefault = true;
		}
	}
}

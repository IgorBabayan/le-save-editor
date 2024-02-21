namespace LastEpochSaveEditor
{
	public partial class MainWindow : Window
	{
		public MainWindow() => InitializeComponent();

		private void MinimizeOrMaximizeWindow()
		{
			MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
			Application.Current.MainWindow.WindowState = Application.Current.MainWindow.WindowState != WindowState.Maximized
				? WindowState.Maximized
				: WindowState.Normal;
		}

		private void OnMinimizeClick(object sender, RoutedEventArgs e) => Application.Current.MainWindow.WindowState = WindowState.Minimized;

		private void OnCloseClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				DragMove();
		}

		private void OnDownloadClick(object sender, RoutedEventArgs e)
		{
			var popup = App.GetService<DownloadWindow>();
			MainGrid.Children.Add(popup);
        }
    }
}

﻿namespace LastEpochSaveEditor
{
	public partial class MainWindow : Window
	{
		public MainWindow() => InitializeComponent();

		private void OnMinimizeClick(object sender, RoutedEventArgs e) => Application.Current.MainWindow.WindowState = WindowState.Minimized;

		private void OnCloseClick(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				DragMove();
		}
    }
}

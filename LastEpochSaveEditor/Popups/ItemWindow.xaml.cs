namespace LastEpochSaveEditor.Popups
{
	public partial class ItemWindow : UserControl
    {
		public ItemWindow()
		{
			InitializeComponent();
			DataContext = App.GetService<ItemViewModel>();
		}

		private void OnCloseClick(object sender, RoutedEventArgs e)
		{
			var grid = ((MainWindow)App.Current.MainWindow).MainGrid;
			grid.Children.Remove(this);
		}
    }
}

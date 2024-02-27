namespace LastEpochSaveEditor.Popups;

public partial class ItemWindow
{
	public ItemWindow()
	{
		InitializeComponent();
		DataContext = App.GetService<ItemViewModel>();
	}
}
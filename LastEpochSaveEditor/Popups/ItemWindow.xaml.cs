namespace LastEpochSaveEditor.Popups;

public partial class ItemWindow
{
	public ItemWindow()
	{
		InitializeComponent();
		DataContext = App.GetService<ItemViewModel>();
	}

	public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
		nameof(IsOpen), typeof(bool), typeof(ItemWindow), new PropertyMetadata(default(bool)));

	public bool IsOpen
	{
		get => (bool)GetValue(IsOpenProperty);
		set => SetValue(IsOpenProperty, value);
	}
}
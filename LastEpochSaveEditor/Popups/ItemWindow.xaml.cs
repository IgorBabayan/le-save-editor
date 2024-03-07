namespace LastEpochSaveEditor.Popups;

public partial class ItemWindow
{
	public ItemWindow()
	{
		InitializeComponent();
		DataContext = App.GetService<ItemViewModel>();
	}

	public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(
		nameof(Item), typeof(ItemDataInfo), typeof(ItemWindow), new PropertyMetadata(default(ItemDataInfo)));

	public ItemDataInfo Item
	{
		get => (ItemDataInfo)GetValue(ItemProperty);
		set => SetValue(ItemProperty, value);
	}

	public static readonly DependencyProperty PopupIdProperty = DependencyProperty.Register(
		nameof(PopupId), typeof(Guid), typeof(ItemWindow), new PropertyMetadata(default(Guid)));

	public Guid PopupId
	{
		get => (Guid)GetValue(PopupIdProperty);
		set => SetValue(PopupIdProperty, value);
	}
}
namespace LastEpochSaveEditor.Controls;

public partial class ItemImage
{
	public ItemImage() => InitializeComponent();

	public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(
		nameof(Item), typeof(ItemDataInfo), typeof(ItemImage), new PropertyMetadata(default(ItemDataInfo)));

	public ItemDataInfo Item
	{
		get => (ItemDataInfo)GetValue(ItemProperty);
		set => SetValue(ItemProperty, value);
	}
}
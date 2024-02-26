namespace LastEpochSaveEditor.Controls;

public partial class ItemButton
{
    public ItemButton() => InitializeComponent();

    public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(
	    nameof(Item), typeof(ItemDataInfo), typeof(ItemButton), new PropertyMetadata(default(ItemDataInfo)));

    public ItemDataInfo Item
    {
	    get => (ItemDataInfo)GetValue(ItemProperty);
	    set => SetValue(ItemProperty, value);
    }
}
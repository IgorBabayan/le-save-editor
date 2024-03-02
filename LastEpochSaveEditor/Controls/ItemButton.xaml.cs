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

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
	    nameof(Command), typeof(ICommand), typeof(ItemButton), new PropertyMetadata(default(ICommand)));

    public ICommand Command
    {
	    get => (ICommand)GetValue(CommandProperty);
	    set => SetValue(CommandProperty, value);
    }
}
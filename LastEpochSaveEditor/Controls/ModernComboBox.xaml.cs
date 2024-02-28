namespace LastEpochSaveEditor.Controls;

public partial class ModernComboBox
{
	public ModernComboBox() => InitializeComponent();

    public IEnumerable ItemsSource
	{
		get => (IEnumerable)GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	public static readonly DependencyProperty ItemsSourceProperty =
		DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ModernComboBox), new PropertyMetadata(default(IEnumerable)));
}

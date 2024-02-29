namespace LastEpochSaveEditor.Controls;

public partial class ModernComboBox : UserControl
{
	public ModernComboBox() => InitializeComponent();

	public IEnumerable ItemsSource
	{
		get => (IEnumerable)GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}

	public static readonly DependencyProperty ItemsSourceProperty =
		DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ModernComboBox), new PropertyMetadata(default(IEnumerable)));

	public object ButtonContent
	{
		get => GetValue(ButtonContentProperty);
		set => SetValue(ButtonContentProperty, value);
	}

	public static readonly DependencyProperty ButtonContentProperty =
		DependencyProperty.Register(nameof(ButtonContent), typeof(object), typeof(ModernComboBox), new PropertyMetadata(default(object)));

	public object SelectedItem
	{
		get => GetValue(SelectedItemProperty);
		set => SetValue(SelectedItemProperty, value);
	}

	public static readonly DependencyProperty SelectedItemProperty =
		DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(ModernComboBox), new PropertyMetadata(default(object)));

	public string DisplayMemberPath
	{
		get => (string)GetValue(DisplayMemberPathProperty);
		set => SetValue(DisplayMemberPathProperty, value);
	}

	public static readonly DependencyProperty DisplayMemberPathProperty =
		DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(ModernComboBox), new PropertyMetadata(default(string)));

	public object ItemsContent
	{
		get => GetValue(ItemsContentProperty);
		set => SetValue(ItemsContentProperty, value);
	}

	public static readonly DependencyProperty ItemsContentProperty =
		DependencyProperty.Register(nameof(ItemsContent), typeof(object), typeof(ModernComboBox), new PropertyMetadata(default(object)));

	public bool IsChecked
	{
		get => (bool)GetValue(IsCheckedProperty);
		set => SetValue(IsCheckedProperty, value);
	}

	public static readonly DependencyProperty IsCheckedProperty =
		DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(ModernComboBox), new PropertyMetadata(default(bool)));
}

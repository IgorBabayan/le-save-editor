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

	public object SelectedItem
	{
		get => GetValue(SelectedItemProperty);
		set => SetValue(SelectedItemProperty, value);
	}

	public static readonly DependencyProperty SelectedItemProperty =
		DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(ModernComboBox), new PropertyMetadata(default(object), OnSelectedItemChanged));

	private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
	{
		if (args.OldValue == null)
			return;
		
		var comboBox = (sender as ModernComboBox)!;
		if (comboBox.GetTemplateChild("PART_MainButton") is ToggleButton button)
			button.IsChecked = false;
	}

	private void OnPopupClosed(object? sender, EventArgs _)
	{
		
	}
}

namespace LastEpochSaveEditor.Controls;

public partial class CloseButton
{
    public CloseButton() => InitializeComponent();

    public static readonly DependencyProperty PopupTooltipProperty = DependencyProperty.Register(
        nameof(PopupTooltip), typeof(string), typeof(CloseButton), new PropertyMetadata(default(string)));

    public string PopupTooltip
    {
        get => (string)GetValue(PopupTooltipProperty);
        set => SetValue(PopupTooltipProperty, value);
    }

    public static readonly DependencyProperty PopupOffsetProperty = DependencyProperty.Register(
	    nameof(PopupOffset), typeof(int), typeof(CloseButton), new PropertyMetadata(default(int)));

    public int PopupOffset
    {
	    get => (int)GetValue(PopupOffsetProperty);
	    set => SetValue(PopupOffsetProperty, value);
    }
}
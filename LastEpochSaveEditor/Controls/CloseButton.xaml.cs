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
}
namespace LastEpochSaveEditor.Controls;

public partial class DockButton
{
    public DockButton() => InitializeComponent();

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(DrawingGroup), typeof(DockButton), new PropertyMetadata(default(DrawingGroup)));

    public DrawingGroup Icon
    {
        get => (DrawingGroup)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public static readonly DependencyProperty PopupTooltipProperty = DependencyProperty.Register(
        nameof(PopupTooltip), typeof(string), typeof(DockButton), new PropertyMetadata(default(string)));

    public string PopupTooltip
    {
        get => (string)GetValue(PopupTooltipProperty);
        set => SetValue(PopupTooltipProperty, value);
    }
}
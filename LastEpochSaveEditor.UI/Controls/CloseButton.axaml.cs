namespace LastEpochSaveEditor.Controls;

public class CloseButton : TemplatedControl
{
    public static readonly StyledProperty<string> PopupTooltipProperty =
        AvaloniaProperty.Register<CloseButton, string>(nameof(PopupTooltip));

    public string PopupTooltip
    {
        get => GetValue(PopupTooltipProperty);
        set => SetValue(PopupTooltipProperty, value);
    }

    public static readonly StyledProperty<ICommand?> CommandProperty = 
        AvaloniaProperty.Register<CloseButton, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
}
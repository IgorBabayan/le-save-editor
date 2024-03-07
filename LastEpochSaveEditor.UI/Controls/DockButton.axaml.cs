namespace LastEpochSaveEditor.Controls;

public class DockButton : TemplatedControl
{
    public static readonly StyledProperty<IImage?> IconProperty = 
        AvaloniaProperty.Register<DockButton, IImage?>(nameof(Icon));

    public IImage? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public static readonly StyledProperty<string?> PopupTooltipProperty = 
        AvaloniaProperty.Register<DockButton, string?>(nameof(PopupTooltip));

    public string? PopupTooltip
    {
        get => GetValue(PopupTooltipProperty);
        set => SetValue(PopupTooltipProperty, value);
    }

    public static readonly StyledProperty<bool?> IsActiveProperty = 
        AvaloniaProperty.Register<DockButton, bool?>(nameof(IsActive));

    public bool? IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public static readonly StyledProperty<ICommand?> CommandProperty = 
        AvaloniaProperty.Register<DockButton, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
}
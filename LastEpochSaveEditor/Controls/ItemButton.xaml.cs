namespace LastEpochSaveEditor.Controls;

public partial class ItemButton : UserControl
{
    public ItemButton() => InitializeComponent();

    public BitmapImage Icon
    {
        get { return (BitmapImage)GetValue(IconProperty); }
        set { SetValue(IconProperty, value); }
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register("Icon", typeof(BitmapImage), typeof(ItemButton), new PropertyMetadata(null));

    public QualityType Quality
    {
        get { return (QualityType)GetValue(QualityProperty); }
        set { SetValue(QualityProperty, value); }
    }

    public static readonly DependencyProperty QualityProperty =
        DependencyProperty.Register("Quality", typeof(QualityType), typeof(ItemButton), new PropertyMetadata(null));

    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register("Command", typeof(ICommand), typeof(ItemButton), new PropertyMetadata(null));

    public int IconWidth
    {
        get { return (int)GetValue(IconWidthProperty); }
        set { SetValue(IconWidthProperty, value); }
    }

    public static readonly DependencyProperty IconWidthProperty =
        DependencyProperty.Register("IconWidth", typeof(int), typeof(ItemButton), new PropertyMetadata(0, new(OnIconWidthChanged)));
    private static void OnIconWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var element = sender as ItemButton;
        if (element != null)
            element.Width = (int)args.NewValue;
    }

    public int IconHeight
    {
        get { return (int)GetValue(IconHeightProperty); }
        set { SetValue(IconHeightProperty, value); }
    }

    public static readonly DependencyProperty IconHeightProperty =
        DependencyProperty.Register("IconHeight", typeof(int), typeof(ItemButton), new PropertyMetadata(0, new(OnIconHeightChanged)));

    private static void OnIconHeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var element = sender as ItemButton;
        if (element != null)
            element.Height = (int)args.NewValue;
    }

    public Thickness IconMargin
    {
        get { return (Thickness)GetValue(IconMarginProperty); }
        set { SetValue(IconMarginProperty, value); }
    }

    public static readonly DependencyProperty IconMarginProperty =
        DependencyProperty.Register("IconMargin", typeof(Thickness), typeof(ItemButton), new PropertyMetadata(new Thickness(0, 0, 0, 0)));
}
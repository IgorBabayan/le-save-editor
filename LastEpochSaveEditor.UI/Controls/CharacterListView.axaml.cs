using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace LastEpochSaveEditor.Controls;

public partial class CharacterListView : UserControl
{
    private static readonly DynamicResourceExtension? _activeColorResource;
    private static readonly DynamicResourceExtension? _hoverColorResource;
    private static readonly DynamicResourceExtension? _defaultColorResource;

    static CharacterListView()
    {
        _activeColorResource = new("ActiveColor");
        _defaultColorResource = new("DockBackgroundColor");
        _hoverColorResource = new("HoverColor");
    }

    public CharacterListView() => InitializeComponent();
    
    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty = 
        AvaloniaProperty.Register<CharacterListView, IEnumerable?>(nameof(ItemsSource));

    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly StyledProperty<object?> SelectedItemProperty = 
        AvaloniaProperty.Register<CharacterListView, object?>(nameof(SelectedItem));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private void ClearBorder()
    {
        Control control;
        Control border;
        object? item;
        var itemsControl = this.FindControl<ItemsControl>("PART_ItemsControl")!;
        for (var index = 0; index < itemsControl.Items.Count; index++)
        {
            item = itemsControl.Items[index];
            if (item != SelectedItem!)
            {
                control = itemsControl.ContainerFromIndex(index)!;
                border = ((ContentPresenter)control).Child!;
                border[!BackgroundProperty] = _defaultColorResource!;
            }
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs _)
    {
        if (sender is Border { DataContext: CharacterInfo item } border)
        {
            SelectedItem = item;
            
            ClearBorder();
            border[!BackgroundProperty] = _activeColorResource!;
        }
    }

    private void OnPointerEntered(object? sender, PointerEventArgs _)
    {
        if (sender is Border border)
        {
            ClearBorder();
            border[!BackgroundProperty] = _hoverColorResource!;
        }
    }

    private void OnPointerExited(object? sender, PointerEventArgs _)
    {
        if (sender is Border { DataContext: CharacterInfo item } border)
        {
            if (item != SelectedItem!)
            {
                ClearBorder();
                border[!BackgroundProperty] = _defaultColorResource!;
            }
            else
                border[!BackgroundProperty] = _activeColorResource!;
        }
    }
}
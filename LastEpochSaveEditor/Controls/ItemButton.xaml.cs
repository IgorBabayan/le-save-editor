namespace LastEpochSaveEditor.Controls;

public partial class ItemButton : IRecipient<CloseCurrentPopupMessage>
{
    public ItemButton()
    {
	    InitializeComponent();
		
	    var messenger = App.GetService<IMessenger>();
	    messenger.RegisterAll(this);
    }

    public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(
	    nameof(Item), typeof(ItemDataInfo), typeof(ItemButton), new PropertyMetadata(default(ItemDataInfo)));

    public ItemDataInfo Item
    {
	    get => (ItemDataInfo)GetValue(ItemProperty);
	    set => SetValue(ItemProperty, value);
    }

    private void OnDragDelta(object sender, DragDeltaEventArgs args)
    {
	    var thumb = sender as Thumb;
	    if (thumb?.Parent is Popup popup)
	    {
		    popup.HorizontalOffset += args.HorizontalChange;
		    popup.VerticalOffset += args.VerticalChange;
	    }
    }

    void IRecipient<CloseCurrentPopupMessage>.Receive(CloseCurrentPopupMessage _)
    {
	    ItemWindowPopup.IsOpen = false;
	    // var control = this.FindName("ItemWindowPopup");
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
	    ItemWindowPopup.IsOpen = true;
    }
}
﻿namespace LastEpochSaveEditor.Controls;

public partial class ItemButton : IRecipient<ItemWindowCloseMessage>
{
	private Guid _id = Guid.Empty;
	public Guid Id
	{
		get
		{
			if (_id == Guid.Empty)
				_id = Guid.NewGuid();
		    
			return _id;
		}
	}
	
    public ItemButton()
    {
	    InitializeComponent();

	    var messenger = App.GetService<IMessenger>();
		messenger.RegisterAll(this);
    }

    public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register(
	    nameof(ItemSource), typeof(ItemDataInfo), typeof(ItemButton), new PropertyMetadata(default(ItemDataInfo)));

    public ItemDataInfo ItemSource
    {
	    get => (ItemDataInfo)GetValue(ItemSourceProperty);
	    set => SetValue(ItemSourceProperty, value);
    }

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
	    nameof(Command), typeof(ICommand), typeof(ItemButton), new PropertyMetadata(default(ICommand?)));

    public ICommand? Command
    {
	    get => (ICommand?)GetValue(CommandProperty);
	    set => SetValue(CommandProperty, value);
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

    private void OnClick(object sender, RoutedEventArgs e)
    {
	    if (FindName("PART_Popup") is Popup popup)
	    {
		    popup.IsOpen = true;
			Command?.Execute(new ItemSelectedValue(Id, ItemSource));
	    }
    }

    public void Receive(ItemWindowCloseMessage message)
    {
	    if (message.Value == Id)
	    {
		    if (FindName("PART_Popup") is Popup popup)
			    popup.IsOpen = false;
		}
    }
}
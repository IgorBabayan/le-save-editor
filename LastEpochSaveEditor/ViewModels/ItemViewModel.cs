namespace LastEpochSaveEditor.ViewModels;

public partial class ItemViewModel : ObservableObject, IRecipient<ItemSelectedMessage>
{
	private readonly IMessenger _messenger;
	private Guid _id;

	#region Properties

	[ObservableProperty]
	private ItemDataInfo? _item;

	[ObservableProperty]
	private ItemDataInfo _selectedItem;

	/*[ObservableProperty]
	private string _selectedItemCategory;*/

	[ObservableProperty]
	private IEnumerable<QualityType> _qualities;

	/*[ObservableProperty]
	private QualityType _selectedQuality;*/

	/*[ObservableProperty]
	private bool _isChecked;*/

	#endregion

	#region Commands

	[RelayCommand]
	private void SetQuality(QualityType quality)
	{
		/*SelectedQuality = quality;
		IsChecked = false;*/
	}

	[RelayCommand]
	private void Close(object id) => _messenger.Send(new ItemWindowCloseMessage((Guid)id));

	[RelayCommand]
	private void Reset() => SelectedItem = Item!.Clone();

	[RelayCommand]
	private void Save()
	{
		
	}

	#endregion

	public ItemViewModel(IMessenger messenger)
	{
		_messenger = messenger;
		_messenger.RegisterAll(this);
		
		Qualities = new[] 
		{
			QualityType.Basic,
			QualityType.Magic,
			QualityType.Rare,
			QualityType.Exalted,
			QualityType.Unique,
			QualityType.Set,
			QualityType.Legendary
		};
	}

	void IRecipient<ItemSelectedMessage>.Receive(ItemSelectedMessage args)
	{
		if (Item == null)
		{
			_id = args.Value.Id;
			
			Item = args.Value.Value;
			SelectedItem = Item.Clone();
		}
		else if (_id == args.Value.Id)
		{
			Item = args.Value.Value;
			SelectedItem = Item.Clone();
		}
	}
}
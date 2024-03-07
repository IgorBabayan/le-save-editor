namespace LastEpochSaveEditor.ViewModels;

public partial class ItemViewModel : ObservableObject
{
	private readonly IMessenger _messenger;

	#region Properties

	[ObservableProperty]
	private ItemDataInfo _item;

	[ObservableProperty]
	private string _selectedItemCategory;

	[ObservableProperty]
	private IEnumerable<QualityType> _qualities;

	[ObservableProperty]
	private QualityType _selectedQuality;

	[ObservableProperty]
	private bool _isChecked;

	#endregion

	#region Commands

	[RelayCommand]
	private void SetQuality(QualityType quality)
	{
		SelectedQuality = quality;
		IsChecked = false;
	}

	[RelayCommand]
	private void Close(object id) => _messenger.Send(new ItemWindowCloseMessage((Guid)id));

	#endregion

	public ItemViewModel(IMessenger messenger)
	{
		_messenger = messenger;
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
}
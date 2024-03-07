namespace LastEpochSaveEditor.ViewModels;

public partial class ItemViewModel : ObservableObject
{
	private readonly IMessenger _messenger;

	#region Properties

	[ObservableProperty]
	private ItemDataInfo _item;

	[ObservableProperty]
	private IEnumerable<ItemTypeCategory> _itemTypes;

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
	private void Close() => _messenger.Send(new CloseCurrentPopupMessage(true));

	[RelayCommand]
	private void SetQuality(QualityType quality)
	{
		SelectedQuality = quality;
		IsChecked = false;
	}

	#endregion

	public ItemViewModel(IMessenger messenger)
	{
		_messenger = messenger;

		Qualities = new[] { QualityType.Basic, QualityType.Magic, QualityType.Rare, QualityType.Exalted, QualityType.Unique, QualityType.Set,
			QualityType.Legendary };
		PopulateCategory();
	}

	private void PopulateCategory()
	{
		ItemTypes = new List<ItemTypeCategory>
		{
			new()
			{
				Name = "Accessories",
				SubCategories = new []{ "Amulet", "Relic", "Ring" }
			},
			new()
			{
				Name = "Armours",
				SubCategories = new []{ "Belts", "Body Armor", "Boots", "Gloves", "Helmets" }
			},
			new()
			{
				Name = "Idols",
				SubCategories = new []{ "Adorned Idol", "Grand Idol", "Humble Idol", "Large Idol", "Ornate Idol", "Small Idol", "Small Lagonian Idol",
					"Stout Idol" }
			},
			new()
			{
				Name = "Off Hands",
				SubCategories = new []{ "Catalyst", "Quiver", "Shield" }
			},
			new()
			{
				Name = "One hand weapons",
				SubCategories = new []{ "1H Axes", "1H Dagger", "1H Maces", "1H Scepter", "1H Swords", "Wands" }
			},
			new()
			{
				Name = "Two hand weapons",
				SubCategories = new[]{ "2H Axes", "2H Maces", "2H Polearm", "2H Staff", "2H Swords", "Bows" }
			}
		};
	}
}
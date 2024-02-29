namespace LastEpochSaveEditor.ViewModels;

public partial class ItemViewModel : ObservableObject, IRecipient<SelectedItemInfoMessage>
{
	private readonly IMessenger _messenger;

	#region Properties

	[ObservableProperty]
	private ItemDataInfo _selectedItem;

	[ObservableProperty]
	private IEnumerable<ItemTypeCategory> _itemTypes;

	[ObservableProperty]
	private string _selectedItemCategory;

	[ObservableProperty]
	private IEnumerable<QualityType> _qualities;

	[ObservableProperty]
	private QualityType _selectedQuality;

	#endregion

	#region Commands

	[RelayCommand]
	private void SetQuality(QualityType quality)
	{

	}

	#endregion

	public ItemViewModel(IMessenger messenger)
	{
		_messenger = messenger; ;
		_messenger.RegisterAll(this);

		Qualities = new[] { QualityType.Basic, QualityType.Magic, QualityType.Rare, QualityType.Exalted, QualityType.Unique, QualityType.Set,
			QualityType.Legendary };
		PopulateCategory();
	}

	[RelayCommand]
	private void Close()
	{
		var window = App.GetService<ItemWindow>();
		var grid = ((MainWindow)App.Current.MainWindow).MainGrid;
		grid.Children.Remove(window);
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

	void IRecipient<SelectedItemInfoMessage>.Receive(SelectedItemInfoMessage message) => ReceiveItemInfo(message);

	private void ReceiveItemInfo(SelectedItemInfoMessage selectedItem)
	{
		switch (selectedItem.InfoType)
		{
			case ItemInfoTypeEnum.Helm:
				SelectedItem = selectedItem.Value.CharacterInventory.Helm;
				break;

			case ItemInfoTypeEnum.Amulet:
				SelectedItem = selectedItem.Value.CharacterInventory.Amulet;
				break;

			case ItemInfoTypeEnum.Weapon:
				SelectedItem = selectedItem.Value.CharacterInventory.Weapon;
				break;

			case ItemInfoTypeEnum.Body:
				SelectedItem = selectedItem.Value.CharacterInventory.Body;
				break;

			case ItemInfoTypeEnum.OffHand:
				SelectedItem = selectedItem.Value.CharacterInventory.OffHand;
				break;

			case ItemInfoTypeEnum.LeftRing:
				SelectedItem = selectedItem.Value.CharacterInventory.LeftRing;
				break;

			case ItemInfoTypeEnum.Belt:
				SelectedItem = selectedItem.Value.CharacterInventory.Belt;
				break;

			case ItemInfoTypeEnum.RightRing:
				SelectedItem = selectedItem.Value.CharacterInventory.RightRing;
				break;

			case ItemInfoTypeEnum.Gloves:
				SelectedItem = selectedItem.Value.CharacterInventory.Gloves;
				break;

			case ItemInfoTypeEnum.Boots:
				SelectedItem = selectedItem.Value.CharacterInventory.Boots;
				break;

			case ItemInfoTypeEnum.Relic:
				SelectedItem = selectedItem.Value.CharacterInventory.Relic;
				break;

			case ItemInfoTypeEnum.All:
				SelectedItem = null;
				break;
		}
	}
}
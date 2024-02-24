namespace LastEpochSaveEditor.ViewModels;

internal partial class CharacterViewModel : ObservableObject, IRecipient<SelectedCharacterChangedMessage>
{
	private readonly IMessenger _messenger;
	private readonly UserControl _itemInfoWindow;

	private Character _selectedCharacter;

	#region Properties

	[ObservableProperty]
	private ItemDataInfo _helm;

	[ObservableProperty]
	private ItemDataInfo _amulet;

	[ObservableProperty]
	private ItemDataInfo _weapon;

	[ObservableProperty]
	private ItemDataInfo _body;

	[ObservableProperty]
	private ItemDataInfo _offHand;

	[ObservableProperty]
	private ItemDataInfo _leftRing;

	[ObservableProperty]
	private ItemDataInfo _belt;

	[ObservableProperty]
	private ItemDataInfo _rightRing;

	[ObservableProperty]
	private ItemDataInfo _gloves;

	[ObservableProperty]
	private ItemDataInfo _boots;

	[ObservableProperty]
	private ItemDataInfo _relic;

	#endregion

	#region Commands

	[RelayCommand]
	private void HelmPressed()
	{
		((MainWindow)App.Current.MainWindow).MainGrid.Children.Add(_itemInfoWindow);
		_messenger.Send(new SelectedItemInfoMessage(_selectedCharacter, ItemInfoTypeEnum.Helm));
	}

	[RelayCommand]
	private void AmuletPressed()
	{
		((MainWindow)App.Current.MainWindow).MainGrid.Children.Add(_itemInfoWindow);
		_messenger.Send(new SelectedItemInfoMessage(_selectedCharacter, ItemInfoTypeEnum.Amulet));
	}

	[RelayCommand]
	private void WeaponPressed()
	{
		((MainWindow)App.Current.MainWindow).MainGrid.Children.Add(_itemInfoWindow);
		_messenger.Send(new SelectedItemInfoMessage(_selectedCharacter, ItemInfoTypeEnum.Weapon));
	}

	[RelayCommand]
	private void BodyPressed()
	{
		((MainWindow)App.Current.MainWindow).MainGrid.Children.Add(_itemInfoWindow);
		_messenger.Send(new SelectedItemInfoMessage(_selectedCharacter, ItemInfoTypeEnum.Body));
	}

	[RelayCommand]
	private void OffHandPressed()
	{
		((MainWindow)App.Current.MainWindow).MainGrid.Children.Add(_itemInfoWindow);
		_messenger.Send(new SelectedItemInfoMessage(_selectedCharacter, ItemInfoTypeEnum.OffHand));
	}

	[RelayCommand]
	private void LeftRingPressed()
	{
		((MainWindow)App.Current.MainWindow).MainGrid.Children.Add(_itemInfoWindow);
		_messenger.Send(new SelectedItemInfoMessage(_selectedCharacter, ItemInfoTypeEnum.LeftRing));
	}

	[RelayCommand]
	private void BeltPressed()
	{
		((MainWindow)App.Current.MainWindow).MainGrid.Children.Add(_itemInfoWindow);
		_messenger.Send(new SelectedItemInfoMessage(_selectedCharacter, ItemInfoTypeEnum.Belt));
	}

	[RelayCommand]
	private void RightRingPressed()
	{
		((MainWindow)App.Current.MainWindow).MainGrid.Children.Add(_itemInfoWindow);
		_messenger.Send(new SelectedItemInfoMessage(_selectedCharacter, ItemInfoTypeEnum.RightRing));
	}

	[RelayCommand]
	private void GlovesPressed()
	{
		((MainWindow)App.Current.MainWindow).MainGrid.Children.Add(_itemInfoWindow);
		_messenger.Send(new SelectedItemInfoMessage(_selectedCharacter, ItemInfoTypeEnum.Gloves));
	}

	[RelayCommand]
	private void BootsPressed()
	{
		((MainWindow)App.Current.MainWindow).MainGrid.Children.Add(_itemInfoWindow);
		_messenger.Send(new SelectedItemInfoMessage(_selectedCharacter, ItemInfoTypeEnum.Boots));
	}

	[RelayCommand]
	private void RelicPressed()
	{
		((MainWindow)App.Current.MainWindow).MainGrid.Children.Add(_itemInfoWindow);
		_messenger.Send(new SelectedItemInfoMessage(_selectedCharacter, ItemInfoTypeEnum.Relic));
	}

	#endregion

	public CharacterViewModel(IMessenger messenger)
	{
		_messenger = messenger; ;
		_messenger.RegisterAll(this);

		_itemInfoWindow = App.GetService<ItemWindow>();
	}

	void IRecipient<SelectedCharacterChangedMessage>.Receive(SelectedCharacterChangedMessage message) => ParseCharacter(message.Value);

	private void ParseCharacter(CharacterInfo characterInfo)
	{
		_selectedCharacter = characterInfo.Character;

		Helm = _selectedCharacter.CharacterInventory.Helm;
		Body = _selectedCharacter.CharacterInventory.Body;
		Weapon = _selectedCharacter.CharacterInventory.Weapon;
		OffHand = _selectedCharacter.CharacterInventory.OffHand;
		Gloves = _selectedCharacter.CharacterInventory.Gloves;
		Belt = _selectedCharacter.CharacterInventory.Belt;
		Boots = _selectedCharacter.CharacterInventory.Boots;
		LeftRing = _selectedCharacter.CharacterInventory.LeftRing;
		RightRing = _selectedCharacter.CharacterInventory.RightRing;
		Amulet = _selectedCharacter.CharacterInventory.Amulet;
		Relic = _selectedCharacter.CharacterInventory.Relic;
	}
}
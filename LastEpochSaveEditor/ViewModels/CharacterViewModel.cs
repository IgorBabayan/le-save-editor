namespace LastEpochSaveEditor.ViewModels;

public partial class CharacterViewModel : ObservableObject, IRecipient<SelectedCharacterChangedMessage>
{
	private readonly IMessenger _messenger;
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
	private void SelectItem(ItemSelectedValue args) => _messenger.Send(new ItemSelectedMessage(args));

	#endregion

	public CharacterViewModel(IMessenger messenger)
	{
		_messenger = messenger;
		_messenger.RegisterAll(this);
	}

	void IRecipient<SelectedCharacterChangedMessage>.Receive(SelectedCharacterChangedMessage message) => ParseCharacter(message.Value);

	private void ParseCharacter(CharacterInfo? characterInfo)
	{
		if (characterInfo == null)
			return;
		
		_selectedCharacter = characterInfo.Character;

		Helm = _selectedCharacter.Inventory.Helm;
		Body = _selectedCharacter.Inventory.Body;
		Weapon = _selectedCharacter.Inventory.Weapon;
		OffHand = _selectedCharacter.Inventory.OffHand;
		Gloves = _selectedCharacter.Inventory.Gloves;
		Belt = _selectedCharacter.Inventory.Belt;
		Boots = _selectedCharacter.Inventory.Boots;
		LeftRing = _selectedCharacter.Inventory.LeftRing;
		RightRing = _selectedCharacter.Inventory.RightRing;
		Amulet = _selectedCharacter.Inventory.Amulet;
		Relic = _selectedCharacter.Inventory.Relic;
	}
}
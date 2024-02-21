namespace LastEpochSaveEditor.ViewModels
{
	public partial class ItemViewModel : ObservableObject, IRecipient<SelectedItemInfoMessage>
	{
		private readonly IMessenger _messenger;

		#region Properties

		[ObservableProperty]
		private ItemDataInfo _selectedItem;

		#endregion

		public ItemViewModel(IMessenger messenger)
        {
			_messenger = messenger; ;
			_messenger.RegisterAll(this);
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
					//SelectedItem = selectedItem.Value.CharacterInventory.Helm;
					break;
			}
		}
	}
}

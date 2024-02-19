using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LastEpochSaveEditor.Messages;
using LastEpochSaveEditor.Models;
using LastEpochSaveEditor.Models.Characters;

namespace LastEpochSaveEditor.ViewModels
{
	internal partial class CharacterViewModel : ObservableObject, IRecipient<SelectedCharacterChangedMessage>
	{
		private readonly IMessenger _messenger;

		#region Properties

		[ObservableProperty]
		private string _helmIcon;

		[ObservableProperty]
		private string _amuletIcon;

		[ObservableProperty]
		private string _weaponIcon;

		[ObservableProperty]
		private string _bodyIcon;

		[ObservableProperty]
		private string _offHandIcon;

		[ObservableProperty]
		private string _leftRingIcon;

		[ObservableProperty]
		private string _beltIcon;

		[ObservableProperty]
		private string _rightRingIcon;

		[ObservableProperty]
		private string _glovesIcon;

		[ObservableProperty]
		private string _bootsIcon;

		[ObservableProperty]
		private string _relicIcon;

		#endregion

		#region Commands

		[RelayCommand]
		private void HelmPressed()
		{
			throw new System.NotImplementedException();
		}

		[RelayCommand]
		private void AmuletPressed()
		{
			throw new System.NotImplementedException();
		}

		[RelayCommand]
		private void OneHandPressed()
		{
			throw new System.NotImplementedException();
		}

		[RelayCommand]
		private void BodyPressed()
		{
			throw new System.NotImplementedException();
		}

		[RelayCommand]
		private void OffHandPressed()
		{
			throw new System.NotImplementedException();
		}

		[RelayCommand]
		private void LeftRingPressed()
		{
			throw new System.NotImplementedException();
		}

		[RelayCommand]
		private void BeltPressed()
		{
			throw new System.NotImplementedException();
		}

		[RelayCommand]
		private void RightRingPressed()
		{
			throw new System.NotImplementedException();
		}

		[RelayCommand]
		private void GlovesPressed()
		{
			throw new System.NotImplementedException();
		}

		[RelayCommand]
		private void BootsPressed()
		{
			throw new System.NotImplementedException();
		}

		[RelayCommand]
		private void RelicPressed()
		{
			throw new System.NotImplementedException();
		}

        #endregion

        public CharacterViewModel(IMessenger messenger)
        {
            _messenger = messenger; ;
			_messenger.RegisterAll(this);
        }

		void IRecipient<SelectedCharacterChangedMessage>.Receive(SelectedCharacterChangedMessage message) => ParseCharacter(message.Value);

		private void ParseCharacter(CharacterInfo characterInfo)
		{
			HelmIcon = characterInfo.Character.CharacterInventory.Helm.Icon;
			BodyIcon = characterInfo.Character.CharacterInventory.Body.Icon;
			WeaponIcon = characterInfo.Character.CharacterInventory.Weapon.Icon;
			OffHandIcon = characterInfo.Character.CharacterInventory.OffHand.Icon;
			GlovesIcon = characterInfo.Character.CharacterInventory.Gloves.Icon;
			BeltIcon = characterInfo.Character.CharacterInventory.Belt.Icon;
			BootsIcon = characterInfo.Character.CharacterInventory.Boots.Icon;
			LeftRingIcon = characterInfo.Character.CharacterInventory.LeftRing.Icon;
			RightRingIcon = characterInfo.Character.CharacterInventory.RightRing.Icon;
			AmuletIcon = characterInfo.Character.CharacterInventory.Amulet.Icon;
			RelicIcon = characterInfo.Character.CharacterInventory.Relic.Icon;
		}
	}
}

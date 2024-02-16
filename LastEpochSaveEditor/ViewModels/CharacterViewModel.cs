using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LastEpochSaveEditor.Messages;
using LastEpochSaveEditor.Models;
using LastEpochSaveEditor.Utils;
using System.Linq;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace LastEpochSaveEditor.ViewModels
{
	internal partial class CharacterViewModel : ObservableObject, IRecipient<SelectedCharacterChangedMessage>
	{
		private readonly IMessenger _messenger;
		private readonly IDB _db;

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

        public CharacterViewModel(IMessenger messenger, IDB db)
        {
			_db = db;
            _messenger = messenger; ;
			_messenger.RegisterAll(this);
        }

        void IRecipient<SelectedCharacterChangedMessage>.Receive(SelectedCharacterChangedMessage message)
		{
			ParseCharacter(message.Value);
		}

		private void ParseCharacter(CharacterInfo character)
		{
			ParseHelm(character);
			ParseAmulet(character);
			ParseWeapon(character);
			ParseBody(character);
			ParseOffHand(character);
			ParseLeftRing(character);
			ParseBelt(character);
			ParseRightRing(character);
			ParseGloves(character);
			ParseBoots(character);
			ParseRelics(character);
		}

		private void ParseHelm(CharacterInfo character)
		{
			var currentHelmet = character.Character.CharacterInventory.Helm;
			HelmIcon = currentHelmet == null ? "/Images/Misc/Helm.png" : "";
		}

		private void ParseAmulet(CharacterInfo character)
		{
			AmuletIcon = "/Images/Misc/Amulet.png";
		}
		
		private void ParseWeapon(CharacterInfo character)
		{
			WeaponIcon = "/Images/Misc/Main Weapon.png";
		}

		private void ParseBody(CharacterInfo character)
		{
			BodyIcon = "/Images/Misc/Body.png";
		}

		private void ParseOffHand(CharacterInfo character)
		{
			OffHandIcon = "/Images/Misc/Off Hand.png";
		}

		private void ParseLeftRing(CharacterInfo character)
		{
			LeftRingIcon = "/Images/Misc/Left Ring.png";
		}

		private void ParseBelt(CharacterInfo character)
		{
			BeltIcon = "/Images/Misc/Belt.png";
		}

		private void ParseRightRing(CharacterInfo character)
		{
			RightRingIcon = "/Images/Misc/Right Ring.png";
		}

		private void ParseGloves(CharacterInfo character)
		{
			GlovesIcon = "/Images/Misc/Gloves.png";
		}

		private void ParseBoots(CharacterInfo character)
		{
			BootsIcon = "/Images/Misc/Boots.png";
		}

		private void ParseRelics(CharacterInfo character)
		{
			RelicIcon = "/Images/Misc/Relic.png";
		}
	}
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
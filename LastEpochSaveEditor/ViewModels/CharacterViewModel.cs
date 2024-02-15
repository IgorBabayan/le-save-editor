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
	public partial class CharacterViewModel : ObservableObject
	{
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
			var helmets = DB.Instance.GetHelmets();
			var currentHelmet = helmets.FirstOrDefault();
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

		public CharacterViewModel() => WeakReferenceMessenger.Default.Register<SelectedCharacterChangedMessage>(this, (_, message) => ParseCharacter(message.Value));

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
			HelmIcon = "/Icons/Misc/Helm.png";
		}

		private void ParseAmulet(CharacterInfo character)
		{
			AmuletIcon = "/Icons/Misc/Amulet.png";
		}
		
		private void ParseWeapon(CharacterInfo character)
		{
			WeaponIcon = "/Icons/Misc/Main Weapon.png";
		}

		private void ParseBody(CharacterInfo character)
		{
			BodyIcon = "/Icons/Misc/Body.png";
		}

		private void ParseOffHand(CharacterInfo character)
		{
			OffHandIcon = "/Icons/Misc/Off Hand.png";
		}

		private void ParseLeftRing(CharacterInfo character)
		{
			LeftRingIcon = "/Icons/Misc/Left Ring.png";
		}

		private void ParseBelt(CharacterInfo character)
		{
			BeltIcon = "/Icons/Misc/Belt.png";
		}

		private void ParseRightRing(CharacterInfo character)
		{
			RightRingIcon = "/Icons/Misc/Right Ring.png";
		}

		private void ParseGloves(CharacterInfo character)
		{
			GlovesIcon = "/Icons/Misc/Gloves.png";
		}

		private void ParseBoots(CharacterInfo character)
		{
			BootsIcon = "/Icons/Misc/Boots.png";
		}

		private void ParseRelics(CharacterInfo character)
		{
			RelicIcon = "/Icons/Misc/Relic.png";
		}
	}
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
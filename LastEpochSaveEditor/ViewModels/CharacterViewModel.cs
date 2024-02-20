using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LastEpochSaveEditor.Messages;
using LastEpochSaveEditor.Models;
using LastEpochSaveEditor.Models.Characters;
using System.Windows.Media.Imaging;

namespace LastEpochSaveEditor.ViewModels
{
	internal partial class CharacterViewModel : ObservableObject, IRecipient<SelectedCharacterChangedMessage>
	{
		private readonly IMessenger _messenger;

		#region Properties

		[ObservableProperty]
		private BitmapImage _helmIcon;

		[ObservableProperty]
		private BitmapImage _amuletIcon;

		[ObservableProperty]
		private BitmapImage _weaponIcon;

		[ObservableProperty]
		private BitmapImage _bodyIcon;

		[ObservableProperty]
		private BitmapImage _offHandIcon;

		[ObservableProperty]
		private BitmapImage _leftRingIcon;

		[ObservableProperty]
		private BitmapImage _beltIcon;

		[ObservableProperty]
		private BitmapImage _rightRingIcon;

		[ObservableProperty]
		private BitmapImage _glovesIcon;

		[ObservableProperty]
		private BitmapImage _bootsIcon;

		[ObservableProperty]
		private BitmapImage _relicIcon;

		[ObservableProperty]
		private QualityType _helmQuality;

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
			HelmQuality = characterInfo.Character.CharacterInventory.Helm.Quality;

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

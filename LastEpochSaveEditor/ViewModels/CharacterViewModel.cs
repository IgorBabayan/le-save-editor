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

		[ObservableProperty]
		private QualityType _amuletQuality;

		[ObservableProperty]
		private QualityType _weaponQuality;

		[ObservableProperty]
		private QualityType _bodyQuality;

		[ObservableProperty]
		private QualityType _offHandQuality;

		[ObservableProperty]
		private QualityType _leftRingQuality;

		[ObservableProperty]
		private QualityType _beltQuality;

		[ObservableProperty]
		private QualityType _rightRingQuality;

		[ObservableProperty]
		private QualityType _glovesQuality;

		[ObservableProperty]
		private QualityType _bootsQuality;

		[ObservableProperty]
		private QualityType _relicQuality;

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
			BodyQuality = characterInfo.Character.CharacterInventory.Body.Quality;

			WeaponIcon = characterInfo.Character.CharacterInventory.Weapon.Icon;
			WeaponQuality = characterInfo.Character.CharacterInventory.Weapon.Quality;

			OffHandIcon = characterInfo.Character.CharacterInventory.OffHand.Icon;
			OffHandQuality = characterInfo.Character.CharacterInventory.OffHand.Quality;

			GlovesIcon = characterInfo.Character.CharacterInventory.Gloves.Icon;
			GlovesQuality = characterInfo.Character.CharacterInventory.Gloves.Quality;

			BeltIcon = characterInfo.Character.CharacterInventory.Belt.Icon;
			BeltQuality = characterInfo.Character.CharacterInventory.Belt.Quality;

			BootsIcon = characterInfo.Character.CharacterInventory.Boots.Icon;
			BootsQuality = characterInfo.Character.CharacterInventory.Boots.Quality;

			LeftRingIcon = characterInfo.Character.CharacterInventory.LeftRing.Icon;
			LeftRingQuality = characterInfo.Character.CharacterInventory.LeftRing.Quality;

			RightRingIcon = characterInfo.Character.CharacterInventory.RightRing.Icon;
			RightRingQuality = characterInfo.Character.CharacterInventory.RightRing.Quality;

			AmuletIcon = characterInfo.Character.CharacterInventory.Amulet.Icon;
			AmuletQuality = characterInfo.Character.CharacterInventory.Amulet.Quality;

			RelicIcon = characterInfo.Character.CharacterInventory.Relic.Icon;
			RelicQuality = characterInfo.Character.CharacterInventory.Relic.Quality;
		}
	}
}

﻿namespace LastEpochSaveEditor.ViewModels
{
	internal partial class MainViewModel : ObservableObject
	{
		private readonly IMessenger _messenger;
		private readonly IDB _db;

		#region Properties

		[ObservableProperty]
		private IEnumerable<CharacterInfo> _characters;

		[ObservableProperty]
		private CharacterInfo _selectedCharacter;

		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(CharacterStashControlVisiblity), nameof(BlessingsControlVisibility), nameof(IdolsControlVisibility))]
		private Visibility _characterControlVisiblity = Visibility.Visible;

		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(CharacterControlVisiblity), nameof(BlessingsControlVisibility), nameof(IdolsControlVisibility))]
		private Visibility _characterStashControlVisiblity = Visibility.Collapsed;

		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(CharacterControlVisiblity), nameof(CharacterStashControlVisiblity), nameof(IdolsControlVisibility))]
		private Visibility _blessingsControlVisibility = Visibility.Collapsed;

		[ObservableProperty]
		[NotifyPropertyChangedFor(nameof(CharacterControlVisiblity), nameof(CharacterStashControlVisiblity), nameof(BlessingsControlVisibility))]
		private Visibility _idolsControlVisibility = Visibility.Collapsed;

		#endregion

		public MainViewModel(IMessenger messenger, IDB db)
		{
			_messenger = messenger;
			_db = db;

			Characters = SaveFileLoader.Load();
			SelectedCharacter = Characters.FirstOrDefault();
		}

		#region Commands

		[RelayCommand]
		private void CharacterTabActivePressed()
		{
			CharacterControlVisiblity = Visibility.Visible;
			CharacterStashControlVisiblity = Visibility.Collapsed;
			BlessingsControlVisibility = Visibility.Collapsed;
			IdolsControlVisibility = Visibility.Collapsed;
		}

		[RelayCommand]
		private void CharacterStashTabActivePressed()
		{
			CharacterStashControlVisiblity = Visibility.Visible;
			CharacterControlVisiblity = Visibility.Collapsed;
			BlessingsControlVisibility = Visibility.Collapsed;
			IdolsControlVisibility = Visibility.Collapsed;
		}

		[RelayCommand]
		private void BlessingsTabActivePressed()
		{
			BlessingsControlVisibility = Visibility.Visible;
			CharacterControlVisiblity = Visibility.Collapsed;
			CharacterStashControlVisiblity = Visibility.Collapsed;
			IdolsControlVisibility = Visibility.Collapsed;
		}

		[RelayCommand]
		private void IdolsTabActivePressed()
		{
			IdolsControlVisibility = Visibility.Visible;
			CharacterControlVisiblity = Visibility.Collapsed;
			CharacterStashControlVisiblity = Visibility.Collapsed;
			BlessingsControlVisibility = Visibility.Collapsed;
		}

		[RelayCommand]
		private async Task ReloadDatabase() => await _db.Reload();

		#endregion

		#region Partials

		partial void OnSelectedCharacterChanged(CharacterInfo value) => _messenger.Send(new SelectedCharacterChangedMessage(value));

		#endregion
	}
}

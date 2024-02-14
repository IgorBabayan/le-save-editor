using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LastEpochSaveEditor.Models;
using LastEpochSaveEditor.Utils;
using System.Collections.Generic;

namespace LastEpochSaveEditor.ViewModels
{
	public partial class MainViewModel : ObservableObject
	{
		#region Properties

		[ObservableProperty]
		private IEnumerable<CharacterInfo> _characters;

		[ObservableProperty]
		private CharacterInfo _selectedCharacter;

		[ObservableProperty]
		private bool _characterTabActive;

		[ObservableProperty]
		private bool _characterStashTabActive;

		[ObservableProperty]
		private bool _blessingsTabActive;

		[ObservableProperty]
		private bool _idolsTabActive;

		#endregion

		public MainViewModel()
		{
			Characters = SaveFileLoader.Load();
		}

		#region Partials

		partial void OnCharacterTabActiveChanged(bool value)
		{
			if (!value)
				return;

			CharacterStashTabActive = false;
			BlessingsTabActive = false;
			IdolsTabActive = false;
		}

		partial void OnCharacterStashTabActiveChanged(bool value)
		{
			if (!value)
				return;

			CharacterTabActive = false;
			BlessingsTabActive = false;
			IdolsTabActive = false;
		}

		partial void OnBlessingsTabActiveChanged(bool value)
		{
			if (!value)
				return;

			CharacterTabActive = false;
			CharacterStashTabActive = false;
			IdolsTabActive = false;
		}

		partial void OnIdolsTabActiveChanged(bool value)
		{
			if (!value)
				return;

			CharacterTabActive = false;
			CharacterStashTabActive = false;
			BlessingsTabActive = false;
		}

		#endregion

		#region Commands

		[RelayCommand]
		private void CharacterTabActivePressed() => CharacterTabActive = true;

		[RelayCommand]
		private void CharacterStashTabActivePressed() => CharacterStashTabActive = true;

		[RelayCommand]
		private void BlessingsTabActivePressed() => BlessingsTabActive = true;

		[RelayCommand]
		private void IdolsTabActivePressed() => IdolsTabActive = true;

		#endregion
	}
}

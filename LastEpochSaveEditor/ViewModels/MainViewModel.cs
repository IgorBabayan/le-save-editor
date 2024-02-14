using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LastEpochSaveEditor.Models;
using LastEpochSaveEditor.Utils;
using System.Collections.Generic;
using System.Windows;

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
		private Visibility _characterControlVisiblity = Visibility.Visible;

		[ObservableProperty]
		private Visibility _characterStashControlVisiblity = Visibility.Hidden;

		[ObservableProperty]
		private Visibility _blessingsControlVisibility = Visibility.Hidden;

		[ObservableProperty]
		private Visibility _idolsControlVisibility = Visibility.Hidden;

		#endregion

		public MainViewModel()
		{
			Characters = SaveFileLoader.Load();
		}

		#region Partials

		partial void OnCharacterControlVisiblityChanged(Visibility value)
		{
			if (value == Visibility.Collapsed)
				return;

			CharacterStashControlVisiblity = Visibility.Collapsed;
			BlessingsControlVisibility = Visibility.Collapsed;
			IdolsControlVisibility = Visibility.Collapsed;
		}

		partial void OnCharacterStashControlVisiblityChanged(Visibility value)
		{
			if (value == Visibility.Collapsed)
				return;

			CharacterControlVisiblity = Visibility.Collapsed;
			BlessingsControlVisibility = Visibility.Collapsed;
			IdolsControlVisibility = Visibility.Collapsed;
		}

		partial void OnBlessingsControlVisibilityChanged(Visibility value)
		{
			if (value == Visibility.Collapsed)
				return;

			CharacterControlVisiblity = Visibility.Collapsed;
			CharacterStashControlVisiblity = Visibility.Collapsed;
			IdolsControlVisibility = Visibility.Collapsed;
		}

		partial void OnIdolsControlVisibilityChanged(Visibility value)
		{
			if (value == Visibility.Collapsed)
				return;

			CharacterControlVisiblity = Visibility.Collapsed;
			CharacterStashControlVisiblity = Visibility.Collapsed;
			BlessingsControlVisibility = Visibility.Collapsed;
		}

		#endregion

		#region Commands

		[RelayCommand]
		private void CharacterTabActivePressed() => CharacterControlVisiblity = Visibility.Visible;

		[RelayCommand]
		private void CharacterStashTabActivePressed() => CharacterStashControlVisiblity = Visibility.Visible;

		[RelayCommand]
		private void BlessingsTabActivePressed() => BlessingsControlVisibility = Visibility.Visible;

		[RelayCommand]
		private void IdolsTabActivePressed() => IdolsControlVisibility = Visibility.Visible;

		#endregion
	}
}

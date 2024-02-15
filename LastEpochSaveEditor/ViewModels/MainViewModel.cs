using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LastEpochSaveEditor.Messages;
using LastEpochSaveEditor.Models;
using LastEpochSaveEditor.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

#pragma warning disable CS8601 // Possible null reference assignment.

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

		public MainViewModel()
		{
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
		private async Task ReloadDatabase() => await DB.Reload();

		#endregion

		#region Partials

		partial void OnSelectedCharacterChanged(CharacterInfo value) => WeakReferenceMessenger.Default.Send(new SelectedCharacterChangedMessage(value));

		#endregion
	}
}

#pragma warning restore CS8601 // Possible null reference assignment.
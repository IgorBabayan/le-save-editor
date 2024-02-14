using CommunityToolkit.Mvvm.ComponentModel;
using LastEpochSaveEditor.Models;
using LastEpochSaveEditor.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LastEpochSaveEditor.ViewModels
{
	public partial class MainViewModel : ObservableObject
	{
		[ObservableProperty]
		private IEnumerable<CharacterInfo> _characters;

		[ObservableProperty]
		private CharacterInfo _selectedCharacter;

		public MainViewModel()
		{
			Characters = SaveFileLoader.Load();
		}
	}
}

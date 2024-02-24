namespace LastEpochSaveEditor.ViewModels;

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
	private INavigationService _navigationService;

	#endregion

	public MainViewModel(IMessenger messenger, IDB db, INavigationService navigationService)
	{
		_messenger = messenger;
		_db = db;

		NavigationService = navigationService;
		NavigationService.NavigateTo<CharacterViewModel>();

		//Characters = SaveFileLoader.Load("1CHARACTERSLOT_BETA_2");
		Characters = SaveFileLoader.Load();
		SelectedCharacter = Characters.FirstOrDefault();
	}

	#region Commands

	[RelayCommand]
	private void CharacterPressed() => NavigationService.NavigateTo<CharacterViewModel>();

	[RelayCommand]
	private void StashPressed() => NavigationService.NavigateTo<CharacterStashViewModel>();

	[RelayCommand]
	private void BlessingsPressed() => NavigationService.NavigateTo<BlessingViewModel>();

	[RelayCommand]
	private void IdolsPressed() => NavigationService.NavigateTo<IdolViewModel>();

	[RelayCommand]
	private async Task ReloadDatabase() => await _db.Reload();

	[RelayCommand]
	private void Download()
	{
		var popup = App.GetService<DownloadWindow>();
		((MainWindow)App.Current.MainWindow).MainGrid.Children.Add(popup);
	}

	#endregion

	#region Partials

	partial void OnSelectedCharacterChanged(CharacterInfo value) => _messenger.Send(new SelectedCharacterChangedMessage(value));

	#endregion
}
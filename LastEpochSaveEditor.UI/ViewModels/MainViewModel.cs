namespace LastEpochSaveEditor.ViewModels;

internal partial class MainViewModel : ObservableObject, IRecipient<CurrentViewChangedMessage>
{
	private readonly IMessenger _messenger;
	private readonly IDialogService _dialogService;
	private readonly IDownloadViewModel _downloadViewModel;
	private readonly IDatabaseFactory _databaseFactory;

	#region Properties

	[ObservableProperty]
	private IEnumerable<CharacterInfo> _characters;

	[ObservableProperty]
	private CharacterInfo _selectedCharacter;

	[ObservableProperty]
	private bool _isCharacterActive;

	[ObservableProperty]
	private bool _isStashActive;

	[ObservableProperty]
	private bool _isBlessingsActive;

	[ObservableProperty]
	private bool _isIdolsActive;

	[ObservableProperty]
	private INavigationService _navigationService;

	[ObservableProperty]
	private bool _isCharactersLoaded;

	[ObservableProperty]
	private string _appTitle;

	[ObservableProperty]
	private string _appDescription;

	#endregion

	public MainViewModel(IMessenger messenger, INavigationService navigationService, IDialogService dialogService, IDownloadViewModel downloadViewModel)
	{
		_messenger = messenger;
		_messenger.RegisterAll(this);

		NavigationService = navigationService;
		_dialogService = dialogService;
		_downloadViewModel = downloadViewModel;

		IsCharactersLoaded = false;
		AppTitle = APPLICATION_TITLE;
		AppDescription = APPLICATION_DESCRIPTION;
	}

	#region Commands

	[RelayCommand]
	private async Task Load()
	{
		Characters = await SaveFileLoader.Load();
		IsCharactersLoaded = true;
		CharacterPressed();
		SelectedCharacter = Enumerable.FirstOrDefault(Characters)!;
	}

	[RelayCommand]
	private void CharacterPressed() => NavigationService.NavigateTo<LastEpochSaveEditor.ViewModels.CharacterViewModel>();

	[RelayCommand]
	private void StashPressed() => NavigationService.NavigateTo<CharacterStashViewModel>();

	[RelayCommand]
	private void BlessingsPressed() => NavigationService.NavigateTo<BlessingViewModel>();

	[RelayCommand]
	private void IdolsPressed() => NavigationService.NavigateTo<IdolViewModel>();

	[RelayCommand]
	private async Task ReloadDatabase() => await _databaseFactory.Create(true);

	[RelayCommand]
	private async Task Download() => await _dialogService.ShowCustomDialog<IDownloadView, IDownloadViewModel>(_downloadViewModel);

	#endregion

	#region Partials

	partial void OnSelectedCharacterChanged(CharacterInfo value) => _messenger.Send(new SelectedCharacterChangedMessage(value));

	#endregion

	void IRecipient<CurrentViewChangedMessage>.Receive(CurrentViewChangedMessage message)
	{
		var view = message.Value;
		IsCharacterActive = view.GetType() == typeof(LastEpochSaveEditor.ViewModels.CharacterViewModel);
		IsStashActive = view.GetType() == typeof(CharacterStashViewModel);
		IsBlessingsActive = view.GetType() == typeof(BlessingViewModel);
		IsIdolsActive = view.GetType() == typeof(IdolViewModel);
	}
}
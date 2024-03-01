namespace LastEpochSaveEditor.ViewModels;

internal partial class MainViewModel : ObservableObject, IRecipient<CurrentViewChangedMessage>
{
	private readonly IMessenger _messenger;
	private readonly IDatabaseSerive _db;

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

	#endregion

	public MainViewModel(IMessenger messenger, IDatabaseSerive db, INavigationService navigationService)
	{
		_db = db;

		_messenger = messenger;
		_messenger.RegisterAll(this);

		NavigationService = navigationService;
		NavigationService.NavigateTo<CharacterViewModel>();

		Characters = SaveFileLoader.Load("1CHARACTERSLOT_BETA_0");
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

	void IRecipient<CurrentViewChangedMessage>.Receive(CurrentViewChangedMessage message)
	{
		var view = message.Value;
		IsCharacterActive = view.GetType() == typeof(CharacterViewModel);
		IsStashActive = view.GetType() == typeof(CharacterStashViewModel);
		IsBlessingsActive = view.GetType() == typeof(BlessingViewModel);
		IsIdolsActive = view.GetType() == typeof(IdolViewModel);
	}
}
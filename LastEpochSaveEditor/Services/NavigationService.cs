namespace LastEpochSaveEditor.Services;

internal interface INavigationService
{
    ObservableObject CurrentView { get; }
    void NavigateTo<TViewModel>() where TViewModel : ObservableObject;
}

internal partial class NavigationService : ObservableObject, INavigationService
{
	private readonly Func<Type, ObservableObject> _factory;
	private readonly IMessenger _messenger;

	[ObservableProperty]
	private ObservableObject _currentView;

	public NavigationService(Func<Type, ObservableObject> factory, IMessenger messenger)
	{
		_factory = factory;
		_messenger = messenger;
	}

	public void NavigateTo<TViewModel>()
		where TViewModel : ObservableObject
	{
		CurrentView = _factory.Invoke(typeof(TViewModel));
		_messenger.Send(new CurrentViewChangedMessage(CurrentView));
	}
}

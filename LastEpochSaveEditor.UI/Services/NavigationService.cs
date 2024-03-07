namespace LastEpochSaveEditor.Services;

internal interface INavigationService
{
	ObservableObject CurrentView { get; }
	void NavigateTo<TViewModel>() where TViewModel : ObservableObject;
}

internal partial class NavigationService(Func<Type, ObservableObject> factory, IMessenger messenger)
	: ObservableObject, INavigationService
{
	[ObservableProperty]
	private ObservableObject? _currentView;

	public void NavigateTo<TViewModel>()
		where TViewModel : ObservableObject
	{
		CurrentView = factory.Invoke(typeof(TViewModel));
		messenger.Send(new CurrentViewChangedMessage(CurrentView));
	}
}
namespace LastEpochSaveEditor.Utils;

internal interface INavigationService
{
    ObservableObject CurrentView { get; }
    void NavigateTo<TViewModel>() where TViewModel : ObservableObject;
}

internal partial class NavigationService : ObservableObject, INavigationService
{
	private readonly Func<Type, ObservableObject> _factory;

	[ObservableProperty]
	private ObservableObject _currentView;

	public NavigationService(Func<Type, ObservableObject> factory) => _factory = factory;

	public void NavigateTo<TViewModel>()
		where TViewModel : ObservableObject => CurrentView = _factory.Invoke(typeof(TViewModel));
}

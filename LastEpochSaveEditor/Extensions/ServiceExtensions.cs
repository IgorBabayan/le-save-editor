namespace LastEpochSaveEditor.Extensions;

internal static class ServiceExtensions
{
	public static void RegisterRepositories(this IServiceCollection services)
	{
		services.AddSingleton<IRepository<SubItem>, SubItemRepository>();
		services.AddSingleton<IRepository<Unique>, UniqueRepository>();
		services.AddSingleton<IRepository<Unique>, SetRepository>();

		services.AddSingleton<ISubItemRepositoryFactory, SubItemRepositoryFactory>();
		services.AddSingleton<IUniqueRepositoryFactory, UniqueRepositoryFactory>();
		services.AddSingleton<ISetRepositoryFactory, SetRepositoryFactory>();

		services.AddSingleton(provider =>
		{
			var factory = provider.GetRequiredService<ISubItemRepositoryFactory>();
			return factory.Create();
		});

		services.AddSingleton(provider =>
		{
			var factory = provider.GetRequiredService<IUniqueRepositoryFactory>();
			return factory.Create();
		});

		services.AddSingleton(provider =>
		{
			var factory = provider.GetRequiredService<ISetRepositoryFactory>();
			return factory.Create();
		});
	}

	public static void RegisterParseFactories(this IServiceCollection services)
	{
		services.AddTransient<IItemInfo<ItemInfoTypeEnum>, HelmItemInfo>();
		services.AddTransient<IItemInfo<ItemInfoTypeEnum>, BeltItemInfo>();
		services.AddSingleton<Func<IEnumerable<IItemInfo<ItemInfoTypeEnum>>>>(x => () => x.GetRequiredService<IEnumerable<IItemInfo<ItemInfoTypeEnum>>>());
		services.AddSingleton<IItemInfoFactory, ItemInfoFactory>();
	}

	public static void RegisterMisc(this IServiceCollection services)
	{
		services.AddSingleton<IDatabaseFactory, DatabaseFactory>();
		services.RegisterRepositories();
		services.AddSingleton<IDatabaseService, DatabaseService>();
		services.AddSingleton<INavigationService, NavigationService>();
		services.RegisterParseFactories();
		services.AddTransient<ICharacterInventory, CharacterInventory>();

		services.AddTransient<Func<Type, ObservableObject>>(services => viewModelType =>
			(ObservableObject)services.GetRequiredService(viewModelType));
	}

	public static void RegisterControls(this IServiceCollection services)
	{
		services.AddSingleton<CharacterView>();
		// services.AddSingleton<CharacterView>(provider => new CharacterView(){ DataContext = provider.GetRequiredService<CharacterViewModel>() });
		services.AddSingleton<CharacterStashView>();
		services.AddSingleton<BlessingView>();
		services.AddSingleton<IdolView>();
		services.AddSingleton<DownloadWindow>();
		services.AddSingleton<ItemWindow>();
	}

	public static void RegisterWindows(this IServiceCollection services)
	{
		services.AddSingleton<MainWindow>(provider => new MainWindow
		{
			DataContext = provider.GetRequiredService<MainViewModel>()
		});
	}

	public static void RegisterViewModels(this IServiceCollection services)
	{
		services.AddSingleton<MainViewModel>();
		services.AddSingleton<DownloadViewModel>();
		services.AddSingleton<CharacterViewModel>();
		services.AddSingleton<CharacterStashViewModel>();
		services.AddSingleton<BlessingViewModel>();
		services.AddSingleton<IdolViewModel>();
		services.AddSingleton<ItemViewModel>();
	}

	public static void RegisterMessenger(this IServiceCollection services)
	{
		services.AddSingleton<WeakReferenceMessenger>();
		services.AddSingleton<IMessenger, WeakReferenceMessenger>(provider => provider.GetRequiredService<WeakReferenceMessenger>());
	}
}

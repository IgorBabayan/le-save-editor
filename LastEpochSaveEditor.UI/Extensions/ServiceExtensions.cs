namespace LastEpochSaveEditor.Extensions;

internal static class ServiceExtensions
{
	public static void RegisterRepositories(this IServiceCollection services)
	{
		services.AddSingleton<IRepository<SubItem>, SubItemRepository>();
		services.AddSingleton<IRepository<Unique>, UniqueRepository>();
		services.AddSingleton<IRepository<Unique>, SetRepository>();
		services.AddSingleton<IRepository<Affix>, AffixRepository>();

		services.AddSingleton<ISubItemRepositoryFactory, SubItemRepositoryFactory>();
		services.AddSingleton<IUniqueRepositoryFactory, UniqueRepositoryFactory>();
		services.AddSingleton<ISetRepositoryFactory, SetRepositoryFactory>();
		services.AddSingleton<IAffixRepositoryFactory, AffixRepositoryFactory>();

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

		services.AddSingleton(provider =>
		{
			var factory = provider.GetRequiredService<IAffixRepositoryFactory>();
			return factory.Create();
		});
	}

	public static void RegisterParseFactories(this IServiceCollection services)
	{
		services.AddTransient<IItemInfo<ItemInfoTypeEnum>, HelmItemInfo>();
		services.AddTransient<IItemInfo<ItemInfoTypeEnum>, BodyItemInfo>();
		services.AddTransient<IItemInfo<ItemInfoTypeEnum>, WeaponItemInfo>();
		services.AddTransient<IItemInfo<ItemInfoTypeEnum>, OffHandItemInfo>();
		services.AddTransient<IItemInfo<ItemInfoTypeEnum>, GlovesItemInfo>();
		services.AddTransient<IItemInfo<ItemInfoTypeEnum>, BeltItemInfo>();
		services.AddTransient<IItemInfo<ItemInfoTypeEnum>, BootsItemInfo>();
		services.AddTransient<IItemInfo<ItemInfoTypeEnum>, RingItemInfo>();
		services.AddTransient<IItemInfo<ItemInfoTypeEnum>, AmuletItemInfo>();
		services.AddTransient<IItemInfo<ItemInfoTypeEnum>, RelicItemInfo>();
		services.AddSingleton<Func<IEnumerable<IItemInfo<ItemInfoTypeEnum>>>>(x => () => x.GetRequiredService<IEnumerable<IItemInfo<ItemInfoTypeEnum>>>());
		services.AddSingleton<IItemInfoFactory, ItemInfoFactory>();
	}

	public static void RegisterFactories(this IServiceCollection services)
	{
		services.AddSingleton<IDatabaseFactory, DatabaseFactory>();
		services.RegisterRepositories();
		services.RegisterParseFactories();
	}

	public static void RegisterMisc(this IServiceCollection services)
	{
		services.AddTransient<ICharacterInventory, CharacterInventory>();
	}

	public static void RegisterServices(this IServiceCollection services)
	{
		services.AddSingleton<IDatabaseService, DatabaseService>();
		services.AddSingleton<INavigationService, NavigationService>();
		services.AddSingleton<IDialogService, DialogService>();
		services.AddTransient<Func<Type, ObservableObject>>(services => viewModelType =>
			(ObservableObject)services.GetRequiredService(viewModelType));
	}

	public static void RegisterControls(this IServiceCollection services)
	{
		services.AddSingleton<CharacterView>();
		services.AddSingleton<CharacterStashView>();
		services.AddSingleton<BlessingView>();
		services.AddSingleton<IdolView>();
		services.AddSingleton<ItemWindow>();
		services.AddSingleton<IDownloadView, DownloadWindow>();
		services.AddSingleton<IErrorView, ErrorWindow>();
		services.AddSingleton<IConfirmationView, ConfirmationWindow>();
		services.AddSingleton<IMessageView, MessageWindow>();
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
		services.AddSingleton<IDownloadViewModel, DownloadViewModel>();
		services.AddSingleton<IErrorViewModel, ErrorViewModel>();
		services.AddSingleton<IConfirmationViewModel, ConfirmationViewModel>();
		services.AddSingleton<IMessageViewModel, MessageViewModel>();
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
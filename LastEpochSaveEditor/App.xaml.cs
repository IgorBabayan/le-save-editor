namespace LastEpochSaveEditor;

public partial class App : Application
{
    private readonly static IHost? _host;

    static App()
    {
        _host = Host.CreateDefaultBuilder()
            .UseSerilog((_, builder) =>
            {
                builder
                    .WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Debug)
                    .WriteTo.File("Logs/log.txt", restrictedToMinimumLevel: LogEventLevel.Error,
                        rollingInterval: RollingInterval.Day);
            })
            .ConfigureServices((_, services) =>
            {
                RegisterMisc(services);
                RegisterWindows(services);
                RegisterControls(services);
                RegisterViewModels(services);
                RegisterMessenger(services);
            }).Build();
    }

    public static TService GetService<TService>()
        where TService : class => _host!.Services.GetRequiredService<TService>();

    private static void RegisterMisc(IServiceCollection services)
    {
        services.AddSingleton<IDB, DB>();
        services.AddSingleton<INavigationService, Utils.NavigationService>();
        services.AddTransient<ICharacterInventory, CharacterInventory>();

        services.AddTransient<Func<Type, ObservableObject>>(services => viewModelType =>
            (ObservableObject)services.GetRequiredService(viewModelType));
    }

    private static void RegisterWindows(IServiceCollection services)
    {
        services.AddSingleton<MainWindow>(provider => new MainWindow
        {
            DataContext = provider.GetRequiredService<MainViewModel>()
        });
    }

    private static void RegisterControls(IServiceCollection services)
    {
        services.AddSingleton<CharacterView>();
        services.AddSingleton<CharacterStashView>();
        services.AddSingleton<BlessingView>();
        services.AddSingleton<IdolView>();
        services.AddSingleton<DownloadWindow>();
        services.AddSingleton<ItemWindow>();
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<DownloadViewModel>();
        services.AddSingleton<CharacterViewModel>();
        services.AddSingleton<CharacterStashViewModel>();
        services.AddSingleton<BlessingViewModel>();
        services.AddSingleton<IdolViewModel>();
        services.AddSingleton<ItemViewModel>();
    }

    private static void RegisterMessenger(IServiceCollection services)
    {
        services.AddSingleton<WeakReferenceMessenger>();
        services.AddSingleton<IMessenger, WeakReferenceMessenger>(provider =>
            provider.GetRequiredService<WeakReferenceMessenger>());
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host!.StartAsync();

        var db = _host.Services.GetRequiredService<IDB>();
        await db.Load();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        var viewModel = _host!.Services.GetRequiredService<CharacterViewModel>();
        WeakReferenceMessenger.Default.UnregisterAll(viewModel);

        await _host!.StopAsync();
        base.OnExit(e);
    }
}
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
				services.RegisterServices();
				services.RegisterFactories();
				services.RegisterMisc();
				services.RegisterWindows();
				services.RegisterControls();
				services.RegisterViewModels();
				services.RegisterMessenger();
			}).Build();
	}

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

	public static TService GetService<TService>()
		where TService : class => _host!.Services.GetRequiredService<TService>();

	public override async void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			await _host!.StartAsync();

			var mainWindow = _host.Services.GetRequiredService<MainWindow>();
			desktop.MainWindow = mainWindow;

			var databaseFactory = GetService<IDatabaseFactory>();
			await databaseFactory.Create();

			desktop.Exit += async (s, e) =>
			{
				var mainViewModel = _host!.Services.GetRequiredService<MainViewModel>();
				var characterViewModel = _host!.Services.GetRequiredService<CharacterViewModel>();
				var itemViewModel = _host!.Services.GetRequiredService<ItemViewModel>();

				WeakReferenceMessenger.Default.UnregisterAll(mainViewModel);
				WeakReferenceMessenger.Default.UnregisterAll(characterViewModel);
				WeakReferenceMessenger.Default.UnregisterAll(itemViewModel);

				await _host!.StopAsync();
			};
		}

		base.OnFrameworkInitializationCompleted();
	}
}
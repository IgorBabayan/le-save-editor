﻿namespace LastEpochSaveEditor;

public partial class App
{
    private static readonly IHost? _host;

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

    public static TService GetService<TService>()
        where TService : class => _host!.Services.GetRequiredService<TService>();

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host!.StartAsync();

        var databaseFactory = GetService<IDatabaseFactory>();
        await databaseFactory.Create();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        var mainViewModel = _host!.Services.GetRequiredService<MainViewModel>();
        var characterViewModel = _host.Services.GetRequiredService<CharacterViewModel>();
        var itemViewModel = _host.Services.GetRequiredService<ItemViewModel>();

        WeakReferenceMessenger.Default.UnregisterAll(mainViewModel);
        WeakReferenceMessenger.Default.UnregisterAll(characterViewModel);
        WeakReferenceMessenger.Default.UnregisterAll(itemViewModel);

        await _host.StopAsync();
        base.OnExit(e);
    }
}
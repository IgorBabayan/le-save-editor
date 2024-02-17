using CommunityToolkit.Mvvm.Messaging;
using LastEpochSaveEditor.Controls;
using LastEpochSaveEditor.Models.Characters;
using LastEpochSaveEditor.Utils;
using LastEpochSaveEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Windows;

namespace LastEpochSaveEditor
{
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
						.WriteTo.File("Logs/log.txt", restrictedToMinimumLevel: LogEventLevel.Error, rollingInterval: RollingInterval.Day);
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

        public App() { }

		public static TService GetService<TService>()
			where TService : class => _host!.Services.GetRequiredService<TService>();

		private static void RegisterMisc(IServiceCollection services)
		{
			services.AddSingleton<IDB, DB>();
			services.AddTransient<CharacterInventory>();
		}

		private static void RegisterWindows(IServiceCollection services)
		{
			services.AddSingleton<MainWindow>(provider => new MainWindow
			{
				DataContext = provider.GetRequiredService<MainViewModel>()
			});
			/*services.AddSingleton<DownloadWindow>(provider => new DownloadWindow
			{
				DataContext = provider.GetRequiredService<DownloadViewModel>()
			});*/
		}

		private static void RegisterControls(IServiceCollection services)
		{
			services.AddSingleton<CharacterControl>();
			services.AddSingleton<CharacterStashControl>();
			services.AddSingleton<BlessingControl>();
			services.AddSingleton<IdolControl>();
			services.AddTransient<DownloadWindow>();
		}

		private static void RegisterViewModels(IServiceCollection services)
		{
			services.AddSingleton<MainViewModel>();
			services.AddSingleton<DownloadViewModel>();
			services.AddSingleton<CharacterViewModel>();
			services.AddSingleton<CharacterStashViewModel>();
			services.AddSingleton<BlessingViewModel>();
			services.AddSingleton<IdolViewModel>();
		}

		private static void RegisterMessenger(IServiceCollection services)
		{
			services.AddSingleton<WeakReferenceMessenger>();
			services.AddSingleton<IMessenger, WeakReferenceMessenger>(provider => provider.GetRequiredService<WeakReferenceMessenger>());
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
}

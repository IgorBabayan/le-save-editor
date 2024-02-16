using CommunityToolkit.Mvvm.Messaging;
using LastEpochSaveEditor.Utils;
using LastEpochSaveEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace LastEpochSaveEditor
{
	public partial class App : Application
	{
		public static IHost? Host { get; private set; }

        public App()
        {
			Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
				.ConfigureServices((_, services) => 
				{
					RegisterMisc(services);
					RegisterWindows(services);
					RegisterViewModels(services);
					RegisterMessenger(services);
				}).Build();
        }

		private void RegisterMisc(IServiceCollection services)
		{
			services.AddSingleton<IDB, DB>();
		}

		private void RegisterWindows(IServiceCollection services)
		{
			services.AddSingleton<MainWindow>(provider => new MainWindow
			{
				DataContext = provider.GetRequiredService<MainViewModel>()
			});
		}

		private void RegisterViewModels(IServiceCollection services)
		{
			services.AddSingleton<MainViewModel>();
			services.AddSingleton<CharacterViewModel>();
			services.AddSingleton<CharacterStashViewModel>();
			services.AddSingleton<BlessingViewModel>();
			services.AddSingleton<IdolViewModel>();
		}

		private void RegisterMessenger(IServiceCollection services)
		{
			services.AddSingleton<WeakReferenceMessenger>();
			services.AddSingleton<IMessenger, WeakReferenceMessenger>(provider => provider.GetRequiredService<WeakReferenceMessenger>());
		}

		protected override async void OnStartup(StartupEventArgs e)
		{
			await Host!.StartAsync();

			var db = Host.Services.GetRequiredService<IDB>();
			await db.Load();

			var mainWindow = Host.Services.GetRequiredService<MainWindow>();
			mainWindow.Show();

			base.OnStartup(e);
		}

		protected override async void OnExit(ExitEventArgs e)
		{
			var viewModel = Host!.Services.GetRequiredService<CharacterViewModel>();
			WeakReferenceMessenger.Default.UnregisterAll(viewModel);
			
			await Host!.StopAsync();
			base.OnExit(e);
		}
	}
}

using LastEpochSaveEditor.Controls;
using LastEpochSaveEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace LastEpochSaveEditor
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static IHost? AppHost { get; private set; }

        public App()
        {
			AppHost = Host.CreateDefaultBuilder()
				.ConfigureServices((_, services) => 
				{
					RegisterWindows(services);
					RegisterViewModels(services);
					RegisterControls(services);
				}).Build();
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

		private void RegisterControls(IServiceCollection services)
		{
			services.AddSingleton<CharacterControl>(provider => new CharacterControl
			{
				DataContext = provider.GetRequiredService<CharacterViewModel>()
			});
			services.AddSingleton<CharacterStashControl>(provider => new CharacterStashControl
			{
				DataContext = provider.GetRequiredService<CharacterStashViewModel>()
			});
			services.AddSingleton<BlessingControl>(provider => new BlessingControl
			{
				DataContext = provider.GetRequiredService<BlessingViewModel>()
			});
			services.AddSingleton<IdolControl>(provider => new IdolControl
			{
				DataContext = provider.GetRequiredService<IdolViewModel>()
			});
		}

		protected override async void OnStartup(StartupEventArgs e)
		{
			await AppHost!.StartAsync();
			var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
			mainWindow.Show();

			base.OnStartup(e);
		}

		protected override async void OnExit(ExitEventArgs e)
		{
			await AppHost!.StopAsync();

			base.OnExit(e);
		}
	}
}

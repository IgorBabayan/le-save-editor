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
				.ConfigureServices((host, services) => 
				{
					services.AddSingleton<MainWindow>();
					services.AddSingleton<MainViewModel>();
				}).Build();
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

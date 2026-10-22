using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Presentation.ViewModels.Settings;
using LE.SaveEditor.Presentation.ViewModels.Shell;
using LE.SaveEditor.Presentation.ViewModels.Updates;
using LE.SaveEditor.Presentation.Views;
using LE.SaveEditor.Presentation.Views.Settings;
using LE.SaveEditor.Presentation.Views.Updates;
using LE.SaveEditor.Services.Saves;
using LE.SaveEditor.Services.Settings;
using LE.SaveEditor.Services.Theming;
using LE.SaveEditor.Services.Updates;

namespace LE.SaveEditor.Extensions;

static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplicationServices() => services
            .AddInfrastructure()
            .AddPresentation();

        private IServiceCollection AddInfrastructure()
        {
            services.AddSingleton<ISettingsService, JsonSettingsService>();
            services.AddSingleton<IThemeService, ThemeService>();
            services.AddSingleton<IUpdateService, GitHubUpdateService>();

            // Save files
            services.AddSingleton<ISaveFolderScanner, SaveFolderScanner>();
            services.AddSingleton<ICharacterSaveParser, CharacterSaveParser>();
            services.AddSingleton<ISaveFolderLoader, SaveFolderLoader>();

            return services;
        }

        private IServiceCollection AddPresentation()
        {
            // Services
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IFolderPickerService, FolderPickerService>();
            services.AddSingleton<IAutoUpdater, AutoUpdater>();

            // Shell
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();

            // Settings (new instance every time the dialog opens)
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsWindow>();

            // Startup update progress window
            services.AddTransient<UpdateViewModel>();
            services.AddTransient<UpdateWindow>();

            return services;
        }
    }
}

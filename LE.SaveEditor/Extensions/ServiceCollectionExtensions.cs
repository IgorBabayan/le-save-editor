using LE.SaveEditor.Presentation.Services;
using LE.SaveEditor.Presentation.ViewModels.Settings;
using LE.SaveEditor.Presentation.ViewModels.Shell;
using LE.SaveEditor.Presentation.Views;
using LE.SaveEditor.Presentation.Views.Settings;
using LE.SaveEditor.Services.Settings;
using LE.SaveEditor.Services.Theming;

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

            return services;
        }

        private IServiceCollection AddPresentation()
        {
            // Services
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IFolderPickerService, FolderPickerService>();

            // Shell
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();

            // Settings (new instance every time the dialog opens)
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsWindow>();

            return services;
        }
    }
}

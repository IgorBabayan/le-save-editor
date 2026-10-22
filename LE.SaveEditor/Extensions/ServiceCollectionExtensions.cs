using LE.SaveEditor.Presentation.ViewModels.Shell;
using LE.SaveEditor.Presentation.Views;

namespace LE.SaveEditor.Extensions;

static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplicationServices() => services.AddPresentation();

        private IServiceCollection AddPresentation()
        {
            // Shell
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();

            return services;
        }
    }
}

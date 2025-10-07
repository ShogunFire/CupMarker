using CupMarker.ViewModels;
using CupMarker.Views;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace CupMarker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public App()
        {
            var services = new ServiceCollection();

            // register your viewmodels
            services.AddTransient<MainViewModel>();

            // register views
            services.AddTransient<MainWindow>();

            // register any services (e.g. IFileService, ISettingsService, etc.)
            // services.AddSingleton<IFileService, FileService>();

            Services = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // resolve and show main window
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }

}

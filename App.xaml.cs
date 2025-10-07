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
            DateTime expiry = new DateTime(2025, 11, 11); // demo expires after this date

            var demoExpired = DateTime.Now > expiry;
            if (demoExpired)
            {
                MessageBox.Show(
                    "Demo version expired. Please contact the developer for the full version.",
                    "Demo Expired",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                // Optional: stop app from launching
                Shutdown();
                return;
            }

            // resolve and show main window
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }

}

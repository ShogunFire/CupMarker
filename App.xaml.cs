using CupMarker.Models;
using CupMarker.Services;
using CupMarker.Services.Interfaces;
using CupMarker.ViewModels;
using CupMarker.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

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
            services.AddTransient<CupControlViewModel>();
            services.AddTransient<LoginControlViewModel>();

            // register views
            services.AddTransient<MainWindow>();
            services.AddTransient<CupControl>();
            services.AddTransient<LoginControl>();

            // register any services (e.g. IFileService, ISettingsService, etc.)
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<INetSuiteApiService, NetSuiteApiService>();
            services.AddSingleton<ISessionService, SessionService>();

            Services = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            string folderPath = Path.Combine(Environment.CurrentDirectory, "tmp_images");
            try
            {
                if (Directory.Exists(folderPath))
                {
                    foreach (var file in Directory.GetFiles(folderPath))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            // Ignore if the file is in use or can't be deleted
                        }
                    }
                }
            }
            catch
            {
                // Silent fail — nothing shown
            }

           

            var nav = Services.GetRequiredService<INavigationService>();
            nav.Navigate<LoginControlViewModel>();

            // resolve and show main window
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }

}

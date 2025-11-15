using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using CupMarker.Services.Interfaces;
using CupMarker.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;
using CupMarker.ViewModels;

namespace CupMarker.Services
{
    public partial class NavigationService : ObservableObject, INavigationService
    {
        private readonly IServiceProvider serviceProvider;

        [ObservableProperty]
        private BitmapImage clientLogo;

        [ObservableProperty]
        private BaseViewModel currentViewModel;

        // Event for subscribers
        public event Action? CurrentViewModelChanged;

        public NavigationService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void Navigate<TViewModel>() where TViewModel : BaseViewModel
        {
            CurrentViewModel = serviceProvider.GetRequiredService<TViewModel>();

            // Fire the event whenever CurrentViewModel changes
            CurrentViewModelChanged?.Invoke();
        }
    }
}

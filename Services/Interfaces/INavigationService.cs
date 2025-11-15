using CupMarker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CupMarker.Services.Interfaces
{
    public interface INavigationService
    {
        void Navigate<TViewModel>() where TViewModel : BaseViewModel;
        BaseViewModel CurrentViewModel { get; }
        event Action? CurrentViewModelChanged;
    }
}

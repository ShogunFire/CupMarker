using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CupMarker.Models;
using CupMarker.Services;
using CupMarker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CupMarker.ViewModels
{
    public partial class LoginControlViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly INetSuiteApiService _apiService;
        private readonly ISessionService _sessionService;

        [ObservableProperty] private string username;
        [ObservableProperty] private string password;
        [ObservableProperty] private string errorMessage;

        public IAsyncRelayCommand LoginCommand { get; }
        private List<Employee> employees;

        public LoginControlViewModel(INavigationService navigationService, INetSuiteApiService apiService, ISessionService sessionService)
        {
            _navigationService = navigationService;
            _apiService = apiService;
            _sessionService = sessionService;

            LoginCommand = new AsyncRelayCommand(LoginAsync);
           
        }

        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

            // Retrieve employees from API
            if(employees == null)
                employees = (await _apiService.GetEmployeeListAsync()).Employees;
            // Find user
            var user = employees.FirstOrDefault(e => e.Username == Username && e.Password == Password);

            if (user == null)
            {
                ErrorMessage = "Invalid username or password";
                return;
            }
            _sessionService.Login(user);


            _navigationService.Navigate<CupControlViewModel>();
        }
    }
}

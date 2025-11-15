using CupMarker.Models;
using CupMarker.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CupMarker.Services
{
    public class SessionService : ISessionService
    {
        public Employee? CurrentUser { get; private set; }

        public void Login(Employee user)
        {
            CurrentUser = user;
        }

        public void Logout()
        {
            CurrentUser = null;
        }

        public bool IsLoggedIn => CurrentUser != null;
    }
}

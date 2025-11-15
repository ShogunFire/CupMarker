using CupMarker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CupMarker.Services.Interfaces
{
    public interface ISessionService
    {
        Employee? CurrentUser { get; }

        bool IsLoggedIn { get; }

        void Login(Employee user);

        void Logout();
    }
}

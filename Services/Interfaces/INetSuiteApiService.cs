using CupMarker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CupMarker.Services.Interfaces
{
    public interface INetSuiteApiService
    {
        Task<EmployeeListResponse> GetEmployeeListAsync();

        Task<bool> SetPersOperatorAsync(string barcode, string operatorText);
    }
}

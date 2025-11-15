using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CupMarker.Models
{
    public class EmployeeListResponse
    {
        [JsonProperty("employees")]
        public List<Employee> Employees { get; set; }
    }

    public class Employee
    {
        [JsonProperty("empid")]
        public string EmpId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("pawsUsername")]
        public string Username { get; set; }

        [JsonProperty("pawsPassword")]
        public string Password { get; set; }
    }
}

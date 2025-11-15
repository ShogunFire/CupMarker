using CupMarker.Models;
using CupMarker.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CupMarker.Services
{
    public class NetSuiteApiService : INetSuiteApiService
    {
        private readonly string consumerKey = "e05657cf2d8c89d5e819f7031df0c49f49298ee6b9f710fb027a0f9da126176d";
        private readonly string consumerSecret = "7e0d98406d4094c1c7d301a9563e7202ab0b2c8bed616c11e045d4a2c8b96c2e";
        private readonly string token = "136aee4a2af579e53ec6a8f24af5b85fbad2776d0d99847fa4101b7b33cd8882";
        private readonly string tokenSecret = "d52d3602753362f4267d7be67d08a4ad17d2c05d8f0982acb1fdb6d47b0c8b44";
        private readonly string realm = "7913744";

        private readonly HttpClient _httpClient;

        public NetSuiteApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<EmployeeListResponse> GetEmployeeListAsync()
        {
            var url = "https://7913744.restlets.api.netsuite.com/app/site/hosting/restlet.nl?script=2276&deploy=1";

            var authHeader = GenerateOAuthHeader(url, "GET");

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", authHeader);
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<EmployeeListResponse>(json);
        }


        public async Task<bool> SetPersOperatorAsync(string barcode, string operatorText)
        {
            var url = "https://7913744.restlets.api.netsuite.com/app/site/hosting/restlet.nl?script=2808&deploy=1";

            var authHeader = GenerateOAuthHeader(url, "PUT");

            var body = new
            {
                custcol_customization_barcode = barcode,
                custcol_pir_pers_operator = operatorText
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Add("Authorization", authHeader);
            request.Headers.Add("Accept", "application/json");
            request.Content = content; // ✅ Content-Type goes here, not in request.Headers

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        private string GenerateOAuthHeader(string url, string method)
        {
            var uri = new Uri(url);
            var nonce = Guid.NewGuid().ToString("N");
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            // Parse query params from URL (script=xxxx&deploy=1, etc.)
            var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var parameters = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key", consumerKey },
                { "oauth_token", token },
                { "oauth_nonce", nonce },
                { "oauth_timestamp", timestamp },
                { "oauth_signature_method", "HMAC-SHA256" },
                { "oauth_version", "1.0" }
            };

            foreach (var key in queryParams.AllKeys)
                parameters.Add(key, queryParams[key]);

            // Build base string
            var baseString = $"{method.ToUpper()}&{Uri.EscapeDataString(uri.GetLeftPart(UriPartial.Path))}&{Uri.EscapeDataString(string.Join("&", parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}")))}";

            var signingKey = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(tokenSecret)}";

            using var hasher = new HMACSHA256(Encoding.ASCII.GetBytes(signingKey));
            var signature = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString)));
            parameters.Add("oauth_signature", signature);

            var header = $"OAuth realm=\"{realm}\", " + string.Join(", ", parameters
                .Where(kvp => kvp.Key.StartsWith("oauth_"))
                .Select(kvp => $"{kvp.Key}=\"{Uri.EscapeDataString(kvp.Value)}\""));

            return header;
        }
    }

}

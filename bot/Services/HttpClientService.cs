using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace CoreBotCLU
{
    public class HttpClientService
    {
        private readonly HttpClient _httpClient;

        public HttpClientService(string baseUri)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUri) };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, string content, Dictionary<string, string> queryParameters = null)
        {
            // Construct the query string if query parameters are provided
            var request = GetQueryParameters(requestUri, queryParameters);

            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(request, stringContent);
        }

        public async Task<string> PostAsync(string requestUri, object data, Dictionary<string, string> queryParameters = null)
        {
            var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            var response = await PostAsync(requestUri, jsonContent, queryParameters);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return null; // Handle error cases as needed
        }

    public async Task<HttpResponseMessage> PutAsync(string requestUri, string content, Dictionary<string, string> queryParameters = null)
    {
         // Construct the query string if query parameters are provided
            var request = GetQueryParameters(requestUri, queryParameters);

        var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
        return await _httpClient.PutAsync(request, stringContent);
    }

    public async Task<string> PutAsync(string requestUri, object data, Dictionary<string, string> queryParameters = null)
    {
        var jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        var response = await PutAsync(requestUri, jsonContent, queryParameters);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        return null; // Handle error cases as needed
    }

        private string GetQueryParameters(string requestUri,Dictionary<string, string> queryParameters)
        {
           // Construct the query string if query parameters are provided
            if (queryParameters != null && queryParameters.Count > 0)
            {
                var queryString = new StringBuilder();
                foreach (var kvp in queryParameters)
                {
                    queryString.Append($"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}&");
                }
                requestUri += "?" + queryString.ToString().TrimEnd('&');
            }
            return requestUri;
        }
    }
}

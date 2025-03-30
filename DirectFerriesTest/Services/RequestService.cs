using DirectFerriesTest.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DirectFerriesTest.Services
{
    public class RequestService : IRequestService
    {
        private readonly HttpClient _client;
        private readonly ILogger<RequestService> _logger;

        public RequestService(HttpClient client, ILogger<RequestService> logger)
        {
            _client = client;
            _logger = logger;
        }

        private void SetAuthorization(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }


        public async Task<string> GetAsync(string url, string token)
        {
            try
            {
                SetAuthorization(token);

                // get the data from the specified url
                var response = await _client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Get request failed");
                }

                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Successfully retrieved content from {url}.");

                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving content from {url}");
                throw;
            }
        }

        public async Task<string> PutAsync(string url, object data, string token)
        {
            try
            {
                SetAuthorization(token);

                var json = JsonSerializer.Serialize(data);
                var contentToPut = new StringContent(json);

                // send off our data to be updated
                var response = await _client.PutAsJsonAsync(url, contentToPut);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Put request failed");
                }

                // get the updated content response
                var content = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Successfully put content {url}: {data}.");

                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating content {url}");
                throw;
            }
        }
    }
}

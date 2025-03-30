using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using DirectFerriesTest.Interfaces;
using System.Security.Authentication;

namespace DirectFerriesTest;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _client;
    private readonly ILogger<ProductService> _logger;

    public AuthenticationService(HttpClient client, ILogger<ProductService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("/auth/login", new { username, password });

            if (!response.IsSuccessStatusCode)
            {
                throw new AuthenticationException("Invalid credentials");
            }

            var json = await response.Content.ReadAsStringAsync();
            var token = JsonDocument.Parse(json).RootElement.GetProperty("accessToken").GetString();

            if (token is null)
            {
                throw new NullReferenceException("Failed to retrieve token from the API.");
            }

            _logger.LogInformation("Login successful. Token: {token}", token);

            return token;
        } catch (Exception ex)
        {
            _logger.LogError(ex, $"Error logging in");
            throw;
        }
    }
}
using DirectFerriesTest.Models;

namespace DirectFerriesTest.Interfaces;

public interface IAuthenticationService
{ 
    Task<string> LoginAsync(string username, string password);
}
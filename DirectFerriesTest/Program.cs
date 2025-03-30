// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using DirectFerriesTest;
using DirectFerriesTest.Interfaces;
using Microsoft.Extensions.Logging;
using DirectFerriesTest.Models;
using System.Security.Authentication;
using DirectFerriesTest.Services;

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    builder.AddConsole();
});

const string baseUrl = "https://dummyjson.com";

services.AddHttpClient<IAuthenticationService, AuthenticationService>(client =>
{
    client.BaseAddress = new Uri(baseUrl);
});

services.AddHttpClient<IRequestService, RequestService>(client =>
{
    client.BaseAddress = new Uri(baseUrl);
});

services.AddScoped<IProductService, ProductService>();

var serviceProvider = services.BuildServiceProvider();
var authService = serviceProvider.GetRequiredService<IAuthenticationService>();
var productService = serviceProvider.GetRequiredService<IProductService>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

try
{
    Console.WriteLine("--- Smartphone Price Updater ---");

    string authToken = string.Empty;

    // keep the application running until a valid auth token is received
    while (true)
    {
        Console.Write("Enter your username: ");
        string? username = Console.ReadLine();

        Console.Write("Enter your password: ");
        string? password = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("Username and/or password cannot be empty");
            continue;
        }

        // attempt to login with the provided inputs!
        try
        {
            authToken = await authService.LoginAsync(username!, password!);
            break;
        } catch (AuthenticationException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    // get the most expensive phones from api and then take 3
    var mostExpensiveThreePhones = await productService.GetMostExpensivePhones(authToken, 3);

    Console.WriteLine("Top 3 Most Expensive Smartphones:");

    foreach (var phone in mostExpensiveThreePhones)
    {
        Console.WriteLine($"Brand: {phone.Brand}, Title: {phone.Title}, Price: {phone.Price}");
    }

    Console.WriteLine("--- It's price hike time! ---");


    // time for the user to enter a percentage to increase the price of the phones
    decimal percentage;

    while (true)
    {
        Console.Write("Enter a number (1-100) to increase each phone's price by that percentage: ");
        string? input = Console.ReadLine();

        if (decimal.TryParse(input, out percentage) && percentage >= 1 && percentage <= 100)
        {
            break;
        }
        else
        {
            Console.WriteLine("Invalid input! Enter a number between 1 and 100.");
        }
    }


    // calculate the new price for each phone and update
    var updatedPhones = new List<Product>();

    foreach (var phone in mostExpensiveThreePhones)
    {
        phone.Price = productService.IncreaseProductPriceByPercentage(phone.Price, percentage);


        // update the product price via the api and add it to the updated phone list
        var updatedProduct = await productService.UpdateProductPrice(phone, authToken);

        updatedPhones.Add(updatedProduct);
    }

    // output the updated phones
    Console.WriteLine("Updated Smartphones Prices:");

    foreach (var phone in updatedPhones)
    {
        Console.WriteLine($"Brand: {phone.Brand}, Title: {phone.Title}, Price: {phone.Price}");
    }

}
catch (Exception)
{
    // display a generic error message to the user
    Console.WriteLine($"An error has occurred!");
}
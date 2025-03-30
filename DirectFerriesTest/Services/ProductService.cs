using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DirectFerriesTest.Interfaces;
using DirectFerriesTest.Models;
using DirectFerriesTest.Services;
using Microsoft.Extensions.Logging;

namespace DirectFerriesTest;

public class ProductService : IProductService
{
    private readonly ILogger<ProductService> _logger;
    private readonly IRequestService _requestService;

    public ProductService(ILogger<ProductService> logger, IRequestService requestService)
    {
        _logger = logger;
        _requestService = requestService;
    }


    private async Task<List<Product>> GetSmartphones(string token)
    {
        try
        {
            var requestContent = await _requestService.GetAsync("/products/category/smartphones", token);

            using var jsonDoc = JsonDocument.Parse(requestContent);

            // get the products list from the json response
            var productsJson = jsonDoc.RootElement.GetProperty("products");

            // now turn that json into a list of Product objects
            var products = JsonSerializer.Deserialize<List<Product>>(productsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            //if the products list is empty or null, something is wrong
            if (products is null || !products.Any())
            {
                throw new InvalidOperationException("Failed to deserialize the smartphone list response from the API.");
            }

            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting smartphones");
            throw;
        }
    }
    
    public async Task<List<Product>> GetMostExpensivePhones(string token, int numberOfPhonesToTake)
    {
        var smartPhones = await GetSmartphones(token);

        // take only 3 most expensive phones
        return smartPhones
            .OrderByDescending(p => p.Price)
            .Take(3)
            .ToList();
    }

    public async Task<Product> UpdateProductPrice(Product product, string token)
    {
        try
        {
            // send off the product to be updated
            var requestContent =  await _requestService.PutAsync($"/products/{product.Id}", product.Price, token);

            // turn the response into a Product object
            var updatedProduct = JsonSerializer.Deserialize<Product>(requestContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // if the updated product is null here, something has gone wrong
            if (updatedProduct is null)
            {
                throw new InvalidOperationException("Failed to deserialize the updated product response from the API.");
            }

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating product {product.Id}");
            throw;
        }
    }


    public decimal IncreaseProductPriceByPercentage(decimal productPrice, decimal percentage)
    {
        // apply the percentage increase to the product price
        return productPrice += Math.Round(productPrice * (percentage / 100), 2);
    }
}
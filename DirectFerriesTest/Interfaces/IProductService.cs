using DirectFerriesTest.Models;

namespace DirectFerriesTest.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetMostExpensivePhones(string authToken, int numberOfPhonesToTake);
    decimal IncreaseProductPriceByPercentage(decimal productPrice, decimal percentage);
    Task<Product> UpdateProductPrice(Product product, string authToken);
}
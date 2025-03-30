using DirectFerriesTest.Interfaces;
using DirectFerriesTest.Models;
using DirectFerriesTest.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace DirectFerriesTest.Tests.Services
{
    public class Tests
    {

        private Mock<ILogger<ProductService>> _mockProductLogger;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private Mock<IRequestService> _mockRequestService;
        private IProductService _productService;

        [SetUp]
        public void Setup()
        {
            // mock httpmessagehander to inject into HttpClient
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            // mock the logger
            _mockProductLogger = new Mock<ILogger<ProductService>>();

            //mock the request service
            _mockRequestService = new Mock<IRequestService>();

            _productService = new ProductService(_mockProductLogger.Object, _mockRequestService.Object);
        }

        [Test]
        public async Task ProductService_IncreaseProductPriceByPercentage_UpdatesPricesCorrectly()
        {
            // arange
            var product = new Product() { Id = 1, Brand = "Samsung", Title = "Galaxy S22", Price = 1000 };
            decimal percentage = 10; // 10% increase

            // act
            var newPrice = _productService.IncreaseProductPriceByPercentage(product.Price, percentage);

            // assert
            Assert.AreEqual(1100, newPrice);  // 1000 + 10% = 1100
        }

        [Test]
        public async Task UpdateProductPrice_ShouldReturnUpdatedProduct_WhenApiReturnsValidResponse()
        {
            // arrange
            var testProduct = new Product { Id = 1, Brand = "Apple", Title = "iPhone 15", Price = 1000 };
            var updatedProductJson = JsonSerializer.Serialize(testProduct);
            var token = "test-token";

            _mockRequestService
                .Setup(rs => rs.PutAsync($"/products/{testProduct.Id}", testProduct.Price, token))
                .ReturnsAsync(updatedProductJson);

            // act
            var result = await _productService.UpdateProductPrice(testProduct, token);

            // assert
            Assert.NotNull(result);
            Assert.AreEqual(testProduct.Id, result.Id);
            Assert.AreEqual(testProduct.Brand, result.Brand);
            Assert.AreEqual(testProduct.Price, result.Price);
        }

        [Test]
        public async Task GetMostExpensivePhones_ShouldReturnTopMostExpensivePhones()
        {
            // arrange
            var token = "test-token";
            var testProducts = new List<Product>
            {
                new Product { Id = 1, Brand = "Apple", Title = "iPhone 17", Price = 1500 },
                new Product { Id = 2, Brand = "Samsung", Title = "Galazy S22", Price = 1100 },
                new Product { Id = 3, Brand = "Apple", Title = "iPhone 16", Price = 1200 },
                new Product { Id = 4, Brand = "Apple", Title = "iPhone 5", Price = 400 },
                new Product { Id = 5, Brand = "Apple", Title = "iPhone 4", Price = 200 },
            };

            var jsonResponse = JsonSerializer.Serialize(new { products = testProducts });

            _mockRequestService
                .Setup(rs => rs.GetAsync("/products/category/smartphones", token))
                .ReturnsAsync(jsonResponse);

            // act
            var result = await _productService.GetMostExpensivePhones(token, 3);

            // assert
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0].Price, Is.EqualTo(1500));
            Assert.That(result[1].Price, Is.EqualTo(1200));
            Assert.That(result[2].Price, Is.EqualTo(1100));
        }
    }
}
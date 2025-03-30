namespace DirectFerriesTest.Models;

public class Product
{
    public long Id { get; init; }
    public string Brand { get; init; } = null!;
    public string Title { get; init; } = null!;
    public decimal Price { get; set; }
}
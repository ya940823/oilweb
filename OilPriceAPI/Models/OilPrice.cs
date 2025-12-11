namespace OilPriceAPI.Models;

public class OilPrice
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Company { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

namespace OilPriceAPI.Models;

public class OilPriceDto
{
    public DateTime Date { get; set; }
    public string Company { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class OilPriceStatistics
{
    public string FuelType { get; set; } = string.Empty;
    public decimal Average { get; set; }
    public decimal Max { get; set; }
    public decimal Min { get; set; }
}

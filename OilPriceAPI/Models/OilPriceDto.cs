namespace OilPriceAPI.Models;

public class OilPriceDto
{
    public DateTime Date { get; set; }
    public decimal Price92 { get; set; }
    public decimal Price95 { get; set; }
    public decimal Price98 { get; set; }
    public decimal PriceDiesel { get; set; }
}

public class OilPricePredictionDto
{
    public DateTime Date { get; set; }
    public decimal Price92 { get; set; }
    public decimal Price95 { get; set; }
    public decimal Price98 { get; set; }
    public decimal PriceDiesel { get; set; }
    public bool IsPrediction { get; set; }
}

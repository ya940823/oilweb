namespace OilPriceAPI.Models;

public class OilPriceDto
{
    public DateTime Date { get; set; }
    public decimal Gas92 { get; set; }
    public decimal Gas95 { get; set; }
    public decimal Gas98 { get; set; }
    public decimal Diesel { get; set; }
    public decimal? Gas92Change { get; set; }
    public decimal? Gas95Change { get; set; }
    public decimal? Gas98Change { get; set; }
    public decimal? DieselChange { get; set; }
}

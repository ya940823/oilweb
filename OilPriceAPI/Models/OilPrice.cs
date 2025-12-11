namespace OilPriceAPI.Models;

public class OilPrice
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Gas92 { get; set; }
    public decimal Gas95 { get; set; }
    public decimal Gas98 { get; set; }
    public decimal Diesel { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

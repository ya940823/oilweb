using System.ComponentModel.DataAnnotations;

namespace OilPriceAPI.Models;

public class OilPrice
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public string Company { get; set; } = string.Empty;
    
    [Required]
    public string FuelType { get; set; } = string.Empty;
    
    [Required]
    public decimal Price { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

using System.ComponentModel.DataAnnotations;

namespace OilPriceAPI.Models
{
    public class OilPrice
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        public decimal Oil92 { get; set; }
        
        public decimal Oil95 { get; set; }
        
        public decimal Oil98 { get; set; }
        
        public decimal Diesel { get; set; }
        
        public string Source { get; set; } = "XML";
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

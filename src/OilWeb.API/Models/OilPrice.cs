using System.ComponentModel.DataAnnotations;

namespace OilWeb.API.Models
{
    public class OilPrice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(50)]
        public string OilType { get; set; } = string.Empty; // 92, 95, 98, 柴油

        [Required]
        public decimal Price { get; set; }

        public decimal? PriceChange { get; set; } // 漲跌幅

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

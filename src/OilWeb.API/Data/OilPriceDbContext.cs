using Microsoft.EntityFrameworkCore;
using OilWeb.API.Models;

namespace OilWeb.API.Data
{
    public class OilPriceDbContext : DbContext
    {
        public OilPriceDbContext(DbContextOptions<OilPriceDbContext> options) : base(options)
        {
        }

        public DbSet<OilPrice> OilPrices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OilPrice>(entity =>
            {
                entity.HasIndex(e => new { e.Date, e.OilType }).IsUnique();
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PriceChange).HasColumnType("decimal(18,2)");
            });

            // 種子資料 - 示例油價
            var seedDate = new DateTime(2025, 12, 1);
            modelBuilder.Entity<OilPrice>().HasData(
                new OilPrice { Id = 1, Date = seedDate, OilType = "92無鉛汽油", Price = 28.5m, PriceChange = 0.0m },
                new OilPrice { Id = 2, Date = seedDate, OilType = "95無鉛汽油", Price = 30.0m, PriceChange = 0.0m },
                new OilPrice { Id = 3, Date = seedDate, OilType = "98無鉛汽油", Price = 32.0m, PriceChange = 0.0m },
                new OilPrice { Id = 4, Date = seedDate, OilType = "超級柴油", Price = 26.5m, PriceChange = 0.0m },
                
                new OilPrice { Id = 5, Date = seedDate.AddDays(7), OilType = "92無鉛汽油", Price = 28.8m, PriceChange = 0.3m },
                new OilPrice { Id = 6, Date = seedDate.AddDays(7), OilType = "95無鉛汽油", Price = 30.3m, PriceChange = 0.3m },
                new OilPrice { Id = 7, Date = seedDate.AddDays(7), OilType = "98無鉛汽油", Price = 32.3m, PriceChange = 0.3m },
                new OilPrice { Id = 8, Date = seedDate.AddDays(7), OilType = "超級柴油", Price = 26.8m, PriceChange = 0.3m },
                
                new OilPrice { Id = 9, Date = seedDate.AddDays(14), OilType = "92無鉛汽油", Price = 28.6m, PriceChange = -0.2m },
                new OilPrice { Id = 10, Date = seedDate.AddDays(14), OilType = "95無鉛汽油", Price = 30.1m, PriceChange = -0.2m },
                new OilPrice { Id = 11, Date = seedDate.AddDays(14), OilType = "98無鉛汽油", Price = 32.1m, PriceChange = -0.2m },
                new OilPrice { Id = 12, Date = seedDate.AddDays(14), OilType = "超級柴油", Price = 26.6m, PriceChange = -0.2m }
            );
        }
    }
}

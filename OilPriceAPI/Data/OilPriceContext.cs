using Microsoft.EntityFrameworkCore;
using OilPriceAPI.Models;

namespace OilPriceAPI.Data
{
    public class OilPriceContext : DbContext
    {
        public OilPriceContext(DbContextOptions<OilPriceContext> options) : base(options)
        {
        }
        
        public DbSet<OilPrice> OilPrices { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<OilPrice>(entity =>
            {
                entity.HasIndex(e => e.Date).IsUnique();
                entity.Property(e => e.Oil92).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Oil95).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Oil98).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Diesel).HasColumnType("decimal(18,2)");
            });
        }
    }
}

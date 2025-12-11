using Microsoft.EntityFrameworkCore;
using OilPriceAPI.Models;

namespace OilPriceAPI.Data;

public class OilPriceContext : DbContext
{
    public OilPriceContext(DbContextOptions<OilPriceContext> options) : base(options)
    {
    }

    public DbSet<OilPrice> OilPrices { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OilPrice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Date).IsUnique();
            entity.Property(e => e.Gas92).HasPrecision(10, 2);
            entity.Property(e => e.Gas95).HasPrecision(10, 2);
            entity.Property(e => e.Gas98).HasPrecision(10, 2);
            entity.Property(e => e.Diesel).HasPrecision(10, 2);
        });
    }
}

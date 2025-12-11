using Microsoft.EntityFrameworkCore;
using OilPriceAPI.Models;

namespace OilPriceAPI.Data;

public class OilPriceContext : DbContext
{
    public OilPriceContext(DbContextOptions<OilPriceContext> options) : base(options)
    {
    }

    public DbSet<OilPrice> OilPrices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OilPrice>()
            .HasIndex(o => new { o.Date, o.Company, o.FuelType })
            .IsUnique();
    }
}

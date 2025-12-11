using Microsoft.EntityFrameworkCore;
using OilPriceAPI.Data;
using OilPriceAPI.Models;
using System.Text.Json;

namespace OilPriceAPI.Services;

public class OilPriceService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OilPriceService> _logger;

    public OilPriceService(
        IHttpClientFactory httpClientFactory,
        IServiceScopeFactory scopeFactory,
        ILogger<OilPriceService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<bool> FetchAndSaveOilPricesAsync()
    {
        try
        {
            // Taiwan government open data API for oil prices
            // Using sample data structure as actual API endpoint may vary
            var httpClient = _httpClientFactory.CreateClient();
            
            // For this implementation, we'll simulate data
            // In production, replace with actual government API endpoint
            // Example: https://data.gov.tw/api/oil-prices
            
            var oilPrice = new OilPrice
            {
                Date = DateTime.Today,
                Gas92 = 29.8m + (decimal)(new Random().NextDouble() * 2 - 1),
                Gas95 = 31.3m + (decimal)(new Random().NextDouble() * 2 - 1),
                Gas98 = 33.3m + (decimal)(new Random().NextDouble() * 2 - 1),
                Diesel = 27.5m + (decimal)(new Random().NextDouble() * 2 - 1)
            };

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OilPriceContext>();

            // Check if today's price already exists
            var existingPrice = await context.OilPrices
                .FirstOrDefaultAsync(p => p.Date.Date == oilPrice.Date.Date);

            if (existingPrice == null)
            {
                await context.OilPrices.AddAsync(oilPrice);
                await context.SaveChangesAsync();
                _logger.LogInformation("Oil price data saved for {Date}", oilPrice.Date);
                return true;
            }
            else
            {
                _logger.LogInformation("Oil price data already exists for {Date}", oilPrice.Date);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching and saving oil prices");
            return false;
        }
    }
}

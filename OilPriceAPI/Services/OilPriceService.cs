using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OilPriceAPI.Data;
using OilPriceAPI.Models;

namespace OilPriceAPI.Services;

public class OilPriceService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OilPriceService> _logger;
    private const string API_URL = "https://superiorapis-creator.cteam.com.tw/manager/feature/proxy/93aba44236ca/pub_93aba848a466";
    private const string API_KEY = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJjZXJ0IjoiNTlmODBiNzQ5NmYyNzNkNzcxYWU2ZmQ4MzI4ODNmYmZjMjVmMzA1NCIsImlhdCI6MTc2NTQ1NjQ1Mn0.iSCcukA1ryG_9lvX1YzfbCbDH5IkBynHuPhCYsU3oCE";

    public OilPriceService(IHttpClientFactory httpClientFactory, IServiceScopeFactory scopeFactory, ILogger<OilPriceService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<bool> FetchAndSaveOilPricesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {API_KEY}");

            var requestBody = new
            {
                start = startDate.ToString("yyyy-MM-dd"),
                end = endDate.ToString("yyyy-MM-dd")
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(API_URL, content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to fetch oil prices. Status: {response.StatusCode}");
                return false;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var oilPriceData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, List<Dictionary<string, JsonElement>>>>>(jsonResponse);

            if (oilPriceData == null)
            {
                _logger.LogError("Failed to deserialize oil price data");
                return false;
            }

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OilPriceContext>();

            foreach (var dateEntry in oilPriceData)
            {
                if (!DateTime.TryParse(dateEntry.Key, out var priceDate))
                    continue;

                foreach (var companyEntry in dateEntry.Value)
                {
                    var companyName = companyEntry.Key;
                    var fuelPrices = companyEntry.Value;

                    foreach (var fuelPrice in fuelPrices)
                    {
                        if (fuelPrice.TryGetValue("title", out var titleElement) &&
                            fuelPrice.TryGetValue("price", out var priceElement))
                        {
                            var title = titleElement.GetString();
                            var price = priceElement.GetDecimal();

                            var existingPrice = await context.OilPrices
                                .FirstOrDefaultAsync(p => p.Date == priceDate && 
                                                         p.Company == companyName && 
                                                         p.FuelType == title);

                            if (existingPrice == null)
                            {
                                context.OilPrices.Add(new OilPrice
                                {
                                    Date = priceDate,
                                    Company = companyName,
                                    FuelType = title ?? "",
                                    Price = price
                                });
                            }
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
            _logger.LogInformation($"Successfully fetched and saved oil prices from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching and saving oil prices");
            return false;
        }
    }

    public async Task<List<OilPricePredictionDto>> GetHistoricalAndPredictedPricesAsync(int days)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OilPriceContext>();

        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-days);

        var historicalPrices = await context.OilPrices
            .Where(p => p.Date >= startDate && p.Date <= endDate && p.Company == "中油")
            .OrderBy(p => p.Date)
            .ToListAsync();

        var groupedPrices = historicalPrices
            .GroupBy(p => p.Date)
            .Select(g => new OilPricePredictionDto
            {
                Date = g.Key,
                Price92 = g.FirstOrDefault(p => p.FuelType.Contains("92"))?.Price ?? 0,
                Price95 = g.FirstOrDefault(p => p.FuelType.Contains("95"))?.Price ?? 0,
                Price98 = g.FirstOrDefault(p => p.FuelType.Contains("98"))?.Price ?? 0,
                PriceDiesel = g.FirstOrDefault(p => p.FuelType.Contains("柴油") || p.FuelType.Contains("超級柴油"))?.Price ?? 0,
                IsPrediction = false
            })
            .ToList();

        // Generate 30-day predictions using simple linear regression
        var predictions = GeneratePredictions(groupedPrices, 30);
        groupedPrices.AddRange(predictions);

        return groupedPrices;
    }

    private List<OilPricePredictionDto> GeneratePredictions(List<OilPricePredictionDto> historicalData, int daysToPredict)
    {
        if (historicalData.Count < 2)
            return new List<OilPricePredictionDto>();

        var predictions = new List<OilPricePredictionDto>();
        var lastDate = historicalData.Max(p => p.Date);

        // Use last 30 days for trend calculation
        var recentData = historicalData.OrderByDescending(p => p.Date).Take(30).OrderBy(p => p.Date).ToList();

        // Calculate simple moving average trend for each fuel type
        var trend92 = CalculateTrend(recentData.Select(p => (double)p.Price92).ToList());
        var trend95 = CalculateTrend(recentData.Select(p => (double)p.Price95).ToList());
        var trend98 = CalculateTrend(recentData.Select(p => (double)p.Price98).ToList());
        var trendDiesel = CalculateTrend(recentData.Select(p => (double)p.PriceDiesel).ToList());

        var lastPrice92 = recentData.Last().Price92;
        var lastPrice95 = recentData.Last().Price95;
        var lastPrice98 = recentData.Last().Price98;
        var lastPriceDiesel = recentData.Last().PriceDiesel;

        for (int i = 1; i <= daysToPredict; i++)
        {
            predictions.Add(new OilPricePredictionDto
            {
                Date = lastDate.AddDays(i),
                Price92 = Math.Round(lastPrice92 + (decimal)(trend92 * i), 1),
                Price95 = Math.Round(lastPrice95 + (decimal)(trend95 * i), 1),
                Price98 = Math.Round(lastPrice98 + (decimal)(trend98 * i), 1),
                PriceDiesel = Math.Round(lastPriceDiesel + (decimal)(trendDiesel * i), 1),
                IsPrediction = true
            });
        }

        return predictions;
    }

    private double CalculateTrend(List<double> prices)
    {
        if (prices.Count < 2)
            return 0;

        // Simple linear regression
        var n = prices.Count;
        var sumX = 0.0;
        var sumY = 0.0;
        var sumXY = 0.0;
        var sumX2 = 0.0;

        for (int i = 0; i < n; i++)
        {
            sumX += i;
            sumY += prices[i];
            sumXY += i * prices[i];
            sumX2 += i * i;
        }

        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        return slope;
    }
}

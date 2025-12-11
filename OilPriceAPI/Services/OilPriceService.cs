using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using OilPriceAPI.Data;
using OilPriceAPI.Models;

namespace OilPriceAPI.Services;

public class OilPriceService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OilPriceService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _apiUrl;
    private readonly string _apiKey;

    public OilPriceService(IHttpClientFactory httpClientFactory, IServiceScopeFactory scopeFactory, ILogger<OilPriceService> logger, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
        _apiUrl = _configuration["OilPriceApi:Url"] ?? "https://vipmbr.cpc.com.tw/cpcstn/listpricewebservice.asmx/getCPCMainProdListPrice_Historical";
        _apiKey = _configuration["OilPriceApi:ApiKey"] ?? "";
    }

    public async Task<bool> FetchAndSaveOilPricesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            // CPC API: Fetch data for each product type (1=92, 2=95, 3=98, 4=diesel)
            var productIds = new[] { "1", "2", "3", "4" };
            var productNames = new Dictionary<string, string>
            {
                { "1", "92無鉛汽油" },
                { "2", "95無鉛汽油" },
                { "3", "98無鉛汽油" },
                { "4", "超級柴油" }
            };

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OilPriceContext>();
            var client = _httpClientFactory.CreateClient();

            foreach (var prodId in productIds)
            {
                try
                {
                    // Make HTTP GET request to CPC API
                    var requestUrl = $"{_apiUrl}?prodid={prodId}";
                    _logger.LogInformation($"Fetching CPC oil prices for product {prodId} ({productNames[prodId]})");
                    
                    var response = await client.GetAsync(requestUrl);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Failed to fetch oil prices for product {prodId}. Status: {response.StatusCode}");
                        continue;
                    }

                    var xmlResponse = await response.Content.ReadAsStringAsync();
                    
                    // Parse XML response
                    var xdoc = XDocument.Parse(xmlResponse);
                    
                    // Extract data from XML structure
                    // Expected format: <ArrayOfMYType><MYType><EffectiveDate>...</EffectiveDate><ReferencePriceNT>...</ReferencePriceNT></MYType>...</ArrayOfMYType>
                    var ns = xdoc.Root?.GetDefaultNamespace() ?? XNamespace.None;
                    var dataElements = xdoc.Descendants(ns + "MYType");

                    foreach (var element in dataElements)
                    {
                        var dateStr = element.Element(ns + "EffectiveDate")?.Value;
                        var priceStr = element.Element(ns + "ReferencePriceNT")?.Value;

                        if (string.IsNullOrEmpty(dateStr) || string.IsNullOrEmpty(priceStr))
                            continue;

                        // Parse date
                        if (!DateTime.TryParse(dateStr, out var priceDate))
                            continue;

                        // Filter by date range
                        if (priceDate < startDate || priceDate > endDate)
                            continue;

                        // Parse price
                        if (!decimal.TryParse(priceStr, out var price))
                            continue;

                        // Save to database
                        var oilPrice = new OilPrice
                        {
                            Date = priceDate.Date,
                            Company = "中油",
                            FuelType = productNames[prodId],
                            Price = price,
                            CreatedAt = DateTime.Now
                        };

                        // Check if record already exists
                        var existing = await context.OilPrices
                            .FirstOrDefaultAsync(p => p.Date.Date == oilPrice.Date.Date && 
                                                     p.Company == oilPrice.Company && 
                                                     p.FuelType == oilPrice.FuelType);

                        if (existing == null)
                        {
                            context.OilPrices.Add(oilPrice);
                        }
                        else
                        {
                            existing.Price = oilPrice.Price;
                            existing.CreatedAt = DateTime.Now;
                        }
                    }

                    _logger.LogInformation($"Processed {dataElements.Count()} records for product {prodId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing product {prodId}: {ex.Message}");
                    continue;
                }

            }

            await context.SaveChangesAsync();
            _logger.LogInformation($"Successfully fetched and saved CPC oil prices from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
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

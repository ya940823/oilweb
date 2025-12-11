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
            // Read from local XML files instead of API
            var xmlFiles = new Dictionary<string, string>
            {
                { "92無鉛汽油", "Data/oil-price-92.xml" },
                { "95無鉛汽油", "Data/oil-price-95.xml" },
                { "98無鉛汽油", "Data/oil-price-98.xml" },
                { "超級柴油", "Data/oil-price-diesel.xml" }
            };

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OilPriceContext>();

            foreach (var kvp in xmlFiles)
            {
                var fuelType = kvp.Key;
                var xmlPath = kvp.Value;

                try
                {
                    _logger.LogInformation($"Loading oil prices from {xmlPath} for {fuelType}");
                    
                    // Check if file exists
                    if (!File.Exists(xmlPath))
                    {
                        _logger.LogWarning($"XML file not found: {xmlPath}. Skipping {fuelType}.");
                        continue;
                    }

                    // Read XML file
                    var xmlContent = await File.ReadAllTextAsync(xmlPath);
                    var xdoc = XDocument.Parse(xmlContent);
                    
                    // Parse CPC XML format with namespaces
                    // Format: <DataSet><diffgr:diffgram><NewDataSet><tbTable><牌價生效時間>...</牌價生效時間><產品名>...</產品名><參考牌價>...</參考牌價></tbTable>...
                    XNamespace ns = "http://tmtd.cpc.com.tw/";
                    XNamespace diffgr = "urn:schemas-microsoft-com:xml-diffgram-v1";
                    
                    var dataElements = xdoc.Descendants("tbTable");
                    
                    int recordCount = 0;
                    foreach (var element in dataElements)
                    {
                        var dateStr = element.Element("牌價生效時間")?.Value;
                        var priceStr = element.Element("參考牌價")?.Value;
                        var productName = element.Element("產品名")?.Value;

                        if (string.IsNullOrEmpty(dateStr) || string.IsNullOrEmpty(priceStr))
                            continue;

                        // Parse date
                        if (!DateTime.TryParse(dateStr, out var priceDate))
                            continue;

                        // Filter by date range (optional - you can remove this to load all data)
                        // Commented out to load all historical data from XML
                        // if (priceDate < startDate || priceDate > endDate)
                        //     continue;

                        // Parse price
                        if (!decimal.TryParse(priceStr, out var price))
                            continue;

                        // Save to database
                        var oilPrice = new OilPrice
                        {
                            Date = priceDate.Date,
                            Company = "中油",
                            FuelType = fuelType,
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
                            recordCount++;
                        }
                        else
                        {
                            existing.Price = oilPrice.Price;
                            existing.CreatedAt = DateTime.Now;
                        }
                    }

                    _logger.LogInformation($"Loaded {recordCount} new records from {xmlPath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing {xmlPath}: {ex.Message}");
                    continue;
                }
            }

            await context.SaveChangesAsync();
            _logger.LogInformation($"Successfully loaded CPC oil prices from local XML files");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading oil prices from XML files");
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

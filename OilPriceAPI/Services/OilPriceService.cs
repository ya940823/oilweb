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
    private readonly IConfiguration _configuration;
    private readonly string _apiUrl;
    private readonly string _apiKey;

    public OilPriceService(IHttpClientFactory httpClientFactory, IServiceScopeFactory scopeFactory, ILogger<OilPriceService> logger, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
        _apiUrl = _configuration["OilPriceApi:Url"] ?? "https://superiorapis-creator.cteam.com.tw/manager/feature/proxy/93aba44236ca/pub_93aba848a466";
        _apiKey = _configuration["OilPriceApi:ApiKey"] ?? "";
    }

    public async Task<bool> FetchAndSaveOilPricesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            if (!string.IsNullOrEmpty(_apiKey))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }

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

            var response = await client.PostAsync(_apiUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to fetch oil prices. Status: {response.StatusCode}");
                return false;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Raw JSON Response: {JsonResponse}", jsonResponse);

            if (string.IsNullOrWhiteSpace(jsonResponse))
            {
                _logger.LogError("Empty or null response received from API.");
                return false;
            }
            
            OilPriceData? oilPriceData = null;

            try
            {
                oilPriceData = JsonSerializer.Deserialize<OilPriceData>(jsonResponse);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization failed. Response: {JsonResponse}", jsonResponse);
                return false;
            }

            if (oilPriceData == null)
            {
                _logger.LogError($"Deserialized OilPriceData object is null. Response JSON: {jsonResponse}");
                return false;
            }

            if (oilPriceData.Status?.ToLower() != "success")
            {
                _logger.LogError("Status in OilPriceData is invalid or missing. Parsed Status: {Status}", oilPriceData.Status);
                return false;
            }

            if (oilPriceData.Data == null)
            {
                _logger.LogError("Data in OilPriceData is null or missing. Parsed Data: {Data}", JsonSerializer.Serialize(oilPriceData.Data));
                return false;
            }

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OilPriceContext>();

            foreach (var dateEntry in oilPriceData.Data)
            {
                if (!DateTime.TryParse(dateEntry.Key, out var priceDate))
                    continue;

                foreach (var companyEntry in dateEntry.Value)
                {
                    var company = companyEntry.Key;

                    foreach (var fuelPrice in companyEntry.Value)
                    {
                        var title = fuelPrice.Title;
                        var price = fuelPrice.Price;

                        if (title == null || price == null)
                            continue;

                        var existingPrice = await context.OilPrices
                            .FirstOrDefaultAsync(p => p.Date == priceDate &&
                                                      p.Company == company &&
                                                      p.FuelType == title);

                        if (existingPrice == null)
                        {
                            context.OilPrices.Add(new OilPrice
                            {
                                Date = priceDate,
                                Company = company,
                                FuelType = title,
                                Price = price.Value
                            });
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
            _logger.LogError(ex, "Unexpected error fetching and saving oil prices.");
            return false;
        }
    }
}

public class OilPriceData
{
    public string? Status { get; set; }
    public Dictionary<string, Dictionary<string, List<FuelPrice>>>? Data { get; set; }
}

public class FuelPrice
{
    public string? Title { get; set; }
    public decimal? Price { get; set; } // 設為 nullable，避免反序列化時值缺失的問題
}

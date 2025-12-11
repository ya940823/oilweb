using Microsoft.EntityFrameworkCore;
using OilWeb.API.Data;
using OilWeb.API.Models;

namespace OilWeb.API.Services
{
    public class OilPriceUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OilPriceUpdateService> _logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromHours(24); // 每24小時檢查一次

        public OilPriceUpdateService(IServiceProvider serviceProvider, ILogger<OilPriceUpdateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("油價更新服務已啟動");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateOilPricesAsync(stoppingToken);
                    _logger.LogInformation("下次更新時間: {NextUpdate}", DateTime.Now.Add(_updateInterval));
                    await Task.Delay(_updateInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "油價更新時發生錯誤");
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // 發生錯誤時30分鐘後重試
                }
            }
        }

        private async Task UpdateOilPricesAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OilPriceDbContext>();

            _logger.LogInformation("開始更新油價資料 - {Time}", DateTime.Now);

            // 檢查今天是否已有資料
            var today = DateTime.Now.Date;
            var todayPrices = await context.OilPrices
                .Where(o => o.Date == today)
                .ToListAsync(cancellationToken);

            if (todayPrices.Any())
            {
                _logger.LogInformation("今日油價已存在，跳過更新");
                return;
            }

            try
            {
                // 實際應用中這裡應該呼叫政府API
                // 範例: var data = await FetchFromGovernmentApiAsync();
                // 這裡使用模擬資料
                var newPrices = await SimulateOilPriceFetchAsync(context, cancellationToken);

                await context.OilPrices.AddRangeAsync(newPrices, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("油價更新成功，共新增 {Count} 筆資料", newPrices.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新油價時發生錯誤");
                throw;
            }
        }

        private async Task<List<OilPrice>> SimulateOilPriceFetchAsync(OilPriceDbContext context, CancellationToken cancellationToken)
        {
            // 模擬從API取得資料
            // 實際應用應替換為真實的政府API呼叫
            var today = DateTime.Now.Date;
            
            // 取得最新價格作為基準
            var latestDate = await context.OilPrices.MaxAsync(o => o.Date, cancellationToken);
            var latestPrices = await context.OilPrices
                .Where(o => o.Date == latestDate)
                .ToListAsync(cancellationToken);

            var random = new Random();
            var newPrices = new List<OilPrice>();

            foreach (var latest in latestPrices)
            {
                // 模擬價格變動 (-0.5 到 +0.5)
                var priceChange = (decimal)(random.NextDouble() - 0.5);
                var newPrice = latest.Price + priceChange;

                newPrices.Add(new OilPrice
                {
                    Date = today,
                    OilType = latest.OilType,
                    Price = Math.Round(newPrice, 1),
                    PriceChange = Math.Round(priceChange, 1),
                    CreatedAt = DateTime.Now
                });
            }

            return newPrices;
        }

        // 以下是實際串接政府API的範例方法 (目前註解)
        /*
        private async Task<List<OilPrice>> FetchFromGovernmentApiAsync()
        {
            using var httpClient = new HttpClient();
            
            // 台灣中油油價API (範例URL，需確認實際API端點)
            var apiUrl = "https://api.data.gov.tw/v1/rest/datastore/...";
            
            var response = await httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync();
            // 解析JSON並轉換為OilPrice物件
            // var data = JsonSerializer.Deserialize<ApiResponse>(jsonContent);
            
            // 轉換並回傳
            return new List<OilPrice>();
        }
        */
    }
}

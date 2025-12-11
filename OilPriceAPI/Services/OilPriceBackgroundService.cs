namespace OilPriceAPI.Services;

public class OilPriceBackgroundService : BackgroundService
{
    private readonly ILogger<OilPriceBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public OilPriceBackgroundService(ILogger<OilPriceBackgroundService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Oil Price Background Service is starting.");

        // Initial fetch of 90 days historical data on startup
        await FetchInitialDataAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            // Check every hour for new data
            var delayMinutes = 60; // Check every hour
            _logger.LogInformation($"Next oil price fetch scheduled in {delayMinutes} minutes");

            await Task.Delay(TimeSpan.FromMinutes(delayMinutes), stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                await FetchDailyDataAsync();
            }
        }
    }

    private async Task FetchInitialDataAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var oilPriceService = scope.ServiceProvider.GetRequiredService<OilPriceService>();
            
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-90);
            
            _logger.LogInformation($"Fetching initial 90 days of oil price data...");
            await oilPriceService.FetchAndSaveOilPricesAsync(startDate, endDate);
            _logger.LogInformation("Initial oil price data fetch completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching initial oil price data");
        }
    }

    private async Task FetchDailyDataAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var oilPriceService = scope.ServiceProvider.GetRequiredService<OilPriceService>();
            
            var today = DateTime.Today;
            
            _logger.LogInformation($"Fetching today's oil price data...");
            await oilPriceService.FetchAndSaveOilPricesAsync(today, today);
            _logger.LogInformation("Daily oil price data fetch completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching daily oil price data");
        }
    }
}

namespace OilPriceAPI.Services;

public class OilPriceBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OilPriceBackgroundService> _logger;

    public OilPriceBackgroundService(IServiceProvider serviceProvider, ILogger<OilPriceBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OilPriceBackgroundService is starting.");

        // Fetch initial data on startup
        await FetchOilPricesAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);
            
            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            var delay = nextRun - now;
            _logger.LogInformation($"Next oil price fetch scheduled at {nextRun}. Waiting for {delay.TotalMinutes:F0} minutes.");

            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                await FetchOilPricesAsync();
            }
        }

        _logger.LogInformation("OilPriceBackgroundService is stopping.");
    }

    private async Task FetchOilPricesAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var oilPriceService = scope.ServiceProvider.GetRequiredService<OilPriceService>();
            
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-90);

            _logger.LogInformation($"Fetching oil prices from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            
            var success = await oilPriceService.FetchAndSaveOilPricesAsync(startDate, endDate);
            
            if (success)
            {
                _logger.LogInformation("Oil prices fetched and saved successfully.");
            }
            else
            {
                _logger.LogWarning("Failed to fetch or save oil prices.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching oil prices in background service.");
        }
    }
}

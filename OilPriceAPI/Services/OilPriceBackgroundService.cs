namespace OilPriceAPI.Services;

public class OilPriceBackgroundService : BackgroundService
{
    private readonly ILogger<OilPriceBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public OilPriceBackgroundService(
        ILogger<OilPriceBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Oil Price Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;
                var scheduledTime = new DateTime(now.Year, now.Month, now.Day, 12, 0, 0);

                if (now > scheduledTime)
                {
                    scheduledTime = scheduledTime.AddDays(1);
                }

                var delay = scheduledTime - now;
                _logger.LogInformation("Next oil price fetch scheduled at {ScheduledTime}", scheduledTime);

                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var oilPriceService = scope.ServiceProvider.GetRequiredService<OilPriceService>();
                    await oilPriceService.FetchAndSaveOilPricesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Oil Price Background Service");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        _logger.LogInformation("Oil Price Background Service is stopping.");
    }
}

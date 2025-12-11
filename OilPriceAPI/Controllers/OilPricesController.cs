using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using OilPriceAPI.Data;
using OilPriceAPI.Models;
using OilPriceAPI.Services;

namespace OilPriceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OilPricesController : ControllerBase
{
    private readonly OilPriceContext _context;
    private readonly OilPriceService _oilPriceService;
    private readonly ILogger<OilPricesController> _logger;

    public OilPricesController(OilPriceContext context, OilPriceService oilPriceService, ILogger<OilPricesController> logger)
    {
        _context = context;
        _oilPriceService = oilPriceService;
        _logger = logger;
    }

    [HttpGet("latest")]
    public async Task<ActionResult<OilPriceDto>> GetLatest()
    {
        var latestDate = await _context.OilPrices.MaxAsync(p => p.Date);
        var latestPrices = await _context.OilPrices
            .Where(p => p.Date == latestDate && p.Company == "中油")
            .ToListAsync();

        if (!latestPrices.Any())
        {
            return NotFound();
        }

        var result = new OilPriceDto
        {
            Date = latestDate,
            Price92 = latestPrices.FirstOrDefault(p => p.FuelType.Contains("92"))?.Price ?? 0,
            Price95 = latestPrices.FirstOrDefault(p => p.FuelType.Contains("95"))?.Price ?? 0,
            Price98 = latestPrices.FirstOrDefault(p => p.FuelType.Contains("98"))?.Price ?? 0,
            PriceDiesel = latestPrices.FirstOrDefault(p => p.FuelType.Contains("柴油") || p.FuelType.Contains("超級柴油"))?.Price ?? 0
        };

        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<OilPriceDto>>> GetHistory([FromQuery] int days = 30)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-days);

        var prices = await _context.OilPrices
            .Where(p => p.Date >= startDate && p.Date <= endDate && p.Company == "中油")
            .OrderBy(p => p.Date)
            .ToListAsync();

        var result = prices
            .GroupBy(p => p.Date)
            .Select(g => new OilPriceDto
            {
                Date = g.Key,
                Price92 = g.FirstOrDefault(p => p.FuelType.Contains("92"))?.Price ?? 0,
                Price95 = g.FirstOrDefault(p => p.FuelType.Contains("95"))?.Price ?? 0,
                Price98 = g.FirstOrDefault(p => p.FuelType.Contains("98"))?.Price ?? 0,
                PriceDiesel = g.FirstOrDefault(p => p.FuelType.Contains("柴油") || p.FuelType.Contains("超級柴油"))?.Price ?? 0
            })
            .ToList();

        return Ok(result);
    }

    [HttpGet("history-with-predictions")]
    public async Task<ActionResult<List<OilPricePredictionDto>>> GetHistoryWithPredictions([FromQuery] int days = 30)
    {
        var result = await _oilPriceService.GetHistoricalAndPredictedPricesAsync(days);
        return Ok(result);
    }

    [HttpGet("range")]
    public async Task<ActionResult<List<OilPriceDto>>> GetRange([FromQuery] string start, [FromQuery] string end)
    {
        if (!DateTime.TryParse(start, out var startDate) || !DateTime.TryParse(end, out var endDate))
        {
            return BadRequest("Invalid date format. Use yyyy-MM-dd");
        }

        var prices = await _context.OilPrices
            .Where(p => p.Date >= startDate && p.Date <= endDate && p.Company == "中油")
            .OrderBy(p => p.Date)
            .ToListAsync();

        var result = prices
            .GroupBy(p => p.Date)
            .Select(g => new OilPriceDto
            {
                Date = g.Key,
                Price92 = g.FirstOrDefault(p => p.FuelType.Contains("92"))?.Price ?? 0,
                Price95 = g.FirstOrDefault(p => p.FuelType.Contains("95"))?.Price ?? 0,
                Price98 = g.FirstOrDefault(p => p.FuelType.Contains("98"))?.Price ?? 0,
                PriceDiesel = g.FirstOrDefault(p => p.FuelType.Contains("柴油") || p.FuelType.Contains("超級柴油"))?.Price ?? 0
            })
            .ToList();

        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] int days = 30)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-days);

        var prices = await _context.OilPrices
            .Where(p => p.Date >= startDate && p.Date <= endDate && p.Company == "中油")
            .OrderBy(p => p.Date)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("日期,92無鉛,95無鉛,98無鉛,超級柴油");

        foreach (var group in prices.GroupBy(p => p.Date).OrderBy(g => g.Key))
        {
            var price92 = group.FirstOrDefault(p => p.FuelType.Contains("92"))?.Price ?? 0;
            var price95 = group.FirstOrDefault(p => p.FuelType.Contains("95"))?.Price ?? 0;
            var price98 = group.FirstOrDefault(p => p.FuelType.Contains("98"))?.Price ?? 0;
            var priceDiesel = group.FirstOrDefault(p => p.FuelType.Contains("柴油") || p.FuelType.Contains("超級柴油"))?.Price ?? 0;

            csv.AppendLine($"{group.Key:yyyy-MM-dd},{price92},{price95},{price98},{priceDiesel}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"oil_prices_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv");
    }

    [HttpGet("statistics")]
    public async Task<ActionResult> GetStatistics([FromQuery] int days = 30)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-days);

        var prices = await _context.OilPrices
            .Where(p => p.Date >= startDate && p.Date <= endDate && p.Company == "中油")
            .ToListAsync();

        if (!prices.Any())
        {
            return NotFound();
        }

        var price92List = prices.Where(p => p.FuelType.Contains("92")).Select(p => p.Price).ToList();
        var price95List = prices.Where(p => p.FuelType.Contains("95")).Select(p => p.Price).ToList();
        var price98List = prices.Where(p => p.FuelType.Contains("98")).Select(p => p.Price).ToList();
        var priceDieselList = prices.Where(p => p.FuelType.Contains("柴油") || p.FuelType.Contains("超級柴油")).Select(p => p.Price).ToList();

        var result = new
        {
            Price92 = new
            {
                Average = price92List.Any() ? Math.Round(price92List.Average(), 2) : 0,
                Max = price92List.Any() ? price92List.Max() : 0,
                Min = price92List.Any() ? price92List.Min() : 0
            },
            Price95 = new
            {
                Average = price95List.Any() ? Math.Round(price95List.Average(), 2) : 0,
                Max = price95List.Any() ? price95List.Max() : 0,
                Min = price95List.Any() ? price95List.Min() : 0
            },
            Price98 = new
            {
                Average = price98List.Any() ? Math.Round(price98List.Average(), 2) : 0,
                Max = price98List.Any() ? price98List.Max() : 0,
                Min = price98List.Any() ? price98List.Min() : 0
            },
            Diesel = new
            {
                Average = priceDieselList.Any() ? Math.Round(priceDieselList.Average(), 2) : 0,
                Max = priceDieselList.Any() ? priceDieselList.Max() : 0,
                Min = priceDieselList.Any() ? priceDieselList.Min() : 0
            }
        };

        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshData([FromQuery] int days = 90)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-days);
        
        var success = await _oilPriceService.FetchAndSaveOilPricesAsync(startDate, endDate);
        
        if (success)
        {
            return Ok(new { message = "Data refreshed successfully" });
        }
        
        return StatusCode(500, new { message = "Failed to refresh data" });
    }
}

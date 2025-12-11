using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OilPriceAPI.Data;
using OilPriceAPI.Models;
using System.Text;

namespace OilPriceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OilPricesController : ControllerBase
{
    private readonly OilPriceContext _context;
    private readonly ILogger<OilPricesController> _logger;

    public OilPricesController(OilPriceContext context, ILogger<OilPricesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/oilprices/latest
    [HttpGet("latest")]
    public async Task<ActionResult<OilPriceDto>> GetLatest()
    {
        var latest = await _context.OilPrices
            .OrderByDescending(p => p.Date)
            .FirstOrDefaultAsync();

        if (latest == null)
        {
            return NotFound();
        }

        var previous = await _context.OilPrices
            .Where(p => p.Date < latest.Date)
            .OrderByDescending(p => p.Date)
            .FirstOrDefaultAsync();

        var dto = new OilPriceDto
        {
            Date = latest.Date,
            Gas92 = latest.Gas92,
            Gas95 = latest.Gas95,
            Gas98 = latest.Gas98,
            Diesel = latest.Diesel,
            Gas92Change = previous != null ? latest.Gas92 - previous.Gas92 : null,
            Gas95Change = previous != null ? latest.Gas95 - previous.Gas95 : null,
            Gas98Change = previous != null ? latest.Gas98 - previous.Gas98 : null,
            DieselChange = previous != null ? latest.Diesel - previous.Diesel : null
        };

        return Ok(dto);
    }

    // GET: api/oilprices/history?days=30
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<OilPrice>>> GetHistory([FromQuery] int days = 30)
    {
        if (days <= 0 || days > 365)
        {
            return BadRequest("Days must be between 1 and 365");
        }

        var startDate = DateTime.Today.AddDays(-days);
        var prices = await _context.OilPrices
            .Where(p => p.Date >= startDate)
            .OrderBy(p => p.Date)
            .ToListAsync();

        return Ok(prices);
    }

    // GET: api/oilprices/range?start=2024-01-01&end=2024-12-31
    [HttpGet("range")]
    public async Task<ActionResult<IEnumerable<OilPrice>>> GetByDateRange(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        if (start > end)
        {
            return BadRequest("Start date must be before end date");
        }

        var prices = await _context.OilPrices
            .Where(p => p.Date >= start && p.Date <= end)
            .OrderBy(p => p.Date)
            .ToListAsync();

        return Ok(prices);
    }

    // GET: api/oilprices/export?days=30
    [HttpGet("export")]
    public async Task<IActionResult> ExportToCsv([FromQuery] int days = 30)
    {
        var startDate = DateTime.Today.AddDays(-days);
        var prices = await _context.OilPrices
            .Where(p => p.Date >= startDate)
            .OrderBy(p => p.Date)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Date,Gas92,Gas95,Gas98,Diesel");

        foreach (var price in prices)
        {
            csv.AppendLine($"{price.Date:yyyy-MM-dd},{price.Gas92},{price.Gas95},{price.Gas98},{price.Diesel}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"oil-prices-{days}days.csv");
    }

    // GET: api/oilprices/statistics?days=30
    [HttpGet("statistics")]
    public async Task<ActionResult<object>> GetStatistics([FromQuery] int days = 30)
    {
        var startDate = DateTime.Today.AddDays(-days);
        var prices = await _context.OilPrices
            .Where(p => p.Date >= startDate)
            .OrderBy(p => p.Date)
            .ToListAsync();

        if (!prices.Any())
        {
            return NotFound();
        }

        var stats = new
        {
            Period = $"{days} days",
            Gas92 = new
            {
                Average = prices.Average(p => p.Gas92),
                Min = prices.Min(p => p.Gas92),
                Max = prices.Max(p => p.Gas92)
            },
            Gas95 = new
            {
                Average = prices.Average(p => p.Gas95),
                Min = prices.Min(p => p.Gas95),
                Max = prices.Max(p => p.Gas95)
            },
            Gas98 = new
            {
                Average = prices.Average(p => p.Gas98),
                Min = prices.Min(p => p.Gas98),
                Max = prices.Max(p => p.Gas98)
            },
            Diesel = new
            {
                Average = prices.Average(p => p.Diesel),
                Min = prices.Min(p => p.Diesel),
                Max = prices.Max(p => p.Diesel)
            }
        };

        return Ok(stats);
    }
}

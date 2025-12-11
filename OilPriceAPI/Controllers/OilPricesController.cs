using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OilPriceAPI.Data;
using OilPriceAPI.Models;

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

    [HttpGet("latest")]
    public async Task<ActionResult<IEnumerable<OilPriceDto>>> GetLatest()
    {
        var latestDate = await _context.OilPrices.MaxAsync(p => (DateTime?)p.Date);
        
        if (latestDate == null)
        {
            return NotFound("No oil price data available.");
        }

        var latestPrices = await _context.OilPrices
            .Where(p => p.Date == latestDate)
            .Select(p => new OilPriceDto
            {
                Date = p.Date,
                Company = p.Company,
                FuelType = p.FuelType,
                Price = p.Price
            })
            .ToListAsync();

        return Ok(latestPrices);
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<OilPriceDto>>> GetHistory([FromQuery] int days = 30)
    {
        var startDate = DateTime.Today.AddDays(-days);

        var prices = await _context.OilPrices
            .Where(p => p.Date >= startDate)
            .OrderBy(p => p.Date)
            .Select(p => new OilPriceDto
            {
                Date = p.Date,
                Company = p.Company,
                FuelType = p.FuelType,
                Price = p.Price
            })
            .ToListAsync();

        return Ok(prices);
    }

    [HttpGet("range")]
    public async Task<ActionResult<IEnumerable<OilPriceDto>>> GetRange([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var prices = await _context.OilPrices
            .Where(p => p.Date >= start && p.Date <= end)
            .OrderBy(p => p.Date)
            .Select(p => new OilPriceDto
            {
                Date = p.Date,
                Company = p.Company,
                FuelType = p.FuelType,
                Price = p.Price
            })
            .ToListAsync();

        return Ok(prices);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportCsv([FromQuery] int days = 30)
    {
        var startDate = DateTime.Today.AddDays(-days);

        var prices = await _context.OilPrices
            .Where(p => p.Date >= startDate)
            .OrderBy(p => p.Date)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Date,Company,FuelType,Price");

        foreach (var price in prices)
        {
            csv.AppendLine($"{price.Date:yyyy-MM-dd},{price.Company},{price.FuelType},{price.Price}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"oil_prices_{days}days.csv");
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<IEnumerable<OilPriceStatistics>>> GetStatistics([FromQuery] int days = 30)
    {
        var startDate = DateTime.Today.AddDays(-days);

        var statistics = await _context.OilPrices
            .Where(p => p.Date >= startDate)
            .GroupBy(p => p.FuelType)
            .Select(g => new OilPriceStatistics
            {
                FuelType = g.Key,
                Average = g.Average(p => p.Price),
                Max = g.Max(p => p.Price),
                Min = g.Min(p => p.Price)
            })
            .ToListAsync();

        return Ok(statistics);
    }
}

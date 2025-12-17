using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OilPriceAPI.Data;
using OilPriceAPI.Models;
using OilPriceAPI.Services;

namespace OilPriceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OilPricesController : ControllerBase
    {
        private readonly OilPriceContext _context;
        private readonly XmlParserService _xmlParserService;
        private readonly ILogger<OilPricesController> _logger;
        
        public OilPricesController(
            OilPriceContext context,
            XmlParserService xmlParserService,
            ILogger<OilPricesController> logger)
        {
            _context = context;
            _xmlParserService = xmlParserService;
            _logger = logger;
        }
        
        // GET: api/oilprices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OilPrice>>> GetAllOilPrices()
        {
            return await _context.OilPrices
                .OrderByDescending(o => o.Date)
                .ToListAsync();
        }
        
        // GET: api/oilprices/latest
        [HttpGet("latest")]
        public async Task<ActionResult<OilPrice>> GetLatestOilPrice()
        {
            var latest = await _context.OilPrices
                .OrderByDescending(o => o.Date)
                .FirstOrDefaultAsync();
                
            if (latest == null)
            {
                return NotFound("No oil price data available");
            }
            
            return latest;
        }
        
        // GET: api/oilprices/history?days=30
        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<OilPrice>>> GetHistory([FromQuery] int days = 30)
        {
            var startDate = DateTime.Now.AddDays(-days);
            
            return await _context.OilPrices
                .Where(o => o.Date >= startDate)
                .OrderByDescending(o => o.Date)
                .ToListAsync();
        }
        
        // GET: api/oilprices/range?start=2024-01-01&end=2024-12-31
        [HttpGet("range")]
        public async Task<ActionResult<IEnumerable<OilPrice>>> GetByDateRange(
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            return await _context.OilPrices
                .Where(o => o.Date >= start && o.Date <= end)
                .OrderByDescending(o => o.Date)
                .ToListAsync();
        }
        
        // GET: api/oilprices/statistics?days=30
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetStatistics([FromQuery] int days = 30)
        {
            var startDate = DateTime.Now.AddDays(-days);
            
            var prices = await _context.OilPrices
                .Where(o => o.Date >= startDate)
                .ToListAsync();
                
            if (!prices.Any())
            {
                return NotFound("No data available for statistics");
            }
            
            return new
            {
                Period = $"Last {days} days",
                Oil92 = new
                {
                    Average = prices.Average(p => p.Oil92),
                    Max = prices.Max(p => p.Oil92),
                    Min = prices.Min(p => p.Oil92)
                },
                Oil95 = new
                {
                    Average = prices.Average(p => p.Oil95),
                    Max = prices.Max(p => p.Oil95),
                    Min = prices.Min(p => p.Oil95)
                },
                Oil98 = new
                {
                    Average = prices.Average(p => p.Oil98),
                    Max = prices.Max(p => p.Oil98),
                    Min = prices.Min(p => p.Oil98)
                },
                Diesel = new
                {
                    Average = prices.Average(p => p.Diesel),
                    Max = prices.Max(p => p.Diesel),
                    Min = prices.Min(p => p.Diesel)
                }
            };
        }
        
        // POST: api/oilprices/upload-xml
        [HttpPost("upload-xml")]
        public async Task<ActionResult<object>> UploadXml(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }
            
            if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("File must be XML format");
            }
            
            try
            {
                var tempPath = Path.GetTempFileName();
                
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                var oilPrices = await _xmlParserService.ParseXmlFileAsync(tempPath);
                var savedCount = await _xmlParserService.SaveToDataBaseAsync(oilPrices);
                
                System.IO.File.Delete(tempPath);
                
                return Ok(new
                {
                    Message = "XML file processed successfully",
                    TotalRecords = oilPrices.Count,
                    SavedRecords = savedCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing XML file");
                return StatusCode(500, new { Error = "Error processing XML file", Details = ex.Message });
            }
        }
    }
}

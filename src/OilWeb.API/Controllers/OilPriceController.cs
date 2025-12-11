using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OilWeb.API.Data;
using OilWeb.API.Models;
using System.Globalization;
using System.Text;

namespace OilWeb.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OilPriceController : ControllerBase
    {
        private readonly OilPriceDbContext _context;
        private readonly ILogger<OilPriceController> _logger;

        public OilPriceController(OilPriceDbContext context, ILogger<OilPriceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 取得最新油價 (所有油種)
        /// </summary>
        [HttpGet("latest")]
        public async Task<ActionResult<IEnumerable<OilPrice>>> GetLatest()
        {
            var latestDate = await _context.OilPrices.MaxAsync(o => o.Date);
            var latestPrices = await _context.OilPrices
                .Where(o => o.Date == latestDate)
                .OrderBy(o => o.OilType)
                .ToListAsync();

            return Ok(latestPrices);
        }

        /// <summary>
        /// 取得歷史油價 (依天數)
        /// </summary>
        /// <param name="days">查詢天數 (預設30天)</param>
        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<OilPrice>>> GetHistory([FromQuery] int days = 30)
        {
            var fromDate = DateTime.Now.Date.AddDays(-days);
            var prices = await _context.OilPrices
                .Where(o => o.Date >= fromDate)
                .OrderBy(o => o.Date)
                .ThenBy(o => o.OilType)
                .ToListAsync();

            return Ok(prices);
        }

        /// <summary>
        /// 取得指定日期區間的油價
        /// </summary>
        /// <param name="from">起始日期</param>
        /// <param name="to">結束日期</param>
        [HttpGet("range")]
        public async Task<ActionResult<IEnumerable<OilPrice>>> GetByDateRange(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            if (!from.HasValue || !to.HasValue)
            {
                return BadRequest("請提供起始日期(from)和結束日期(to)");
            }

            var prices = await _context.OilPrices
                .Where(o => o.Date >= from.Value && o.Date <= to.Value)
                .OrderBy(o => o.Date)
                .ThenBy(o => o.OilType)
                .ToListAsync();

            return Ok(prices);
        }

        /// <summary>
        /// 取得油價趨勢分析
        /// </summary>
        [HttpGet("trend")]
        public async Task<ActionResult<object>> GetTrend()
        {
            var prices = await _context.OilPrices
                .OrderBy(o => o.Date)
                .ThenBy(o => o.OilType)
                .ToListAsync();

            var groupedByType = prices.GroupBy(o => o.OilType);
            
            var trends = groupedByType.Select(g => new
            {
                OilType = g.Key,
                TotalRecords = g.Count(),
                AveragePrice = g.Average(o => o.Price),
                MaxPrice = g.Max(o => o.Price),
                MinPrice = g.Min(o => o.Price),
                CurrentPrice = g.OrderByDescending(o => o.Date).FirstOrDefault()?.Price,
                TotalChange = g.OrderByDescending(o => o.Date).FirstOrDefault()?.Price - g.OrderBy(o => o.Date).FirstOrDefault()?.Price
            });

            return Ok(trends);
        }

        /// <summary>
        /// 油價預測 (簡單線性回歸)
        /// </summary>
        /// <param name="oilType">油種</param>
        /// <param name="days">預測天數</param>
        [HttpGet("predict")]
        public async Task<ActionResult<object>> PredictPrice([FromQuery] string oilType = "92無鉛汽油", [FromQuery] int days = 7)
        {
            var prices = await _context.OilPrices
                .Where(o => o.OilType == oilType)
                .OrderBy(o => o.Date)
                .Select(o => new { o.Date, o.Price })
                .ToListAsync();

            if (prices.Count < 2)
            {
                return BadRequest("資料不足，無法進行預測");
            }

            // 簡單線性回歸計算
            var n = prices.Count;
            var sumX = 0.0;
            var sumY = 0.0;
            var sumXY = 0.0;
            var sumX2 = 0.0;

            for (int i = 0; i < n; i++)
            {
                var x = i;
                var y = (double)prices[i].Price;
                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            var intercept = (sumY - slope * sumX) / n;

            // 預測未來價格
            var predictions = new List<object>();
            var lastDate = prices.Last().Date;
            
            for (int i = 1; i <= days; i++)
            {
                var predictedPrice = intercept + slope * (n + i - 1);
                predictions.Add(new
                {
                    Date = lastDate.AddDays(i * 7), // 假設每週更新
                    PredictedPrice = Math.Round((decimal)predictedPrice, 2)
                });
            }

            return Ok(new
            {
                OilType = oilType,
                CurrentPrice = prices.Last().Price,
                Trend = slope > 0 ? "上漲趨勢" : slope < 0 ? "下跌趨勢" : "持平",
                Predictions = predictions
            });
        }

        /// <summary>
        /// 匯出CSV
        /// </summary>
        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportCsv([FromQuery] int days = 30)
        {
            var fromDate = DateTime.Now.Date.AddDays(-days);
            var prices = await _context.OilPrices
                .Where(o => o.Date >= fromDate)
                .OrderBy(o => o.Date)
                .ThenBy(o => o.OilType)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("日期,油種,價格,漲跌幅");

            foreach (var price in prices)
            {
                csv.AppendLine($"{price.Date:yyyy-MM-dd},{price.OilType},{price.Price},{price.PriceChange ?? 0}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"oil_prices_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}

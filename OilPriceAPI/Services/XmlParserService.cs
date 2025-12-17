using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using OilPriceAPI.Data;
using OilPriceAPI.Models;

namespace OilPriceAPI.Services
{
    public class XmlParserService
    {
        private readonly OilPriceContext _context;
        private readonly ILogger<XmlParserService> _logger;
        
        public XmlParserService(OilPriceContext context, ILogger<XmlParserService> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<List<OilPrice>> ParseXmlFileAsync(string filePath)
        {
            var oilPrices = new List<OilPrice>();
            
            try
            {
                var doc = XDocument.Load(filePath);
                
                foreach (var item in doc.Descendants("item"))
                {
                    var oilPrice = new OilPrice
                    {
                        Date = DateTime.Parse(item.Element("Date")?.Value ?? DateTime.Now.ToString()),
                        Oil92 = decimal.Parse(item.Element("Oil92")?.Value ?? "0"),
                        Oil95 = decimal.Parse(item.Element("Oil95")?.Value ?? "0"),
                        Oil98 = decimal.Parse(item.Element("Oil98")?.Value ?? "0"),
                        Diesel = decimal.Parse(item.Element("Diesel")?.Value ?? "0"),
                        Source = "XML"
                    };
                    
                    oilPrices.Add(oilPrice);
                }
                
                _logger.LogInformation($"Parsed {oilPrices.Count} oil price records from XML");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing XML file");
                throw;
            }
            
            return oilPrices;
        }
        
        public async Task<int> SaveToDataBaseAsync(List<OilPrice> oilPrices)
        {
            int savedCount = 0;
            
            try
            {
                foreach (var oilPrice in oilPrices)
                {
                    var exists = await _context.OilPrices
                        .AnyAsync(o => o.Date.Date == oilPrice.Date.Date);
                    
                    if (!exists)
                    {
                        _context.OilPrices.Add(oilPrice);
                        savedCount++;
                    }
                }
                
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Saved {savedCount} new oil price records to database");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving oil prices to database");
                throw;
            }
            
            return savedCount;
        }
    }
}

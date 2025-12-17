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
                
                // Check if it's CPC DataSet format
                var ns = doc.Root?.GetDefaultNamespace();
                if (ns != null && ns.NamespaceName == "http://tmtd.cpc.com.tw/")
                {
                    // Parse CPC DataSet format
                    oilPrices = ParseCpcDataSetFormat(doc);
                }
                else
                {
                    // Parse simple format
                    oilPrices = ParseSimpleFormat(doc);
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
        
        private List<OilPrice> ParseSimpleFormat(XDocument doc)
        {
            var oilPrices = new List<OilPrice>();
            
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
            
            return oilPrices;
        }
        
        private List<OilPrice> ParseCpcDataSetFormat(XDocument doc)
        {
            var oilPrices = new List<OilPrice>();
            
            // CPC uses ADO.NET DataSet format with namespace
            // The Table1 elements are in no namespace (they inherit from parent but aren't prefixed)
            
            // Find all Table1 elements regardless of namespace
            var tables = doc.Descendants().Where(e => e.Name.LocalName == "Table1");
            
            _logger.LogInformation($"Found {tables.Count()} Table1 elements in CPC XML");
            
            foreach (var table in tables)
            {
                try
                {
                    // CPC uses Chinese field names - try multiple common formats
                    // Elements are in no namespace, so just use LocalName
                    var dateElement = table.Elements().FirstOrDefault(e => 
                        e.Name.LocalName == "參考日期" || 
                        e.Name.LocalName == "日期" || 
                        e.Name.LocalName == "Date");
                    
                    var oil92Element = table.Elements().FirstOrDefault(e => 
                        e.Name.LocalName == "無鉛92" || 
                        e.Name.LocalName == "92無鉛汽油" || 
                        e.Name.LocalName == "Oil92");
                    
                    var oil95Element = table.Elements().FirstOrDefault(e => 
                        e.Name.LocalName == "無鉛95" || 
                        e.Name.LocalName == "95無鉛汽油" || 
                        e.Name.LocalName == "Oil95");
                    
                    var oil98Element = table.Elements().FirstOrDefault(e => 
                        e.Name.LocalName == "無鉛98" || 
                        e.Name.LocalName == "98無鉛汽油" || 
                        e.Name.LocalName == "Oil98");
                    
                    var dieselElement = table.Elements().FirstOrDefault(e => 
                        e.Name.LocalName == "超級柴油" || 
                        e.Name.LocalName == "柴油" || 
                        e.Name.LocalName == "Diesel");
                    
                    var dateStr = dateElement?.Value;
                    var oil92Str = oil92Element?.Value;
                    var oil95Str = oil95Element?.Value;
                    var oil98Str = oil98Element?.Value;
                    var dieselStr = dieselElement?.Value;
                    
                    if (string.IsNullOrEmpty(dateStr))
                    {
                        _logger.LogWarning("Skipping Table1 record with no date");
                        continue;
                    }
                    
                    var oilPrice = new OilPrice
                    {
                        Date = DateTime.Parse(dateStr),
                        Oil92 = decimal.TryParse(oil92Str, out var oil92) ? oil92 : 0,
                        Oil95 = decimal.TryParse(oil95Str, out var oil95) ? oil95 : 0,
                        Oil98 = decimal.TryParse(oil98Str, out var oil98) ? oil98 : 0,
                        Diesel = decimal.TryParse(dieselStr, out var diesel) ? diesel : 0,
                        Source = "CPC-XML"
                    };
                    
                    oilPrices.Add(oilPrice);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing individual Table1 record, skipping");
                    continue;
                }
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

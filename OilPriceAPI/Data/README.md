# Local Oil Price Data

## Overview

This system now reads oil price data from local XML files instead of calling the CPC API. This approach works even when the API is blocked or unavailable.

**NEW: Sample Data Included**

The XML files now come pre-populated with sample oil price data for demonstration purposes. This means:
- ✅ First-time run will automatically load sample data into database
- ✅ Chart will display immediately with realistic weekly price data
- ✅ All features (predictions, comparisons, statistics) work out of the box
- ✅ You can replace sample data with real CPC data anytime

## XML Files

The following XML files contain oil price data:

- **oil-price-92.xml** - 92無鉛汽油 (92 unleaded gasoline) - **Contains 11 weeks of sample data**
- **oil-price-95.xml** - 95無鉛汽油 (95 unleaded gasoline) - **Contains 11 weeks of sample data**
- **oil-price-98.xml** - 98無鉛汽油 (98 unleaded gasoline) - **Contains 11 weeks of sample data**
- **oil-price-diesel.xml** - 超級柴油 (Super diesel) - **Contains 11 weeks of sample data**

**Sample Data Details:**
- Covers October 7, 2024 to December 16, 2024 (11 weekly records)
- Weekly updates matching Taiwan's oil price update schedule
- Realistic price ranges and variations
- Sufficient for demonstrating predictions and comparisons

## First Run Behavior

When you first run the application:

1. **Application starts** and detects empty database
2. **Automatically loads** data from XML files (11 records per fuel type = 44 total)
3. **Database is populated** with sample data
4. **Chart displays** immediately with full functionality
5. **All features work**: predictions, week comparisons, 10L price difference, statistics

No need to manually seed data or click any buttons!

## XML Format

The system expects the CPC DataSet format with Chinese field names:

```xml
<?xml version="1.0" encoding="utf-8"?>
<DataSet xmlns="http://tmtd.cpc.com.tw/">
  <diffgr:diffgram xmlns:msdata="urn:schemas-microsoft-com:xml-msdata" xmlns:diffgr="urn:schemas-microsoft-com:xml-diffgram-v1">
    <NewDataSet xmlns="">
      <tbTable diffgr:id="tbTable1" msdata:roworder="0">
        <牌價生效時間>2024-10-07T00:00:00+08:00</牌價生效時間>
        <產品名>無鉛汽油92</產品名>
        <參考牌價>30.0</參考牌價>
        <計價單位>元/公升</計價單位>
      </tbTable>
      <tbTable diffgr:id="tbTable2" msdata:roworder="1">
        <牌價生效時間>2024-10-14T00:00:00+08:00</牌價生效時間>
        <產品名>無鉛汽油92</產品名>
        <參考牌價>30.1</參考牌價>
        <計價單位>元/公升</計價單位>
      </tbTable>
      <!-- More records... -->
    </NewDataSet>
  </diffgr:diffgram>
</DataSet>
```

**Important Note about XML Attributes:**
- The CPC format uses `msdata:roworder` (lowercase 'o') not `msdata:rowOrder` (uppercase 'O')
- Visual Studio may show a warning about undeclared attributes - this is normal and can be ignored
- The `msdata:roworder` attribute is defined by the Microsoft XML Schema namespace
- Simply paste your CPC XML data directly - it will work correctly

## How to Replace Sample Data with Real Data

### Method 1: Paste XML Content (RECOMMENDED)

1. Get XML data from CPC API or export
2. Open the appropriate file (e.g., `oil-price-92.xml`)
3. Replace the **entire file contents** with your CPC XML data
4. Ensure the format matches the example above
5. Save the file
6. Click "🔄 更新資料" button to reload data

### Method 2: Manual Entry

If you only have a few records, you can manually add `<tbTable>` entries:

```xml
<tbTable diffgr:id="tbTable12" msdata:roworder="11">
  <牌價生效時間>2024-12-23T00:00:00+08:00</牌價生效時間>
  <產品名>無鉛汽油92</產品名>
  <參考牌價>30.5</參考牌價>
  <計價單位>元/公升</計價單位>
</tbTable>
```

**Note:** Use `msdata:roworder` (lowercase 'o') as shown above to match the CPC format.

## Fields Required

For each record, the system reads these fields:

- **牌價生效時間** (Effective Date) - Date and time when price takes effect
- **產品名** (Product Name) - Name of fuel type (optional, for validation)
- **參考牌價** (Reference Price) - Price in NT$ per liter
- **計價單位** (Unit) - Unit of measurement (optional)

## Loading Data

### Automatic Loading (First Run)

On first run, the system automatically loads XML data into the database:
- Checks if database is empty
- Loads all 4 XML files
- Inserts records into database
- Ready to use immediately

### Manual Reload

You can manually trigger a data refresh after updating XML files:

1. **Via Web UI**: Click the "🔄 更新資料" (Update Data) button on the homepage

2. **Via API**:
```bash
POST http://localhost:5000/api/oilprices/refresh?days=90
```

3. **Via curl**:
```bash
curl -X POST "http://localhost:5000/api/oilprices/refresh?days=90"
```

## Checking Results

After application starts:

1. Open http://localhost:5000 in your browser
2. The chart should display oil price data with predictions
3. Check "每10公升與上週差距" section for week-over-week comparisons
4. Verify statistics section shows data analysis
5. Open browser Console (F12) to see debug logs

## Troubleshooting

### No Data Displaying (Even with Sample Data)

1. Check application logs for "Loading initial data from XML files..."
2. Verify XML files exist in the `Data/` directory
3. Check for XML parsing errors in logs
4. Ensure database file `oilprice.db` was created

### Want to Reset to Sample Data

1. Delete database file: `oilprice.db`
2. Restart application
3. Sample data will be reloaded from XML files

### Parsing Errors

Common issues:
- Missing closing tags
- Incorrect encoding (must be UTF-8)
- Missing namespace declarations
- Invalid date format

## Benefits of Local XML with Sample Data

✅ **Works immediately** - No setup or data entry required
✅ **No API key required** - No external dependencies
✅ **Works offline** - No network connection needed
✅ **Realistic demo** - Sample data shows all features
✅ **Easy to replace** - Simply paste real CPC data when available
✅ **Full control** - Complete data sovereignty
✅ **Fast loading** - No API delays
✅ **No rate limits** - No restrictions

## Production Deployment

For production deployment:

1. **Before publishing**: Replace sample data with real CPC historical data
2. **Publish**: Run `dotnet publish -c Release`
3. **Deploy**: Upload to web server
4. **First run**: Database auto-initializes from XML files
5. **Updates**: Update XML files and click refresh button

The XML files are included in deployment output automatically.

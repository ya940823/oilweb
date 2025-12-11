# Local Oil Price Data

## Overview

This system now reads oil price data from local XML files instead of calling the CPC API. This approach works even when the API is blocked or unavailable.

## XML Files

Place your CPC oil price XML data in these files:

- **oil-price-92.xml** - 92無鉛汽油 (92 unleaded gasoline)
- **oil-price-95.xml** - 95無鉛汽油 (95 unleaded gasoline)
- **oil-price-98.xml** - 98無鉛汽油 (98 unleaded gasoline)
- **oil-price-diesel.xml** - 超級柴油 (Super diesel)

## XML Format

The system expects the CPC DataSet format with Chinese field names:

```xml
<?xml version="1.0" encoding="utf-8"?>
<DataSet xmlns="http://tmtd.cpc.com.tw/">
  <diffgr:diffgram xmlns:msdata="urn:schemas-microsoft-com:xml-msdata" xmlns:diffgr="urn:schemas-microsoft-com:xml-diffgram-v1">
    <NewDataSet xmlns="">
      <tbTable diffgr:id="tbTable1" msdata:rowOrder="0">
        <牌價生效時間>1999-01-06T00:00:00+08:00</牌價生效時間>
        <產品名>無鉛汽油92</產品名>
        <參考牌價>15</參考牌價>
        <計價單位>元/公升</計價單位>
      </tbTable>
      <tbTable diffgr:id="tbTable2" msdata:rowOrder="1">
        <牌價生效時間>1999-04-07T00:00:00+08:00</牌價生效時間>
        <產品名>無鉛汽油92</產品名>
        <參考牌價>15.3</參考牌價>
        <計價單位>元/公升</計價單位>
      </tbTable>
      <!-- Add more records here -->
    </NewDataSet>
  </diffgr:diffgram>
</DataSet>
```

## How to Add Data

### Method 1: Paste XML Content (RECOMMENDED)

1. Get XML data from CPC API or export
2. Open the appropriate file (e.g., `oil-price-92.xml`)
3. Replace the contents with your XML data
4. Ensure the format matches the example above
5. Save the file

### Method 2: Manual Entry

If you only have a few records, you can manually add `<tbTable>` entries:

```xml
<tbTable diffgr:id="tbTable3" msdata:rowOrder="2">
  <牌價生效時間>2024-12-01T00:00:00+08:00</牌價生效時間>
  <產品名>無鉛汽油92</產品名>
  <參考牌價>30.3</參考牌價>
  <計價單位>元/公升</計價單位>
</tbTable>
```

## Fields Required

For each record, the system reads these fields:

- **牌價生效時間** (Effective Date) - Date and time when price takes effect
- **產品名** (Product Name) - Name of fuel type (optional, for validation)
- **參考牌價** (Reference Price) - Price in NT$ per liter
- **計價單位** (Unit) - Unit of measurement (optional)

## Loading Data

### Automatic Loading

The background service will automatically load data from XML files every hour.

### Manual Loading

You can manually trigger a data refresh:

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

After loading data:

1. Open http://localhost:5000 in your browser
2. The chart should display your oil price data
3. Check console logs for any errors during XML parsing

## Troubleshooting

### No Data Displaying

1. Check that XML files exist in the `Data/` directory
2. Verify XML format matches the example
3. Check application logs for parsing errors
4. Ensure at least one `<tbTable>` entry exists in each file

### Parsing Errors

Common issues:
- Missing closing tags
- Incorrect encoding (must be UTF-8)
- Missing namespace declarations
- Invalid date format

### Empty Files

The template XML files contain comment examples. You can either:
- Replace entire file content with your data
- Add `<tbTable>` entries inside the `<NewDataSet>` section

## Example: Complete File

Here's a complete example with multiple records:

```xml
<?xml version="1.0" encoding="utf-8"?>
<DataSet xmlns="http://tmtd.cpc.com.tw/">
  <diffgr:diffgram xmlns:msdata="urn:schemas-microsoft-com:xml-msdata" xmlns:diffgr="urn:schemas-microsoft-com:xml-diffgram-v1">
    <NewDataSet xmlns="">
      <tbTable diffgr:id="tbTable1" msdata:rowOrder="0">
        <牌價生效時間>2024-10-01T00:00:00+08:00</牌價生效時間>
        <產品名>無鉛汽油92</產品名>
        <參考牌價>29.5</參考牌價>
        <計價單位>元/公升</計價單位>
      </tbTable>
      <tbTable diffgr:id="tbTable2" msdata:rowOrder="1">
        <牌價生效時間>2024-11-01T00:00:00+08:00</牌價生效時間>
        <產品名>無鉛汽油92</產品名>
        <參考牌價>30.0</參考牌價>
        <計價單位>元/公升</計價單位>
      </tbTable>
      <tbTable diffgr:id="tbTable3" msdata:rowOrder="2">
        <牌價生效時間>2024-12-01T00:00:00+08:00</牌價生效時間>
        <產品名>無鉛汽油92</產品名>
        <參考牌價>30.3</參考牌價>
        <計價單位>元/公升</計價單位>
      </tbTable>
    </NewDataSet>
  </diffgr:diffgram>
</DataSet>
```

## Benefits of Local XML

✅ No API key required
✅ Works offline
✅ No network restrictions
✅ Full control over data
✅ Historical data preservation
✅ Fast loading
✅ No rate limits

## Notes

- The system will load ALL records from XML files, regardless of date
- Duplicate records (same date, company, fuel type) will be updated, not inserted
- XML parsing errors will be logged but won't crash the application
- You can update XML files while the application is running and trigger a refresh

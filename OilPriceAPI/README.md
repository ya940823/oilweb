# 台灣油價走勢分析 API

完整的油價追蹤與預測系統，支援政府開放資料平台整合與 30 天價格預測。

## 專案特色

### 🎯 核心功能
- **本地 XML 資料源**：從本地 XML 檔案讀取中油歷史油價資料（不再依賴外部 API）
- **30 天預測**：使用線性迴歸預測未來油價走勢
- **虛線視覺化**：圖表中歷史資料用實線、預測資料用虛線顯示
- **離線運作**：完全離線可用，不受網路限制
- **RESTful API**：提供完整的 API 端點供查詢
- **響應式介面**：支援各種裝置瀏覽

### 📊 預測演算法
- 基於最近 30 天的歷史資料
- 使用簡單線性迴歸計算趨勢
- 分別計算 92、95、98 無鉛汽油與柴油的趨勢
- 預測未來 30 天的價格變化

### 🔄 資料載入
- 背景服務每小時自動從 XML 檔案載入資料
- 支援手動刷新：`POST /api/oilprices/refresh?days=90`
- 首次啟動時自動載入 XML 檔案中的所有歷史資料
- 資料來源：本地 XML 檔案（Data 目錄下）

## 安裝與執行

### 前置需求
- .NET 10.0 SDK
- SQL Server（LocalDB）或 SQLite

### 快速開始

1. **Clone 專案**
```bash
git clone https://github.com/ya940823/oilweb.git
cd oilweb/OilPriceAPI
```

2. **準備 XML 資料檔案**

系統從本地 XML 檔案讀取油價資料。請將中油 XML 資料貼到以下檔案：

```
OilPriceAPI/Data/oil-price-92.xml   ← 92 無鉛汽油
OilPriceAPI/Data/oil-price-95.xml   ← 95 無鉛汽油
OilPriceAPI/Data/oil-price-98.xml   ← 98 無鉛汽油
OilPriceAPI/Data/oil-price-diesel.xml ← 超級柴油
}
```

3. **還原套件**
```bash
dotnet restore
```

4. **執行專案**
```bash
dotnet run
```

5. **準備 XML 資料**（**重要！**）

⚠️ **首次執行時資料庫是空的，圖表將無法顯示。請準備 XML 資料：**

**方法 1 - 貼上中油 XML 資料（推薦）：**

1. 將中油 API 回傳的 XML 資料貼到對應檔案：
   - `Data/oil-price-92.xml` ← 92 無鉛汽油
   - `Data/oil-price-95.xml` ← 95 無鉛汽油
   - `Data/oil-price-98.xml` ← 98 無鉛汽油
   - `Data/oil-price-diesel.xml` ← 超級柴油

2. XML 格式範例：
```xml
<?xml version="1.0" encoding="utf-8"?>
<DataSet xmlns="http://tmtd.cpc.com.tw/">
  <diffgr:diffgram xmlns:msdata="urn:schemas-microsoft-com:xml-msdata" xmlns:diffgr="urn:schemas-microsoft-com:xml-diffgram-v1">
    <NewDataSet xmlns="">
      <tbTable diffgr:id="tbTable1" msdata:rowOrder="0">
        <牌價生效時間>2024-12-01T00:00:00+08:00</牌價生效時間>
        <產品名>無鉛汽油92</產品名>
        <參考牌價>30.3</參考牌價>
        <計價單位>元/公升</計價單位>
      </tbTable>
      <!-- 更多記錄... -->
    </NewDataSet>
  </diffgr:diffgram>
</DataSet>
```

3. 點擊「🔄 更新資料」按鈕或等待自動載入

詳細說明請參考：`Data/README.md`

**方法 2 - 使用範例資料（快速測試）：**

如果您沒有中油 XML 資料，可以使用網頁按鈕新增範例資料：

開啟瀏覽器訪問 http://localhost:5000，如果資料庫為空，會看到黃色警告框，點擊「📊 新增範例資料」按鈕即可新增 36 筆測試資料。

6. **瀏覽網站**
開啟瀏覽器前往 `http://localhost:5000`

> **注意**：系統現在從本地 XML 檔案讀取資料，不再依賴外部 API。只要 XML 檔案中有資料，圖表就會正常顯示。

## 資料庫設定

專案支援 SQL Server 和 SQLite：

### 使用 SQLite（預設）
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=oilprice.db"
  }
}
```

### 使用 SQL Server
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OilPriceDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

## API 端點

### 取得最新油價
```
GET /api/oilprices/latest
```

回應範例：
```json
{
  "date": "2025-12-09T00:00:00",
  "price92": 30.3,
  "price95": 31.8,
  "price98": 33.8,
  "priceDiesel": 28.3
}
```

### 取得歷史資料（含預測）⭐
```
GET /api/oilprices/history-with-predictions?days=30
```

回應範例：
```json
[
  {
    "date": "2025-12-09T00:00:00",
    "price92": 30.3,
    "price95": 31.8,
    "price98": 33.8,
    "priceDiesel": 28.3,
    "isPrediction": false
  },
  {
    "date": "2025-12-10T00:00:00",
    "price92": 30.5,
    "price95": 32.0,
    "price98": 34.0,
    "priceDiesel": 28.5,
    "isPrediction": true
  }
]
```

### 取得歷史資料
```
GET /api/oilprices/history?days=30
```

### 取得統計資訊
```
GET /api/oilprices/statistics?days=30
```

### 匯出 CSV
```
GET /api/oilprices/export?days=30
```

### 手動更新資料
```
POST /api/oilprices/refresh?days=90
```

## 前端使用

### 主要頁面
- `index.html` - 完整的 UI 介面（需要 Chart.js CDN）
- `demo-chart.html` - 獨立的圖表示範頁面（無需外部相依）

### Chart.js 整合

圖表使用 Chart.js 的 segment 功能來實現虛線預測：

```javascript
{
  label: '92無鉛',
  data: allData,
  segment: {
    borderDash: ctx => {
      // 預測部分使用虛線
      return ctx.p0DataIndex >= historicalLength - 1 ? [5, 5] : undefined;
    }
  }
}
```

## 專案結構

```
OilPriceAPI/
├── Controllers/
│   └── OilPricesController.cs      # API 控制器
├── Data/
│   └── OilPriceContext.cs          # 資料庫上下文
├── Models/
│   ├── OilPrice.cs                 # 油價資料模型
│   └── OilPriceDto.cs              # DTO 模型
├── Services/
│   ├── OilPriceService.cs          # 油價服務（含預測邏輯）
│   └── OilPriceBackgroundService.cs # 背景服務
├── wwwroot/
│   ├── css/
│   │   └── style.css               # 樣式檔
│   ├── js/
│   │   └── app.js                  # 前端邏輯
│   ├── index.html                  # 主頁面
│   └── demo-chart.html             # 示範頁面
├── Program.cs                       # 應用程式進入點
└── appsettings.json                # 設定檔
```

## 開發說明

### 新增 Migration
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### 執行測試
```bash
dotnet test
```

### 建置發佈版本
```bash
dotnet publish -c Release
```

## 安全性

- ✅ 已通過 CodeQL 安全掃描（0 個漏洞）
- ✅ API 金鑰存放在設定檔中，不在原始碼
- ✅ 使用 Entity Framework Core 防止 SQL Injection
- ✅ CORS 可依需求設定限制

## 授權

此專案為教育用途開發。

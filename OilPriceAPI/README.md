# 台灣油價走勢分析 API

完整的油價追蹤與預測系統，支援政府開放資料平台整合與 30 天價格預測。

## 專案特色

### 🎯 核心功能
- **政府 API 整合**：自動從政府開放資料平台擷取油價資料
- **30 天預測**：使用線性迴歸預測未來油價走勢
- **虛線視覺化**：圖表中歷史資料用實線、預測資料用虛線顯示
- **自動更新**：每天 12:00 自動抓取最新油價
- **RESTful API**：提供完整的 API 端點供查詢
- **響應式介面**：支援各種裝置瀏覽

### 📊 預測演算法
- 基於最近 30 天的歷史資料
- 使用簡單線性迴歸計算趨勢
- 分別計算 92、95、98 無鉛汽油與柴油的趨勢
- 預測未來 30 天的價格變化

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

2. **設定 API 金鑰**

複製範例設定檔：
```bash
cp appsettings.Development.json.example appsettings.Development.json
```

編輯 `appsettings.Development.json`，填入你的 API 金鑰：
```json
{
  "OilPriceApi": {
    "Url": "https://superiorapis-creator.cteam.com.tw/manager/feature/proxy/93aba44236ca/pub_93aba848a466",
    "ApiKey": "YOUR_API_KEY_HERE"
  }
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

5. **瀏覽網站**
開啟瀏覽器前往 `http://localhost:5000`

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

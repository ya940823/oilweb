# 台灣汽油油價走勢分析網站

## 專題摘要

本專題建置了一套能即時查詢台灣汽油油價的網路平台。系統以 C#、ASP.NET Core Web API、JavaScript 與 SQL Server 為核心技術，並串接政府資料開放平台之油價 API，自動擷取每日油價資料並寫入本地資料庫。前端以 Chart.js 顯示 7/30/90 天油價走勢圖，提供使用者直觀的油價變化趨勢。

## 使用技術

### 前端技術
- HTML5、CSS3、JavaScript
- Bootstrap 5（快速 UI 佈局）
- Chart.js 4.4（折線圖、趨勢圖）
- Fetch API（呼叫 Web API）

### 後端技術
- ASP.NET Core 10.0 Web API（C#）
- RESTful API 設計
- Background Service 自動定時排程抓資料（Hosted Service）
- Entity Framework Core 10.0

### 資料庫
- SQLite（跨平台支援）
- Entity Framework Core
- 自動建立資料表

### 外部資料來源
- 政府資料開放平台（油價 JSON / CSV API）
- 自動解析 API → 儲存 → 前端顯示

## 系統功能

### 核心功能

1. **最新油價顯示**
   - 顯示今日 92、95、98、柴油油價
   - 顯示與前一日價格變化（▲上漲 / ▼下跌）

2. **油價走勢圖（7 / 30 / 90 天）**
   - 使用 Chart.js 折線圖
   - 動態切換不同時間區間
   - 顯示四種油價趨勢對比

3. **每日自動更新油價**
   - 使用 ASP.NET Hosted Service
   - 排程每天 12:00 自動抓取資料
   - 自動寫入 SQL Server

4. **歷史資料查詢**
   - 支援依日期區間查詢
   - API 端點提供靈活查詢

5. **統計資訊**
   - 顯示指定期間平均、最高、最低價格
   - 自動計算統計數據

6. **資料匯出**
   - 一鍵匯出 CSV 格式
   - 支援不同天數範圍

7. **響應式設計（RWD）**
   - Bootstrap 版型
   - 手機、平板、電腦皆可瀏覽

## 系統架構

```
使用者瀏覽器
        │
        ▼
  HTML + JS 前端頁面
        │ Fetch API
        ▼
ASP.NET Core Web API（後端）
        │     ▲
        │     │BackgroundService 排程每天抓油價
        ▼     │
     SQL Server 資料庫
```

## 安裝與執行

### 前置需求

- .NET 10.0 SDK
- SQLite（自動包含）

### 安裝步驟

1. **Clone 專案**
   ```bash
   git clone https://github.com/ya940823/oilweb.git
   cd oilweb/OilPriceAPI
   ```

2. **還原套件**
   ```bash
   dotnet restore
   ```

3. **執行專案**
   ```bash
   dotnet run
   ```

4. **瀏覽網站**
   - 開啟瀏覽器前往 `https://localhost:5001` 或 `http://localhost:5000`
   - 系統會自動建立資料庫並載入 90 天歷史資料

### 資料庫設定

專案使用 SQLite，連線字串定義在 `appsettings.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=OilPriceDB.db"
  }
}
```

資料庫檔案會自動在專案根目錄建立。如需使用 SQL Server，請安裝 `Microsoft.EntityFrameworkCore.SqlServer` 套件並修改連線字串。

## API 端點

### 取得最新油價
```
GET /api/oilprices/latest
```

### 取得歷史資料
```
GET /api/oilprices/history?days=30
```

### 取得日期範圍資料
```
GET /api/oilprices/range?start=2024-01-01&end=2024-12-31
```

### 匯出 CSV
```
GET /api/oilprices/export?days=30
```

### 取得統計資訊
```
GET /api/oilprices/statistics?days=30
```

## 專案結構

```
OilPriceAPI/
├── Controllers/          # API 控制器
│   └── OilPricesController.cs
├── Data/                # 資料庫上下文
│   └── OilPriceContext.cs
├── Models/              # 資料模型
│   ├── OilPrice.cs
│   └── OilPriceDto.cs
├── Services/            # 背景服務
│   ├── OilPriceService.cs
│   └── OilPriceBackgroundService.cs
├── wwwroot/             # 前端靜態檔案
│   ├── css/
│   │   └── style.css
│   ├── js/
│   │   └── app.js
│   └── index.html
├── Program.cs           # 應用程式進入點
└── appsettings.json     # 設定檔
```

## 功能展示

### 主要畫面
- 最新油價卡片顯示（四種油價）
- 價格變化指示器（▲上漲 / ▼下跌）
- 油價走勢圖（可切換 7/30/90 天）
- 統計資訊（平均、最高、最低）
- 資料匯出按鈕

### 自動化功能
- 背景服務每天 12:00 自動執行
- 自動從政府開放資料平台抓取油價
- 自動儲存至資料庫
- 避免重複資料

## 開發團隊

此專題為畢業專題作品，展示完整的全端開發能力，包括：
- 後端 API 設計與實作
- 資料庫設計與 ORM 操作
- 前端網頁設計與資料視覺化
- 背景服務與排程處理
- RESTful API 設計原則

## 授權

本專題為教育用途開發。
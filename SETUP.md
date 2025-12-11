# 台灣汽油油價走勢分析網站 - 安裝指南

## 快速開始

### 1. 環境需求
- .NET 10.0 SDK 或更高版本
- 任何支援 .NET 的作業系統（Windows、macOS、Linux）
- 網頁瀏覽器（Chrome、Firefox、Edge、Safari）

### 2. 下載與安裝

```bash
# Clone 專案
git clone https://github.com/ya940823/oilweb.git
cd oilweb/OilPriceAPI

# 還原 NuGet 套件
dotnet restore

# 建置專案
dotnet build
```

### 3. 執行應用程式

```bash
# 執行專案
dotnet run

# 或指定網址
dotnet run --urls="http://localhost:5000"
```

### 4. 瀏覽網站

開啟瀏覽器前往：
- http://localhost:5000

首次執行時，系統會：
1. 自動建立 SQLite 資料庫（OilPriceDB.db）
2. 自動載入過去 90 天的模擬油價資料
3. 啟動背景服務，於每天 12:00 自動更新油價

## 專案結構說明

```
OilPriceAPI/
├── Controllers/              # API 控制器
│   └── OilPricesController.cs   # 油價 API 端點
├── Data/                    # 資料庫相關
│   └── OilPriceContext.cs      # Entity Framework 資料庫上下文
├── Models/                  # 資料模型
│   ├── OilPrice.cs            # 油價實體模型
│   └── OilPriceDto.cs         # 油價資料傳輸物件
├── Services/                # 服務層
│   ├── OilPriceService.cs     # 油價資料服務
│   └── OilPriceBackgroundService.cs  # 背景排程服務
├── wwwroot/                 # 前端靜態檔案
│   ├── css/
│   │   └── style.css         # 自訂樣式表
│   ├── js/
│   │   └── app.js            # 前端 JavaScript
│   └── index.html            # 主頁面
├── Program.cs               # 應用程式進入點
├── appsettings.json         # 應用程式設定
└── OilPriceAPI.csproj       # 專案檔
```

## API 端點說明

### 取得最新油價
```
GET /api/oilprices/latest
```

回應範例：
```json
{
  "date": "2025-12-11T00:00:00",
  "gas92": 30.5,
  "gas95": 30.6,
  "gas98": 33.8,
  "diesel": 27.6,
  "gas92Change": -0.3,
  "gas95Change": -1.5,
  "gas98Change": -0.4,
  "dieselChange": 0.0
}
```

### 取得歷史資料
```
GET /api/oilprices/history?days=30
```

參數：
- `days`: 要取得的天數（1-365）

### 取得日期範圍資料
```
GET /api/oilprices/range?start=2024-01-01&end=2024-12-31
```

參數：
- `start`: 開始日期（ISO 8601 格式）
- `end`: 結束日期（ISO 8601 格式）

### 取得統計資訊
```
GET /api/oilprices/statistics?days=30
```

參數：
- `days`: 統計天數（預設 30）

回應範例：
```json
{
  "period": "30 days",
  "gas92": {
    "average": 29.81,
    "min": 28.88,
    "max": 30.77
  },
  "gas95": {
    "average": 31.42,
    "min": 30.36,
    "max": 32.27
  },
  // ... 其他油種
}
```

### 匯出 CSV
```
GET /api/oilprices/export?days=30
```

參數：
- `days`: 要匯出的天數

回應：CSV 檔案下載

## 資料庫設定

### 使用 SQLite（預設）

專案預設使用 SQLite，無需額外設定。資料庫檔案會在專案根目錄自動建立。

連線字串（appsettings.json）：
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=OilPriceDB.db"
  }
}
```

### 切換到 SQL Server（選用）

如需使用 SQL Server：

1. 安裝套件：
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

2. 修改 `Program.cs`：
```csharp
// 將這行：
options.UseSqlite(connectionString)

// 改為：
options.UseSqlServer(connectionString)
```

3. 更新 `appsettings.json`：
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=OilPriceDB;Trusted_Connection=true;"
  }
}
```

## 背景服務說明

系統包含一個背景服務（`OilPriceBackgroundService`），會自動在每天 12:00 執行以下動作：

1. 呼叫政府資料開放平台 API
2. 取得最新油價資料
3. 儲存至資料庫
4. 避免重複資料

背景服務會在應用程式啟動時自動執行，並記錄執行狀態到 Console。

## 前端功能

### 今日油價顯示
- 顯示 92、95、98 無鉛汽油及柴油價格
- 顯示與前一日的價格變化
- 使用顏色區分漲跌（紅色▲上漲、綠色▼下跌）

### 油價走勢圖
- 可切換 7、30、90 天的價格趨勢
- 使用 Chart.js 繪製折線圖
- 四種油品價格對比顯示

### 統計資訊
- 顯示指定期間的平均、最低、最高價格
- 自動計算並顯示

### 資料匯出
- 一鍵匯出 CSV 格式
- 包含日期和各種油價資料

## 疑難排解

### 問題：資料庫無法建立
解決方式：
- 確認應用程式有寫入目錄的權限
- 檢查 SQLite 是否正確安裝

### 問題：背景服務未執行
解決方式：
- 檢查 Console 輸出確認服務狀態
- 查看錯誤訊息

### 問題：前端無法載入
解決方式：
- 確認應用程式正在執行
- 檢查瀏覽器 Console 是否有錯誤訊息
- 確認 API 端點可以正常存取

### 問題：圖表無法顯示
解決方式：
- 確認 Chart.js 正確載入
- 檢查瀏覽器 Console 是否有 JavaScript 錯誤
- 如果 CDN 被封鎖，需要下載 Chart.js 到本地

## 開發資訊

### 技術堆疊
- **後端**：ASP.NET Core 10.0 Web API
- **資料庫**：SQLite / SQL Server
- **ORM**：Entity Framework Core 10.0
- **前端**：HTML5, CSS3, JavaScript (ES6+)
- **UI 框架**：Bootstrap 5.3
- **圖表庫**：Chart.js 4.4

### 套件相依性
- Microsoft.EntityFrameworkCore.Sqlite (10.0.1)
- Microsoft.EntityFrameworkCore.Tools (10.0.1)
- Microsoft.EntityFrameworkCore.Design (10.0.1)
- Microsoft.AspNetCore.SpaServices.Extensions (10.0.1)

## 授權

本專題為教育用途開發，歡迎學習與研究使用。

## 貢獻

歡迎提出問題或改進建議！

## 聯絡資訊

如有問題，請在 GitHub Issues 中提出。

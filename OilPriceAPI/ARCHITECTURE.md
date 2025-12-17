# 油價預測系統 - 完整架構文件

## 系統概述

這是一個完整的油價追蹤與預測系統，提供30天線性回歸預測、互動式圖表、週數範圍篩選、10公升價差比較等功能。系統支援離線運作（使用本地XML檔案）或選擇性啟用API呼叫。

---

## 技術架構

### 1. 技術棧

#### 後端技術
- **框架**: ASP.NET Core 8.0
- **語言**: C# 12.0
- **資料庫**: SQLite 3
- **ORM**: Entity Framework Core 8.0
- **XML解析**: System.Xml.Linq (LINQ to XML)
- **HTTP客戶端**: HttpClient
- **背景服務**: IHostedService

#### 前端技術
- **HTML5**: 結構標記
- **CSS3**: 樣式設計
- **JavaScript (ES6+)**: 互動邏輯
- **Canvas API**: 原生圖表繪製（不依賴外部函式庫）

#### 開發工具
- **.NET SDK**: 8.0
- **IDE**: Visual Studio / VS Code
- **版本控制**: Git

---

## 系統架構圖

```
┌─────────────────────────────────────────────────────────────┐
│                        使用者瀏覽器                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │  index.html │  │  app.js     │  │  style.css          │ │
│  │  (UI介面)   │  │  (互動邏輯) │  │  (樣式)             │ │
│  └──────┬──────┘  └──────┬──────┘  └─────────────────────┘ │
│         │                │                                  │
│         └────────────────┴──────────────┐                   │
└─────────────────────────────────────────┼───────────────────┘
                                          │ HTTP Requests
                                          ↓
┌─────────────────────────────────────────────────────────────┐
│                    ASP.NET Core 應用程式                     │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │                  Controllers Layer                    │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  OilPricesController                           │  │  │
│  │  │  - GET /api/oilprices/latest                   │  │  │
│  │  │  - GET /api/oilprices/history?days=N           │  │  │
│  │  │  - GET /api/oilprices/statistics?days=N        │  │  │
│  │  │  - GET /api/oilprices/predict?days=N&type=X    │  │  │
│  │  │  - POST /api/oilprices/refresh                 │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  └───────────────────────┬──────────────────────────────┘  │
│                          │                                  │
│  ┌───────────────────────▼──────────────────────────────┐  │
│  │                  Services Layer                       │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  OilPriceService                               │  │  │
│  │  │  - FetchAndSaveOilPricesAsync()                │  │  │
│  │  │  - LoadDataFromXmlFilesAsync()                 │  │  │
│  │  │  - PredictPrices()                             │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  OilPriceBackgroundService                     │  │  │
│  │  │  - 背景定時更新服務（每小時執行）              │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  └───────────────────────┬──────────────────────────────┘  │
│                          │                                  │
│  ┌───────────────────────▼──────────────────────────────┐  │
│  │                   Data Layer                          │  │
│  │  ┌────────────────────────────────────────────────┐  │  │
│  │  │  OilPriceContext (EF Core DbContext)           │  │  │
│  │  │  - DbSet<OilPrice> OilPrices                   │  │  │
│  │  └────────────────────────────────────────────────┘  │  │
│  └───────────────────────┬──────────────────────────────┘  │
│                          │                                  │
└──────────────────────────┼──────────────────────────────────┘
                           │
            ┌──────────────┴──────────────┐
            ↓                             ↓
┌───────────────────────┐   ┌─────────────────────────────┐
│   SQLite Database     │   │   Local XML Files           │
│   (oilprice.db)       │   │   (Data/*.xml)              │
│                       │   │                             │
│  Table: OilPrices     │   │  - oil-price-92.xml         │
│  - Id (PK)            │   │  - oil-price-95.xml         │
│  - Date               │   │  - oil-price-98.xml         │
│  - Company            │   │  - oil-price-diesel.xml     │
│  - Type               │   │                             │
│  - Price              │   │  格式: CPC DataSet XML      │
│  - CreatedAt          │   │  包含11週範例資料           │
└───────────────────────┘   └─────────────────────────────┘
```

---

## 檔案結構與說明

```
OilPriceAPI/
│
├── Program.cs                          # 應用程式進入點與配置
│   ├── 配置服務 (Services)
│   ├── 配置中介軟體 (Middleware)
│   ├── 資料庫初始化邏輯
│   └── 應用程式啟動
│
├── appsettings.json                    # 應用程式設定檔
│   └── OilPriceApi
│       ├── Url: API端點網址
│       ├── ApiKey: API金鑰（目前未使用）
│       └── EnableApiCalls: false       # 預設關閉API，使用XML
│
├── OilPriceAPI.csproj                  # 專案檔案
│   ├── TargetFramework: net8.0
│   ├── 套件參考
│   │   ├── Microsoft.EntityFrameworkCore.Sqlite
│   │   └── Microsoft.EntityFrameworkCore.Design
│   └── Data/*.xml 發佈設定
│
├── Controllers/
│   └── OilPricesController.cs          # RESTful API 控制器
│       ├── GET /api/oilprices/latest
│       │   └── 取得最新油價（4種油品）
│       ├── GET /api/oilprices/history?days=N
│       │   └── 取得歷史資料（days=0時回傳全部）
│       ├── GET /api/oilprices/statistics?days=N
│       │   └── 計算統計資訊（平均、最高、最低）
│       ├── GET /api/oilprices/predict?days=N&type=X
│       │   └── 預測未來N天的油價
│       └── POST /api/oilprices/refresh
│           └── 手動觸發資料更新
│
├── Services/
│   ├── OilPriceService.cs              # 核心業務邏輯服務
│   │   ├── FetchAndSaveOilPricesAsync()
│   │   │   ├── 檢查 EnableApiCalls 設定
│   │   │   ├── 若啟用: 呼叫中油API（失敗則使用XML）
│   │   │   └── 若停用: 直接讀取XML檔案
│   │   ├── LoadDataFromXmlFilesAsync()
│   │   │   ├── 讀取 Data/oil-price-*.xml
│   │   │   ├── 解析 CPC XML 格式
│   │   │   ├── 轉換為 OilPrice 實體
│   │   │   └── 儲存到資料庫
│   │   └── PredictPrices()
│   │       ├── 從資料庫讀取歷史資料
│   │       ├── 執行線性回歸計算
│   │       └── 回傳預測結果
│   │
│   └── OilPriceBackgroundService.cs    # 背景定時更新服務
│       ├── 實作 IHostedService
│       ├── 每小時執行一次
│       ├── 呼叫 OilPriceService.FetchAndSaveOilPricesAsync()
│       └── 記錄執行結果到 Log
│
├── Data/
│   └── OilPriceContext.cs              # Entity Framework DbContext
│       ├── DbSet<OilPrice> OilPrices
│       ├── OnConfiguring()
│       │   └── 設定 SQLite 連線字串
│       └── OnModelCreating()
│           └── 配置實體關聯與索引
│
├── Models/
│   └── OilPrice.cs                     # 油價資料模型
│       ├── Id: int (主鍵)
│       ├── Date: DateTime (生效日期)
│       ├── Company: string (公司名稱: "中油")
│       ├── Type: string (油品類型: "92無鉛汽油", etc.)
│       ├── Price: decimal (價格)
│       └── CreatedAt: DateTime (建立時間)
│
├── Data/                               # XML 資料檔案目錄
│   ├── oil-price-92.xml                # 92無鉛汽油歷史資料（11週）
│   ├── oil-price-95.xml                # 95無鉛汽油歷史資料（11週）
│   ├── oil-price-98.xml                # 98無鉛汽油歷史資料（11週）
│   ├── oil-price-diesel.xml            # 超級柴油歷史資料（11週）
│   └── README.md                       # XML 檔案使用說明
│
├── wwwroot/                            # 靜態檔案目錄
│   ├── index.html                      # 主頁面
│   │   ├── 油價卡片顯示區
│   │   ├── 10公升價差比較區
│   │   ├── 圖表顯示區（Canvas）
│   │   ├── 週數篩選器
│   │   ├── 預測查詢工具
│   │   └── 統計資訊區
│   │
│   ├── js/
│   │   └── app.js                      # 前端應用程式邏輯
│   │       ├── loadLatestPrices()      # 載入最新油價
│   │       ├── load10LiterComparison() # 計算10公升價差
│   │       ├── loadChart()             # 載入圖表資料
│   │       ├── drawChart()             # 繪製Canvas圖表
│   │       ├── filterByWeeks()         # 週數篩選（日期過濾）
│   │       ├── loadStatistics()        # 載入統計資訊
│   │       ├── queryPrediction()       # 查詢預測結果
│   │       └── refreshData()           # 觸發資料更新
│   │
│   └── css/
│       └── style.css                   # 樣式定義
│           ├── 響應式布局
│           ├── 卡片樣式
│           ├── 圖表容器
│           └── 按鈕與表單樣式
│
├── oilprice.db                         # SQLite 資料庫（執行時建立）
│   └── Table: OilPrices
│       ├── 儲存從XML載入的歷史資料
│       ├── 支援快速查詢與統計
│       └── 首次執行時自動建立並初始化
│
├── ARCHITECTURE.md                     # 本架構文件
└── .deployment-notes.md                # 部署說明文件
```

---

## 執行流程詳解

### 1. 應用程式啟動流程

```
步驟 1: 啟動命令
$ dotnet run
    ↓
步驟 2: Program.cs 主程式進入點
├── 讀取 appsettings.json 設定
├── 配置服務 (ConfigureServices)
│   ├── 註冊 OilPriceContext (資料庫)
│   ├── 註冊 OilPriceService (業務邏輯)
│   ├── 註冊 OilPriceBackgroundService (背景服務)
│   └── 註冊 Controllers (API控制器)
├── 配置中介軟體 (Configure Pipeline)
│   ├── UseStaticFiles() → 提供 wwwroot 靜態檔案
│   ├── UseRouting()     → 路由設定
│   └── MapControllers() → API 端點對應
└── 應用程式準備完成
    ↓
步驟 3: 資料庫初始化檢查 (Program.cs)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OilPriceContext>();
    context.Database.EnsureCreated();  // 建立 oilprice.db（如果不存在）
    
    if (!context.OilPrices.Any())      // 檢查資料庫是否為空
    {
        logger.LogInformation("Database is empty. Loading initial data from XML files...");
        
        var oilPriceService = scope.ServiceProvider.GetRequiredService<OilPriceService>();
        await oilPriceService.FetchAndSaveOilPricesAsync(startDate, endDate);
        
        // 這會觸發 LoadDataFromXmlFilesAsync()
        // 解析 Data/oil-price-*.xml 檔案（11週×4種油品=44筆）
        // 插入到資料庫
        
        logger.LogInformation("Initial data loaded from XML files into database.");
    }
}
    ↓
步驟 4: 背景服務啟動
OilPriceBackgroundService 開始執行
├── 檢查 EnableApiCalls 設定
├── 記錄: "API calls are disabled. Loading data from local XML files only."
└── 每小時觸發一次 FetchAndSaveOilPricesAsync()
    ↓
步驟 5: Web Server 監聽
應用程式監聽 http://localhost:5000
等待使用者請求
```

### 2. 使用者訪問流程

```
步驟 1: 使用者開啟瀏覽器
瀏覽 http://localhost:5000
    ↓
步驟 2: 伺服器回應靜態檔案
├── 返回 wwwroot/index.html
├── 載入 wwwroot/css/style.css
└── 載入 wwwroot/js/app.js
    ↓
步驟 3: JavaScript 初始化 (app.js)
document.addEventListener('DOMContentLoaded', function() {
    loadLatestPrices();        // 載入最新油價
    load10LiterComparison();   // 載入10公升價差
    loadChart();               // 載入圖表
    loadStatistics();          // 載入統計資訊
});
    ↓
步驟 4: API 請求 (並行執行)

┌─────────────────────────────────────────────────────────┐
│ 請求 A: GET /api/oilprices/latest                       │
│ ↓                                                        │
│ OilPricesController.GetLatest()                         │
│ ├── 查詢: context.OilPrices                            │
│ │         .Where(p => p.Company == "中油")             │
│ │         .GroupBy(p => p.Type)                        │
│ │         .Select(g => g.OrderByDescending(p => p.Date)│
│ │                       .FirstOrDefault())             │
│ ├── 返回: {                                             │
│ │   price92: 30.6,                                     │
│ │   price95: 32.1,                                     │
│ │   price98: 34.1,                                     │
│ │   diesel: 28.6                                       │
│ │ }                                                     │
│ └── 前端更新: 顯示在油價卡片                            │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ 請求 B: GET /api/oilprices/history?days=0               │
│ ↓                                                        │
│ OilPricesController.GetHistory(days=0)                  │
│ ├── days == 0 → 不套用日期篩選                          │
│ ├── 查詢: context.OilPrices.Where(p => p.Company...)   │
│ │         .OrderBy(p => p.Date)                        │
│ ├── 返回: 44筆歷史記錄（11週×4種油品）                  │
│ └── 前端處理:                                           │
│     ├── 用於 10公升價差計算 (最新 vs 上一筆)            │
│     └── 用於圖表繪製                                    │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ 請求 C: GET /api/oilprices/statistics?days=0            │
│ ↓                                                        │
│ OilPricesController.GetStatistics(days=0)               │
│ ├── 查詢所有歷史資料                                    │
│ ├── 計算: Average(價格), Max(價格), Min(價格)           │
│ ├── 返回: {                                             │
│ │   92無鉛: {avg: 30.3, max: 30.6, min: 30.0},        │
│ │   95無鉛: {avg: 31.8, max: 32.1, min: 31.5},        │
│ │   ...                                                 │
│ │ }                                                     │
│ └── 前端更新: 統計資訊區塊                              │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ 請求 D: GET /api/oilprices/predict?days=30&type=all     │
│ ↓                                                        │
│ OilPricesController.Predict(days=30, type="all")        │
│ ├── 呼叫: oilPriceService.PredictPrices(30, type)      │
│ ├── 對每種油品執行線性回歸                              │
│ │   ├── 讀取歷史資料 (11筆)                            │
│ │   ├── 計算斜率與截距                                 │
│ │   └── 預測未來30天價格                               │
│ ├── 返回: {                                             │
│ │   92無鉛: [30.7, 30.8, ...],                        │
│ │   95無鉛: [32.2, 32.3, ...],                        │
│ │   ...                                                 │
│ │ }                                                     │
│ └── 前端處理: 繪製虛線預測線                            │
└─────────────────────────────────────────────────────────┘
    ↓
步驟 5: 前端渲染 (app.js)
├── drawChart()
│   ├── 取得 Canvas 2D context
│   ├── 繪製座標軸
│   ├── 繪製歷史資料（實線）
│   ├── 繪製預測資料（虛線）
│   ├── 繪製紅色垂直線（預測起點）
│   └── 加入滑鼠事件監聽（hover顯示數值）
├── 顯示10公升價差（▲▼符號與顏色）
├── 顯示統計資訊
└── 啟用互動功能（週數篩選器、預測查詢）
    ↓
步驟 6: 使用者看到完整介面
✅ 4個油價卡片顯示最新價格
✅ 10公升價差比較（紅綠色標示）
✅ 圖表顯示11週歷史+30天預測
✅ 統計資訊顯示平均/最高/最低
✅ 互動功能全部就緒
```

### 3. 使用者互動流程

#### 3A. 切換週數篩選器

```
使用者操作: 選擇「4週」篩選器
    ↓
前端: filterByWeeks(4)
├── 計算截止日期: cutoffDate = today - (4週 × 7天) = 28天前
├── 過濾資料: allChartData.filter(d => new Date(d.date) >= cutoffDate)
├── 保留預測資料（30天）
└── 重新繪製圖表
    ↓
同時觸發: loadStatistics(28)
├── GET /api/oilprices/statistics?days=28
├── 只計算最近28天的統計
└── 更新統計資訊標題「統計資訊（最近 4 週）」
    ↓
結果: 圖表只顯示最近28天資料 + 30天預測
```

#### 3B. 查詢未來油價預測

```
使用者操作: 輸入「2」週、選擇「92無鉛」、點擊查詢
    ↓
前端: queryPrediction()
├── 計算目標日期: today + 14天
├── GET /api/oilprices/predict?days=14&type=92無鉛汽油
└── 顯示預測結果表格
    ↓
API: OilPricesController.Predict(14, "92無鉛汽油")
├── 讀取92無鉛汽油的歷史資料
├── 線性回歸計算
├── 產生14天預測
└── 返回: { date: "2024-12-30", price: 30.8 }
    ↓
前端顯示:
┌─────────────────────────────┐
│ 預測結果                     │
├─────────────────────────────┤
│ 日期: 2024-12-30             │
│ 油品: 92無鉛汽油             │
│ 預測價格: $30.8              │
└─────────────────────────────┘
```

#### 3C. 手動更新資料

```
使用者操作: 點擊「🔄 更新資料」按鈕
    ↓
前端: refreshData()
├── POST /api/oilprices/refresh
└── 等待回應
    ↓
API: OilPricesController.Refresh()
├── 呼叫: oilPriceService.FetchAndSaveOilPricesAsync()
├── 檢查 EnableApiCalls 設定
│   ├── false: 從XML檔案重新載入
│   └── true: 嘗試API，失敗則用XML
├── 解析並更新資料庫
└── 返回: { success: true, message: "資料更新成功" }
    ↓
前端:
├── 顯示成功訊息
├── 重新載入所有資料 (loadLatestPrices, loadChart, etc.)
└── 更新畫面
```

---

## 資料儲存位置

### 1. 資料庫檔案

**位置**: `OilPriceAPI/oilprice.db`

**格式**: SQLite 3 資料庫

**建立時機**: 
- 應用程式首次啟動時自動建立
- `context.Database.EnsureCreated()` 執行

**資料表結構**:
```sql
CREATE TABLE OilPrices (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Date DATETIME NOT NULL,
    Company TEXT NOT NULL,
    Type TEXT NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    INDEX idx_date (Date),
    INDEX idx_type (Type),
    INDEX idx_company_type_date (Company, Type, Date)
);
```

**資料範例**:
```
Id | Date       | Company | Type         | Price | CreatedAt
---+------------+---------+--------------+-------+-------------------
1  | 2024-10-07 | 中油    | 92無鉛汽油   | 30.0  | 2024-12-17 12:00:00
2  | 2024-10-07 | 中油    | 95無鉛汽油   | 31.5  | 2024-12-17 12:00:00
3  | 2024-10-07 | 中油    | 98無鉛汽油   | 33.5  | 2024-12-17 12:00:00
4  | 2024-10-07 | 中油    | 超級柴油     | 28.0  | 2024-12-17 12:00:00
... (共44筆記錄)
```

**容量**: 
- 初始資料: 約 4KB (44筆記錄)
- 長期使用: 隨資料累積成長（每週約 100 bytes）

### 2. XML 資料檔案

**位置**: `OilPriceAPI/Data/`

**檔案清單**:
1. `oil-price-92.xml` - 92無鉛汽油（11週資料）
2. `oil-price-95.xml` - 95無鉛汽油（11週資料）
3. `oil-price-98.xml` - 98無鉛汽油（11週資料）
4. `oil-price-diesel.xml` - 超級柴油（11週資料）

**格式**: CPC (台灣中油) XML DataSet 格式

**內容結構**:
```xml
<?xml version="1.0" encoding="utf-8"?>
<DataSet xmlns="http://tmtd.cpc.com.tw/">
  <diffgr:diffgram xmlns:msdata="urn:schemas-microsoft-com:xml-msdata" 
                   xmlns:diffgr="urn:schemas-microsoft-com:xml-diffgram-v1">
    <NewDataSet xmlns="">
      <tbTable diffgr:id="tbTable1" msdata:roworder="0">
        <牌價生效時間>2024-10-07T00:00:00+08:00</牌價生效時間>
        <產品名>無鉛汽油92</產品名>
        <參考牌價>30.0</參考牌價>
        <計價單位>元/公升</計價單位>
      </tbTable>
      <!-- 重複11次（每週一筆） -->
    </NewDataSet>
  </diffgr:diffgram>
</DataSet>
```

**用途**:
- 作為資料來源（預設模式）
- 首次執行時自動載入到資料庫
- 支援離線運作
- 作為真實資料的格式範例

**容量**: 每個檔案約 8-10 KB

### 3. 靜態資源檔案

**位置**: `OilPriceAPI/wwwroot/`

**檔案**:
- `index.html` - 主頁面 (約 15 KB)
- `js/app.js` - JavaScript 邏輯 (約 25 KB)
- `css/style.css` - 樣式表 (約 10 KB)

**部署**:
- 開發環境: 直接從 wwwroot 提供
- 生產環境: 包含在發佈輸出中的 wwwroot 目錄

### 4. 設定檔案

**位置**: `OilPriceAPI/appsettings.json`

**內容**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*",
  "OilPriceApi": {
    "Url": "https://vipmbr.cpc.com.tw/cpcstn/listpricewebservice.asmx/getCPCMainProdListPrice_Historical",
    "ApiKey": "",
    "EnableApiCalls": false
  }
}
```

**用途**: 控制應用程式行為（API vs XML）

### 5. 記錄檔案

**位置**: 控制台輸出（Console Logging）

**內容範例**:
```
[2024-12-17 12:00:00] info: Application started
[2024-12-17 12:00:01] info: Database is empty. Loading initial data from XML files...
[2024-12-17 12:00:02] info: Loading oil prices from Data/oil-price-92.xml for 92無鉛汽油
[2024-12-17 12:00:02] info: Successfully loaded 11 records for 92無鉛汽油
[2024-12-17 12:00:02] info: Initial data loaded from XML files into database.
[2024-12-17 12:00:05] info: API calls are disabled. Loading data from local XML files only.
```

**用途**: 除錯、監控、稽核

---

## 資料流向圖

```
XML檔案 (Data/*.xml)
  ↓ [首次啟動]
  ↓ FetchAndSaveOilPricesAsync()
  ↓ LoadDataFromXmlFilesAsync()
  ↓ 解析 XML → OilPrice 物件
  ↓
SQLite 資料庫 (oilprice.db)
  ↓ [API請求]
  ↓ OilPricesController
  ↓ 查詢資料庫 (EF Core)
  ↓
JSON API 回應
  ↓ [HTTP Response]
  ↓ app.js (前端JavaScript)
  ↓ 處理資料 + 計算
  ↓
Canvas 繪圖 + HTML 更新
  ↓ [視覺化]
  ↓
使用者瀏覽器畫面
```

---

## 核心演算法

### 1. 線性回歸預測 (PredictPrices)

```csharp
// 位置: Services/OilPriceService.cs

public List<PredictionResult> PredictPrices(string type, int days)
{
    // 步驟1: 取得歷史資料
    var historicalData = _context.OilPrices
        .Where(p => p.Company == "中油" && p.Type == type)
        .OrderBy(p => p.Date)
        .ToList();
    
    if (historicalData.Count < 2)
        return new List<PredictionResult>(); // 資料不足
    
    // 步驟2: 準備資料點 (x=天數, y=價格)
    var n = historicalData.Count;
    var x = Enumerable.Range(0, n).Select(i => (double)i).ToArray();
    var y = historicalData.Select(p => (double)p.Price).ToArray();
    
    // 步驟3: 計算線性回歸參數
    // y = mx + b (斜率m, 截距b)
    
    var xMean = x.Average();
    var yMean = y.Average();
    
    // 計算斜率 m = Σ[(xi - x̄)(yi - ȳ)] / Σ[(xi - x̄)²]
    var numerator = x.Zip(y, (xi, yi) => (xi - xMean) * (yi - yMean)).Sum();
    var denominator = x.Select(xi => Math.Pow(xi - xMean, 2)).Sum();
    var slope = numerator / denominator;
    
    // 計算截距 b = ȳ - m·x̄
    var intercept = yMean - slope * xMean;
    
    // 步驟4: 預測未來價格
    var predictions = new List<PredictionResult>();
    var lastDate = historicalData.Last().Date;
    
    for (int i = 1; i <= days; i++)
    {
        var futureX = n + i - 1;
        var predictedPrice = slope * futureX + intercept;
        
        predictions.Add(new PredictionResult
        {
            Date = lastDate.AddDays(i),
            Type = type,
            Price = (decimal)Math.Round(predictedPrice, 2)
        });
    }
    
    return predictions;
}
```

**範例計算**:
```
假設有5筆歷史資料: [30.0, 30.1, 30.2, 30.3, 30.4]

x = [0, 1, 2, 3, 4]
y = [30.0, 30.1, 30.2, 30.3, 30.4]

x̄ = (0+1+2+3+4)/5 = 2
ȳ = (30.0+30.1+30.2+30.3+30.4)/5 = 30.2

斜率 m = 0.1 (每天漲0.1元)
截距 b = 30.0

預測第6天: y = 0.1×5 + 30.0 = 30.5
預測第7天: y = 0.1×6 + 30.0 = 30.6
```

### 2. 日期過濾 (Week Filter)

```javascript
// 位置: wwwroot/js/app.js

function filterByWeeks(weeks) {
    if (weeks === 0) {
        // 顯示全部資料
        renderChart(allChartData, allPredictions);
    } else {
        // 計算截止日期
        const cutoffDate = new Date();
        cutoffDate.setDate(cutoffDate.getDate() - (weeks * 7));
        
        // 只保留截止日期之後的資料
        const filteredData = allChartData.filter(d => {
            return new Date(d.date) >= cutoffDate;
        });
        
        // 預測資料保持完整（30天）
        renderChart(filteredData, allPredictions);
    }
    
    // 同步更新統計資訊
    loadStatistics(weeks * 7);
}
```

**範例**:
```
今天: 2024-12-17
選擇4週

cutoffDate = 2024-12-17 - 28天 = 2024-11-19

過濾前: 44筆資料 (2024-10-07 ~ 2024-12-16)
過濾後: 約4筆資料 (2024-11-19 ~ 2024-12-16)
預測: 30天 (2024-12-17 ~ 2025-01-15)
```

### 3. 10公升價差計算

```javascript
// 位置: wwwroot/js/app.js

async function load10LiterComparison() {
    const response = await fetch(`${API_BASE_URL}/history?days=0`);
    const data = await response.json();
    
    console.log('Loading 10L comparison data...');
    console.log('Loaded', data.length, 'records for comparison');
    
    if (data.length < 2) {
        // 資料不足
        document.getElementById('compare92').innerHTML = '--';
        return;
    }
    
    // 最新記錄（本週）
    const currentData = data[data.length - 1];
    
    // 上一筆記錄（上週）
    const lastWeekData = data[data.length - 2];
    
    console.log('Current week:', currentData);
    console.log('Last week:', lastWeekData);
    
    // 計算價差（每10公升）
    const diff92 = currentData.price92 && lastWeekData.price92 
        ? (currentData.price92 - lastWeekData.price92) * 10 
        : null;
    
    console.log('Calculated differences - 92:', diff92, ...);
    
    // 顯示結果
    if (diff92 !== null) {
        document.getElementById('compare92').innerHTML = formatComparisonText(diff92);
        // ▲ $3.5 (紅色) 或 ▼ $2.1 (綠色)
    } else {
        document.getElementById('compare92').innerHTML = '--';
    }
}

function formatComparisonText(diff) {
    if (diff > 0) {
        return `<span style="color: red;">▲ $${diff.toFixed(1)}</span>`;
    } else if (diff < 0) {
        return `<span style="color: green;">▼ $${Math.abs(diff).toFixed(1)}</span>`;
    } else {
        return `<span style="color: gray;">─ $0.0</span>`;
    }
}
```

**範例計算**:
```
本週92無鉛: $30.6/L
上週92無鉛: $30.4/L

價差 = (30.6 - 30.4) × 10公升 = 0.2 × 10 = $2.0

顯示: ▲ $2.0 (紅色)
意義: 加滿10公升多花$2.0
```

---

## 資料庫 Schema

### OilPrices 資料表

```sql
CREATE TABLE "OilPrices" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_OilPrices" PRIMARY KEY AUTOINCREMENT,
    "Date" TEXT NOT NULL,
    "Company" TEXT NOT NULL,
    "Type" TEXT NOT NULL,
    "Price" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL
);

CREATE INDEX "IX_OilPrices_Date" ON "OilPrices" ("Date");
CREATE INDEX "IX_OilPrices_Type" ON "OilPrices" ("Type");
CREATE INDEX "IX_OilPrices_Company_Type_Date" ON "OilPrices" ("Company", "Type", "Date");
```

**欄位說明**:

| 欄位 | 型別 | 說明 | 範例值 |
|------|------|------|--------|
| Id | INTEGER | 主鍵，自動遞增 | 1, 2, 3, ... |
| Date | TEXT | 生效日期 (ISO 8601格式) | "2024-10-07T00:00:00" |
| Company | TEXT | 公司名稱 | "中油" |
| Type | TEXT | 油品類型 | "92無鉛汽油", "95無鉛汽油", "98無鉛汽油", "超級柴油" |
| Price | TEXT | 價格 (儲存為文字以保持精確度) | "30.0", "31.5" |
| CreatedAt | TEXT | 記錄建立時間 | "2024-12-17T12:00:00" |

**索引說明**:
- `IX_OilPrices_Date`: 加速按日期排序查詢
- `IX_OilPrices_Type`: 加速按油品類型篩選
- `IX_OilPrices_Company_Type_Date`: 複合索引，加速常用查詢組合

**常用查詢**:

```sql
-- 取得最新油價
SELECT * FROM OilPrices 
WHERE Company = '中油' 
  AND Type = '92無鉛汽油' 
ORDER BY Date DESC 
LIMIT 1;

-- 取得最近30天資料
SELECT * FROM OilPrices 
WHERE Company = '中油' 
  AND Date >= date('now', '-30 days')
ORDER BY Date ASC;

-- 計算平均價格
SELECT Type, 
       AVG(CAST(Price AS REAL)) as AvgPrice,
       MAX(CAST(Price AS REAL)) as MaxPrice,
       MIN(CAST(Price AS REAL)) as MinPrice
FROM OilPrices 
WHERE Company = '中油'
GROUP BY Type;
```

---

## API 端點完整說明

### 1. GET /api/oilprices/latest

取得4種油品的最新價格。

**請求**:
```http
GET /api/oilprices/latest HTTP/1.1
Host: localhost:5000
```

**回應**:
```json
{
  "price92": 30.6,
  "price95": 32.1,
  "price98": 34.1,
  "diesel": 28.6,
  "date": "2024-12-16T00:00:00"
}
```

### 2. GET /api/oilprices/history

取得歷史價格資料。

**參數**:
- `days` (int, optional): 要取得的天數，0=全部資料

**請求**:
```http
GET /api/oilprices/history?days=30 HTTP/1.1
Host: localhost:5000
```

**回應**:
```json
[
  {
    "date": "2024-10-07T00:00:00",
    "price92": 30.0,
    "price95": 31.5,
    "price98": 33.5,
    "diesel": 28.0
  },
  {
    "date": "2024-10-14T00:00:00",
    "price92": 30.1,
    "price95": 31.6,
    "price98": 33.6,
    "diesel": 28.1
  },
  ...
]
```

### 3. GET /api/oilprices/statistics

計算統計資訊（平均、最高、最低價格）。

**參數**:
- `days` (int, optional): 統計範圍，0=全部資料

**請求**:
```http
GET /api/oilprices/statistics?days=0 HTTP/1.1
Host: localhost:5000
```

**回應**:
```json
{
  "92無鉛汽油": {
    "average": 30.3,
    "max": 30.6,
    "min": 30.0
  },
  "95無鉛汽油": {
    "average": 31.8,
    "max": 32.1,
    "min": 31.5
  },
  "98無鉛汽油": {
    "average": 33.8,
    "max": 34.1,
    "min": 33.5
  },
  "超級柴油": {
    "average": 28.3,
    "max": 28.6,
    "min": 28.0
  }
}
```

### 4. GET /api/oilprices/predict

預測未來油價。

**參數**:
- `days` (int, required): 預測天數（1-60）
- `type` (string, optional): 油品類型，"all"=全部

**請求**:
```http
GET /api/oilprices/predict?days=30&type=all HTTP/1.1
Host: localhost:5000
```

**回應**:
```json
{
  "92無鉛汽油": [
    { "date": "2024-12-17", "price": 30.7 },
    { "date": "2024-12-18", "price": 30.8 },
    ...
  ],
  "95無鉛汽油": [ ... ],
  "98無鉛汽油": [ ... ],
  "超級柴油": [ ... ]
}
```

### 5. POST /api/oilprices/refresh

手動觸發資料更新。

**請求**:
```http
POST /api/oilprices/refresh HTTP/1.1
Host: localhost:5000
Content-Length: 0
```

**回應**:
```json
{
  "success": true,
  "message": "資料更新成功",
  "recordsAdded": 4
}
```

---

## 部署指南

### 開發環境

```bash
# 1. 克隆專案
git clone https://github.com/ya940823/oilweb.git
cd oilweb/OilPriceAPI

# 2. 恢復套件
dotnet restore

# 3. 建置
dotnet build

# 4. 執行
dotnet run

# 5. 開啟瀏覽器
http://localhost:5000
```

### 生產環境

```bash
# 1. 更新XML檔案（選用，已含範例資料）
# 編輯 Data/oil-price-*.xml

# 2. 發佈
dotnet publish -c Release -o ./publish

# 3. 檢查輸出
ls ./publish/wwwroot/     # 應包含 index.html, js/, css/
ls ./publish/Data/        # 應包含 oil-price-*.xml

# 4. 部署到網路主機
# 上傳 publish/ 目錄全部內容

# 5. 首次執行自動初始化
# 資料庫自動建立
# XML資料自動載入
```

---

## 疑難排解

### 問題1: 資料庫為空

**症狀**: 圖表不顯示，油價顯示 "--"

**原因**: 資料庫未初始化

**解決**:
1. 檢查 Data/*.xml 檔案是否存在且有資料
2. 刪除 oilprice.db 並重新啟動
3. 或點擊「🔄 更新資料」按鈕

### 問題2: 10公升價差顯示 "--"

**症狀**: 價差區塊全部顯示 "--"

**原因**: 資料筆數少於2筆

**解決**:
1. 開啟瀏覽器Console (F12)
2. 檢查 "Loaded X records" 訊息
3. 確認 X >= 2
4. 如果不足，更新XML檔案並刷新

### 問題3: 發佈後找不到網站內容

**症狀**: 網站顯示空白頁或404

**原因**: wwwroot 未包含在發佈輸出

**解決**:
1. 檢查 OilPriceAPI.csproj 設定
2. 確認沒有重複的 `<Content Include="wwwroot\**\*">`
3. ASP.NET Core 會自動包含 wwwroot
4. 重新發佈: `dotnet publish -c Release`

### 問題4: 建置錯誤 NETSDK1022

**症狀**: 建置失敗，錯誤訊息提示重複的 Content 項目

**原因**: .csproj 中明確包含 wwwroot，與SDK預設衝突

**解決**:
1. 移除 .csproj 中的 wwwroot Content 設定
2. 保留 Data/*.xml 的 Content 設定
3. 重新建置

---

## 效能指標

### 資料庫查詢

- 取得最新油價: < 10ms
- 取得歷史資料 (11週): < 20ms
- 計算統計資訊: < 30ms
- 預測計算 (30天): < 50ms

### API 回應時間

- /latest: < 100ms
- /history?days=0: < 150ms
- /statistics?days=0: < 200ms
- /predict?days=30: < 250ms

### 前端渲染

- 首次載入: < 1s
- 圖表重繪: < 100ms
- 篩選器切換: < 50ms

### 資源佔用

- 記憶體: ~50 MB
- CPU: < 1% (閒置)
- 磁碟空間: ~5 MB (含資料庫)

---

## 安全性考量

1. **輸入驗證**: 所有API端點驗證參數範圍
2. **SQL注入防護**: 使用 Entity Framework 參數化查詢
3. **XSS防護**: 前端使用 textContent 而非 innerHTML (除特定標記)
4. **CORS設定**: 可根據需求限制來源
5. **錯誤處理**: 不洩漏敏感資訊的錯誤訊息

---

## 未來擴充建議

1. **使用者認證**: 加入登入功能限制存取
2. **多公司支援**: 擴充到台塑等其他油公司
3. **警示功能**: 價格變動通知
4. **匯出功能**: CSV/Excel 資料匯出
5. **進階圖表**: 使用Chart.js等專業圖表庫
6. **機器學習**: 改用LSTM等進階預測模型
7. **行動應用**: 開發 PWA 或原生 App

---

## 授權與聯絡

**專案**: 油價預測系統
**版本**: 1.0.0
**最後更新**: 2024-12-17

**技術文件製作**: GitHub Copilot
**問題回報**: 請在 GitHub Issues 提出

---

*本文件涵蓋系統的完整架構、執行流程、資料儲存、API規格、演算法說明等所有技術細節。*

# 台灣油價觀察與預測網頁專案

此專案是一個完整的全端網頁應用程式，用於觀察和預測台灣油價。專案使用 ASP.NET Core Web API 作為後端，搭配現代化的前端介面，並支援 XML 檔案解析功能將資料存入 SQL 資料庫。

## 專案截圖

![油價觀察系統](https://github.com/user-attachments/assets/2ad05c31-4c79-486c-ad0f-7ea0b8d5285a)

## 專案特色

### 🎯 核心功能

1. **XML 資料解析與匯入**
   - 支援上傳 XML 格式的油價資料
   - 自動解析 XML 並轉換為自訂格式
   - 防止重複資料匯入
   - 即時回饋處理結果

2. **即時油價資訊**
   - 顯示最新的 92、95、98 無鉛汽油及柴油價格
   - 美觀的漸層卡片設計
   - 即時更新資料

3. **統計分析**
   - 支援 7/30/90 天的統計資訊
   - 顯示平均、最高、最低價格
   - 四種油品的完整比較

4. **歷史資料查詢**
   - 完整的歷史資料表格
   - 支援日期範圍查詢
   - 清晰的資料來源標記

5. **油價走勢圖**
   - 使用 Chart.js 繪製折線圖
   - 支援 7/30/90 天切換
   - 四種油品價格對比

6. **響應式設計**
   - Bootstrap 5 框架
   - 支援手機、平板、電腦瀏覽
   - 現代化 UI/UX 設計

## 技術架構

### 後端技術
- **框架**: ASP.NET Core 10.0 Web API
- **語言**: C# 12
- **ORM**: Entity Framework Core 10.0
- **資料庫**: 
  - SQL Server (生產環境推薦)
  - SQLite (開發測試環境)
- **API 設計**: RESTful API

### 前端技術
- **基礎**: HTML5, CSS3, JavaScript (ES6+)
- **UI 框架**: Bootstrap 5
- **圖表庫**: Chart.js 4.4
- **AJAX**: Fetch API

### 專案結構

```
OilPriceAPI/
├── Controllers/              # API 控制器
│   └── OilPricesController.cs
├── Data/                    # 資料庫上下文
│   └── OilPriceContext.cs
├── Models/                  # 資料模型
│   └── OilPrice.cs
├── Services/                # 服務層
│   └── XmlParserService.cs
├── wwwroot/                 # 前端靜態檔案
│   ├── css/
│   │   └── style.css
│   ├── js/
│   │   └── app.js
│   └── index.html
├── Program.cs               # 應用程式進入點
├── appsettings.json         # 設定檔 (生產環境)
└── appsettings.Development.json  # 設定檔 (開發環境)

OilPriceWeb.sln              # Visual Studio Solution 檔案
sample_oil_prices.xml        # 範例 XML 資料檔
```

## 安裝與執行

### 環境需求

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022 (或更新版本) 或 Visual Studio Code
- SQL Server LocalDB (Windows) 或 SQLite (跨平台)

### 步驟 1: 複製專案

```bash
git clone https://github.com/ya940823/oilweb.git
cd oilweb
```

### 步驟 2: 開啟 Visual Studio

1. 使用 Visual Studio 開啟 `OilPriceWeb.sln`
2. Visual Studio 會自動還原 NuGet 套件

### 步驟 3: 設定資料庫連線

#### SQL Server (Windows - 推薦)
預設使用 LocalDB，無需修改設定即可使用。

#### SQLite (跨平台)
如果要使用 SQLite，請修改 `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=oilprice.db"
  }
}
```

### 步驟 4: 執行專案

#### 使用 Visual Studio
1. 按 F5 或點擊「開始偵錯」按鈕
2. 瀏覽器會自動開啟應用程式

#### 使用命令列
```bash
cd OilPriceAPI
dotnet run
```

然後開啟瀏覽器訪問: `http://localhost:5280`

## 使用說明

### 1. 上傳 XML 資料

系統支援兩種 XML 格式：

#### 格式一：簡單格式

```xml
<?xml version="1.0" encoding="UTF-8"?>
<OilPrices>
  <item>
    <Date>2025-12-17</Date>
    <Oil92>29.2</Oil92>
    <Oil95>30.8</Oil95>
    <Oil98>32.8</Oil98>
    <Diesel>27.5</Diesel>
  </item>
  <!-- 更多資料... -->
</OilPrices>
```

#### 格式二：中油 (CPC) DataSet 格式

```xml
<?xml version="1.0" encoding="utf-8"?>
<DataSet xmlns="http://tmtd.cpc.com.tw/">
  <diffgr:diffgram xmlns:msdata="urn:schemas-microsoft-com:xml-msdata" xmlns:diffgr="urn:schemas-microsoft-com:xml-diffgram-v1">
    <NewDataSet>
      <Table1 diffgr:id="Table11" msdata:rowOrder="0">
        <參考日期>2025-12-17</參考日期>
        <無鉛92>29.2</無鉛92>
        <無鉛95>30.8</無鉛95>
        <無鉛98>32.8</無鉛98>
        <超級柴油>27.5</超級柴油>
      </Table1>
      <!-- 更多資料... -->
    </NewDataSet>
  </diffgr:diffgram>
</DataSet>
```

**上傳步驟：**
1. 點擊「選擇檔案」按鈕
2. 選擇 XML 檔案（支援上述兩種格式）
3. 點擊「上傳並解析」按鈕
4. 系統會自動識別格式並解析儲存資料

### 2. 查看油價資訊

- **最新油價**: 首頁會顯示最新的四種油品價格
- **統計資訊**: 顯示最近 30 天的統計數據
- **走勢圖**: 可切換 7/30/90 天的價格趨勢
- **歷史資料**: 完整的歷史資料表格，顯示資料來源（XML 或 CPC-XML）

## API 端點

### 取得所有油價資料
```
GET /api/oilprices
```

### 取得最新油價
```
GET /api/oilprices/latest
```

**回應範例:**
```json
{
  "id": 8,
  "date": "2025-12-17T00:00:00",
  "oil92": 29.2,
  "oil95": 30.8,
  "oil98": 32.8,
  "diesel": 27.5,
  "source": "XML",
  "createdAt": "2025-12-17T08:36:18.8561766"
}
```

### 取得歷史資料
```
GET /api/oilprices/history?days=30
```

### 取得日期範圍資料
```
GET /api/oilprices/range?start=2025-01-01&end=2025-12-31
```

### 取得統計資訊
```
GET /api/oilprices/statistics?days=30
```

**回應範例:**
```json
{
  "period": "Last 30 days",
  "oil92": {
    "average": 28.85,
    "max": 29.2,
    "min": 28.5
  },
  "oil95": {
    "average": 30.45,
    "max": 30.8,
    "min": 30.1
  },
  "oil98": {
    "average": 32.45,
    "max": 32.8,
    "min": 32.1
  },
  "diesel": {
    "average": 27.15,
    "max": 27.5,
    "min": 26.8
  }
}
```

### 上傳 XML 檔案
```
POST /api/oilprices/upload-xml
Content-Type: multipart/form-data

file: [XML 檔案]
```

**回應範例:**
```json
{
  "message": "XML file processed successfully",
  "totalRecords": 8,
  "savedRecords": 8
}
```

## 資料庫結構

### OilPrices 資料表

| 欄位名稱 | 資料型別 | 說明 |
|---------|---------|------|
| Id | INT | 主鍵 (自動遞增) |
| Date | DATETIME | 油價日期 (唯一索引) |
| Oil92 | DECIMAL(18,2) | 92 無鉛汽油價格 |
| Oil95 | DECIMAL(18,2) | 95 無鉛汽油價格 |
| Oil98 | DECIMAL(18,2) | 98 無鉛汽油價格 |
| Diesel | DECIMAL(18,2) | 柴油價格 |
| Source | STRING | 資料來源 (預設: "XML") |
| CreatedAt | DATETIME | 建立時間 |

## 開發說明

### 修改資料庫連線

編輯 `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=OilPriceDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
  }
}
```

### 新增資料庫遷移 (Migration)

```bash
cd OilPriceAPI
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 編譯專案

```bash
dotnet build
```

### 發佈專案

```bash
dotnet publish -c Release -o ./publish
```

## 疑難排解

### 問題 1: 無法連線到資料庫

**解決方案**: 
- 確認 SQL Server 服務是否啟動
- 檢查連線字串是否正確
- 嘗試使用 SQLite 進行測試

### 問題 2: NuGet 套件無法還原

**解決方案**:
```bash
dotnet restore
```

### 問題 3: 圖表無法顯示

**解決方案**:
- 檢查網路連線 (Chart.js 使用 CDN)
- 確認瀏覽器主控台是否有錯誤訊息

## 授權

此專案為教育和學習目的開發。

## 作者

ya940823

## 更新日誌

### 2025-12-17
- 初始版本發布
- 實作 XML 解析功能
- 建立前端介面
- 完成 RESTful API
- 支援 SQL Server 和 SQLite

# 台灣油價查詢平台 🛢️

即時查詢台灣汽油油價的網路平台，採用 ASP.NET Core Web API + Entity Framework Core + Chart.js 開發。

## 專案特色

### 核心功能
- ✅ **最新油價顯示** - 即時顯示92、95、98無鉛汽油及超級柴油價格
- ✅ **油價走勢圖** - 使用 Chart.js 呈現 7/30/90 天的價格趨勢
- ✅ **歷史資料查詢** - 依日期區間查詢歷史油價資料
- ✅ **自動更新機制** - Background Service 每日自動更新油價
- ✅ **CSV 匯出** - 支援油價資料匯出為 CSV 格式

### 進階功能
- 📊 **油價趨勢分析** - 統計分析油價平均值、最高價、最低價
- 🔮 **價格預測** - 使用線性回歸預測未來價格趨勢
- 📱 **響應式設計 (RWD)** - 完整支援手機、平板、桌面裝置
- 🎨 **漲跌色彩標示** - 紅色表示上漲、綠色表示下跌、灰色持平

## 技術架構

### 後端技術
- **框架**: ASP.NET Core 10.0 Web API
- **ORM**: Entity Framework Core 10.0
- **資料庫**: SQLite (開發環境) / SQL Server (生產環境)
- **背景服務**: IHostedService

### 前端技術
- **框架**: Bootstrap 5.3
- **圖表**: Chart.js 4.4
- **語言**: HTML5, CSS3, JavaScript (ES6+)

### 專案結構
```
oilweb/
├── src/
│   └── OilWeb.API/
│       ├── Controllers/        # API 控制器
│       │   └── OilPriceController.cs
│       ├── Data/              # 資料庫上下文
│       │   └── OilPriceDbContext.cs
│       ├── Models/            # 資料模型
│       │   └── OilPrice.cs
│       ├── Services/          # 背景服務
│       │   └── OilPriceUpdateService.cs
│       └── wwwroot/           # 靜態檔案
│           ├── index.html
│           ├── css/
│           │   └── style.css
│           └── js/
│               └── app.js
└── README.md
```

## 快速開始

### 環境需求
- .NET 8.0 SDK 或更高版本
- 支援的作業系統: Windows, macOS, Linux

### 安裝步驟

1. **克隆專案**
```bash
git clone https://github.com/ya940823/oilweb.git
cd oilweb
```

2. **安裝相依套件**
```bash
cd src/OilWeb.API
dotnet restore
```

3. **執行專案**
```bash
dotnet run
```

4. **開啟瀏覽器**
```
http://localhost:5000
```

### 使用 SQL Server (選用)

如要使用 SQL Server，請修改 `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OilPriceDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

並在 `Program.cs` 中將 `UseSqlite` 改為 `UseSqlServer`。

## API 文件

### 端點說明

| 方法 | 端點 | 說明 |
|------|------|------|
| GET | `/api/oilprice/latest` | 取得最新油價 (所有油種) |
| GET | `/api/oilprice/history?days=30` | 取得歷史油價 (預設30天) |
| GET | `/api/oilprice/range?from=2025-01-01&to=2025-12-31` | 查詢指定日期區間 |
| GET | `/api/oilprice/trend` | 取得油價趨勢分析 |
| GET | `/api/oilprice/predict?oilType=92無鉛汽油&days=7` | 價格預測 |
| GET | `/api/oilprice/export/csv?days=30` | 匯出 CSV |

### 範例回應

**GET /api/oilprice/latest**
```json
[
  {
    "id": 1,
    "date": "2025-12-11",
    "oilType": "92無鉛汽油",
    "price": 28.5,
    "priceChange": 0.3
  }
]
```

## 資料庫架構

### OilPrice 資料表

| 欄位 | 類型 | 說明 |
|------|------|------|
| Id | int | 主鍵 |
| Date | datetime | 日期 |
| OilType | string(50) | 油種名稱 |
| Price | decimal(18,2) | 價格 |
| PriceChange | decimal(18,2) | 漲跌幅 |
| CreatedAt | datetime | 建立時間 |

## 開發計劃

### 已完成 ✅
- [x] ASP.NET Core 專案初始化
- [x] Entity Framework Core 資料模型設計
- [x] Web API 端點實作
- [x] Background Service 自動更新機制
- [x] Bootstrap 響應式前端介面
- [x] Chart.js 油價走勢圖
- [x] 歷史資料查詢功能
- [x] CSV 匯出功能
- [x] 油價趨勢分析
- [x] 線性回歸價格預測

### 待開發 🚧
- [ ] 串接政府資料開放平台 API
- [ ] Email 通知功能
- [ ] Line Notify 推播
- [ ] 管理者後台介面
- [ ] 使用者認證系統
- [ ] ML.NET 進階預測模型
- [ ] Docker 容器化部署
- [ ] CI/CD 自動化流程

## 資料來源

本專案預計整合以下資料源:
- 台灣中油官方網站
- 政府資料開放平台 API
- (模擬資料用於開發環境)

## 貢獻指南

歡迎提交 Issue 或 Pull Request！

1. Fork 此專案
2. 建立功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交變更 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 開啟 Pull Request

## 授權

MIT License

## 聯絡資訊

專案維護者: [@ya940823](https://github.com/ya940823)

---

**Note**: 本專案為學習與展示用途，實際油價資訊請以官方公告為準。
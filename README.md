# OilWeb - ASP.NET Core Web API

這是一個基於 ASP.NET Core 的 Web API 專案範本。

## 專案結構

```
OilWeb/
├── Controllers/        # API 控制器
│   └── HealthController.cs
├── Models/            # 資料模型
│   └── ApiResponse.cs
├── Services/          # 業務邏輯服務
├── Properties/        # 專案屬性和啟動設定
├── Program.cs         # 應用程式進入點
├── appsettings.json   # 應用程式設定
└── OilWeb.csproj     # 專案檔案
```

## 技術規格

- **.NET Version**: .NET 10.0
- **Framework**: ASP.NET Core Web API
- **Features**:
  - RESTful API 架構
  - CORS 支援
  - OpenAPI/Swagger 文件
  - 結構化日誌記錄
  - 健康檢查端點

## 開始使用

### 前置需求

- .NET 10.0 SDK 或更高版本

### 建置專案

```bash
dotnet build
```

### 執行專案

```bash
dotnet run
```

應用程式將在以下地址啟動：
- HTTP: http://localhost:5219
- HTTPS: https://localhost:7034

### 測試 API

健康檢查端點：
```bash
curl http://localhost:5219/api/health
curl http://localhost:5219/api/health/ping
```

天氣預報範例端點：
```bash
curl http://localhost:5219/weatherforecast
```

## API 端點

### Health Check
- `GET /api/health` - 取得服務健康狀態
- `GET /api/health/ping` - 簡單的 ping/pong 測試

### Weather Forecast (範例)
- `GET /weatherforecast` - 取得天氣預報資料

## 開發指南

### 新增控制器

在 `Controllers/` 目錄中建立新的控制器類別，繼承自 `ControllerBase`。

### 新增模型

在 `Models/` 目錄中建立資料模型類別。

### 新增服務

在 `Services/` 目錄中建立業務邏輯服務，並在 `Program.cs` 中註冊。

## 設定

應用程式設定位於 `appsettings.json` 和 `appsettings.Development.json` 檔案中。

## License

MIT License
# 故障排除指南

## 常見問題

### 1. 圖表無法顯示

**症狀**：圖表區域為空白，沒有任何線條或資料

**可能原因**：
- 資料庫是空的
- 瀏覽器阻擋外部 CDN
- JavaScript 錯誤

**解決方案**：
1. 檢查資料庫是否有資料：
   ```bash
   cd OilPriceAPI
   sqlite3 oilprice.db "SELECT COUNT(*) FROM OilPrices;"
   ```

2. 如果計數為 0，新增範例資料：
   ```bash
   # 參考 SAMPLE_DATA.md 檔案中的 SQL 腳本
   sqlite3 oilprice.db < sample_data.sql
   ```

3. 重新載入頁面

### 2. 價格顯示為 "--"

**症狀**：油價卡片顯示 "--" 而不是實際價格

**原因**：資料庫中沒有資料

**解決方案**：
1. 新增範例資料（參考 SAMPLE_DATA.md）
2. 或等待背景服務從政府 API 抓取資料（每天 12:00）
3. 或手動觸發資料更新：
   ```bash
   curl -X POST "http://localhost:5000/api/oilprices/refresh?days=90"
   ```

### 3. API 連線失敗

**症狀**：Console 顯示 "Resource temporarily unavailable" 錯誤

**原因**：無法連接到政府開放資料平台 API

**這是正常的**：
- 在某些環境中（如沙盒、防火牆後），外部 API 可能無法訪問
- 系統設計為可以使用本地資料庫運作
- 新增範例資料即可正常使用所有功能

### 4. 統計資訊顯示 "--"

**症狀**：統計區域的平均、最高、最低價格顯示 "--"

**原因**：指定天數範圍內沒有資料

**解決方案**：
1. 確保資料庫有足夠的歷史資料
2. 減少查詢天數（例如從 90 天改為 30 天）
3. 新增更多歷史資料

### 5. 預測線沒有顯示

**症狀**：只看到實線，沒有虛線預測

**原因**：歷史資料不足（需要至少 2 筆資料點）

**解決方案**：
1. 確保資料庫有至少 2 筆不同日期的資料
2. 建議至少 4-5 筆資料以獲得更準確的預測
3. 使用 SAMPLE_DATA.md 中的完整範例資料集（36 筆記錄）

## 驗證系統運作

### 快速檢查清單

1. **檢查應用程式運作**：
   ```bash
   curl http://localhost:5000/
   ```
   應該返回 HTML 內容

2. **檢查 API 端點**：
   ```bash
   curl http://localhost:5000/api/oilprices/latest
   ```
   應該返回 JSON 格式的油價資料

3. **檢查資料庫**：
   ```bash
   sqlite3 oilprice.db "SELECT COUNT(*) FROM OilPrices;"
   ```
   應該返回大於 0 的數字

4. **檢查預測端點**：
   ```bash
   curl "http://localhost:5000/api/oilprices/history-with-predictions?days=30"
   ```
   應該返回包含 `isPrediction: true` 的項目

### 完整測試流程

```bash
# 1. 啟動應用程式
cd OilPriceAPI
dotnet run

# 2. 在另一個終端測試 API
curl http://localhost:5000/api/oilprices/latest

# 3. 在瀏覽器開啟
# http://localhost:5000

# 4. 驗證功能
# - 檢查油價卡片顯示數字
# - 檢查圖表顯示線條
# - 點擊 7天/30天/90天 按鈕
# - 檢查統計資訊
```

## 開發環境設定

### 初次設定

```bash
# 1. 還原 NuGet 套件
dotnet restore

# 2. 建置專案
dotnet build

# 3. 執行應用程式
dotnet run

# 4. 新增範例資料（在另一個終端）
cd OilPriceAPI
sqlite3 oilprice.db < sample_data.sql

# 5. 開啟瀏覽器測試
# http://localhost:5000
```

### 重置環境

```bash
# 刪除資料庫
rm oilprice.db

# 清理建置檔案
dotnet clean

# 重新建置
dotnet build

# 啟動（會自動建立新資料庫）
dotnet run

# 新增範例資料
sqlite3 oilprice.db < sample_data.sql
```

## 生產環境部署

### 檢查清單

- [ ] 更新 `appsettings.json` 中的 API 金鑰
- [ ] 設定正確的資料庫連接字串
- [ ] 確保資料庫目錄有寫入權限
- [ ] 配置 CORS 設定（如需要）
- [ ] 設定適當的 HTTPS 憑證
- [ ] 測試背景服務是否正常運作
- [ ] 驗證資料備份策略

## 取得協助

如果問題仍然存在：

1. 檢查應用程式日誌
2. 檢查瀏覽器 Console 錯誤訊息
3. 驗證所有相依套件已正確安裝
4. 確認 .NET 10.0 SDK 已安裝
5. 查看 README.md 和 SAMPLE_DATA.md

## 已知限制

- 預測準確度依賴歷史資料品質和數量
- 線性迴歸適用於短期預測（30天）
- 極端市場變化可能影響預測準確性
- 背景服務需要網路連線到政府 API

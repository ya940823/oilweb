# 新增範例資料

如果資料庫是空的（例如首次執行或無法連接到政府 API），圖表將無法顯示。您可以使用以下 SQL 腳本新增範例資料以測試系統功能。

## 使用 SQLite 新增範例資料

```bash
cd OilPriceAPI
sqlite3 oilprice.db < sample_data.sql
```

或者直接執行：

```bash
cd OilPriceAPI
sqlite3 oilprice.db << 'EOF'
-- 新增範例油價資料（2025年10月至12月）
INSERT INTO OilPrices (Date, Company, FuelType, Price, CreatedAt) VALUES 
-- 2025年10月
('2025-10-14', '中油', '92無鉛汽油', '28.5', datetime('now')),
('2025-10-14', '中油', '95無鉛汽油', '30.0', datetime('now')),
('2025-10-14', '中油', '98無鉛汽油', '32.0', datetime('now')),
('2025-10-14', '中油', '超級柴油', '26.5', datetime('now')),
('2025-10-21', '中油', '92無鉛汽油', '28.7', datetime('now')),
('2025-10-21', '中油', '95無鉛汽油', '30.2', datetime('now')),
('2025-10-21', '中油', '98無鉛汽油', '32.2', datetime('now')),
('2025-10-21', '中油', '超級柴油', '26.7', datetime('now')),
('2025-10-28', '中油', '92無鉛汽油', '28.9', datetime('now')),
('2025-10-28', '中油', '95無鉛汽油', '30.4', datetime('now')),
('2025-10-28', '中油', '98無鉛汽油', '32.4', datetime('now')),
('2025-10-28', '中油', '超級柴油', '26.9', datetime('now')),
-- 2025年11月
('2025-11-04', '中油', '92無鉛汽油', '29.1', datetime('now')),
('2025-11-04', '中油', '95無鉛汽油', '30.6', datetime('now')),
('2025-11-04', '中油', '98無鉛汽油', '32.6', datetime('now')),
('2025-11-04', '中油', '超級柴油', '27.1', datetime('now')),
('2025-11-11', '中油', '92無鉛汽油', '29.5', datetime('now')),
('2025-11-11', '中油', '95無鉛汽油', '31.0', datetime('now')),
('2025-11-11', '中油', '98無鉛汽油', '33.0', datetime('now')),
('2025-11-11', '中油', '超級柴油', '27.5', datetime('now')),
('2025-11-18', '中油', '92無鉛汽油', '29.7', datetime('now')),
('2025-11-18', '中油', '95無鉛汽油', '31.2', datetime('now')),
('2025-11-18', '中油', '98無鉛汽油', '33.2', datetime('now')),
('2025-11-18', '中油', '超級柴油', '27.7', datetime('now')),
('2025-11-25', '中油', '92無鉛汽油', '29.9', datetime('now')),
('2025-11-25', '中油', '95無鉛汽油', '31.4', datetime('now')),
('2025-11-25', '中油', '98無鉛汽油', '33.4', datetime('now')),
('2025-11-25', '中油', '超級柴油', '27.9', datetime('now')),
-- 2025年12月
('2025-12-02', '中油', '92無鉛汽油', '30.1', datetime('now')),
('2025-12-02', '中油', '95無鉛汽油', '31.6', datetime('now')),
('2025-12-02', '中油', '98無鉛汽油', '33.6', datetime('now')),
('2025-12-02', '中油', '超級柴油', '28.1', datetime('now')),
('2025-12-09', '中油', '92無鉛汽油', '30.3', datetime('now')),
('2025-12-09', '中油', '95無鉛汽油', '31.8', datetime('now')),
('2025-12-09', '中油', '98無鉛汽油', '33.8', datetime('now')),
('2025-12-09', '中油', '超級柴油', '28.3', datetime('now'));

SELECT COUNT(*) as '總記錄數' FROM OilPrices;
SELECT '範例資料已成功新增！' as '狀態';
EOF
```

## 驗證資料

新增資料後，您可以驗證：

```bash
# 檢查記錄數
sqlite3 oilprice.db "SELECT COUNT(*) FROM OilPrices;"

# 查看日期範圍
sqlite3 oilprice.db "SELECT MIN(Date), MAX(Date) FROM OilPrices;"

# 查看最新油價
sqlite3 oilprice.db "SELECT * FROM OilPrices WHERE Date = (SELECT MAX(Date) FROM OilPrices);"
```

## 測試 API

資料新增後，您可以測試 API 端點：

```bash
# 取得最新油價
curl http://localhost:5000/api/oilprices/latest

# 取得歷史資料含預測
curl "http://localhost:5000/api/oilprices/history-with-predictions?days=30"

# 取得統計資訊
curl "http://localhost:5000/api/oilprices/statistics?days=30"
```

## 重置資料

如需重置資料庫：

```bash
cd OilPriceAPI
rm oilprice.db
dotnet run  # 將自動重新建立資料庫
```

然後再次執行上述 SQL 腳本新增範例資料。

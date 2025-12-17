#!/bin/bash
# 新增範例油價資料到資料庫
# Add sample oil price data to the database

echo "正在新增範例資料到資料庫..."
echo "Adding sample data to database..."

sqlite3 oilprice.db << 'EOF'
INSERT OR IGNORE INTO OilPrices (Date, Company, FuelType, Price, CreatedAt) VALUES 
('2025-10-14', '中油', '92無鉛汽油', 28.5, datetime('now')),
('2025-10-14', '中油', '95無鉛汽油', 30.0, datetime('now')),
('2025-10-14', '中油', '98無鉛汽油', 32.0, datetime('now')),
('2025-10-14', '中油', '超級柴油', 26.5, datetime('now')),
('2025-10-21', '中油', '92無鉛汽油', 28.7, datetime('now')),
('2025-10-21', '中油', '95無鉛汽油', 30.2, datetime('now')),
('2025-10-21', '中油', '98無鉛汽油', 32.2, datetime('now')),
('2025-10-21', '中油', '超級柴油', 26.7, datetime('now')),
('2025-10-28', '中油', '92無鉛汽油', 28.9, datetime('now')),
('2025-10-28', '中油', '95無鉛汽油', 30.4, datetime('now')),
('2025-10-28', '中油', '98無鉛汽油', 32.4, datetime('now')),
('2025-10-28', '中油', '超級柴油', 26.9, datetime('now')),
('2025-11-04', '中油', '92無鉛汽油', 29.1, datetime('now')),
('2025-11-04', '中油', '95無鉛汽油', 30.6, datetime('now')),
('2025-11-04', '中油', '98無鉛汽油', 32.6, datetime('now')),
('2025-11-04', '中油', '超級柴油', 27.1, datetime('now')),
('2025-11-11', '中油', '92無鉛汽油', 29.5, datetime('now')),
('2025-11-11', '中油', '95無鉛汽油', 31.0, datetime('now')),
('2025-11-11', '中油', '98無鉛汽油', 33.0, datetime('now')),
('2025-11-11', '中油', '超級柴油', 27.5, datetime('now')),
('2025-11-18', '中油', '92無鉛汽油', 29.7, datetime('now')),
('2025-11-18', '中油', '95無鉛汽油', 31.2, datetime('now')),
('2025-11-18', '中油', '98無鉛汽油', 33.2, datetime('now')),
('2025-11-18', '中油', '超級柴油', 27.7, datetime('now')),
('2025-11-25', '中油', '92無鉛汽油', 29.9, datetime('now')),
('2025-11-25', '中油', '95無鉛汽油', 31.4, datetime('now')),
('2025-11-25', '中油', '98無鉛汽油', 33.4, datetime('now')),
('2025-11-25', '中油', '超級柴油', 27.9, datetime('now')),
('2025-12-02', '中油', '92無鉛汽油', 30.1, datetime('now')),
('2025-12-02', '中油', '95無鉛汽油', 31.6, datetime('now')),
('2025-12-02', '中油', '98無鉛汽油', 33.6, datetime('now')),
('2025-12-02', '中油', '超級柴油', 28.1, datetime('now')),
('2025-12-09', '中油', '92無鉛汽油', 30.3, datetime('now')),
('2025-12-09', '中油', '95無鉛汽油', 31.8, datetime('now')),
('2025-12-09', '中油', '98無鉛汽油', 33.8, datetime('now')),
('2025-12-09', '中油', '超級柴油', 28.3, datetime('now'));

SELECT COUNT(*) as '總記錄數' FROM OilPrices;
EOF

echo "✅ 範例資料已成功新增！"
echo "✅ Sample data added successfully!"
echo ""
echo "現在可以重新載入網頁查看圖表"
echo "You can now reload the webpage to see the chart"

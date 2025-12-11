// 台灣油價查詢平台 JavaScript

const API_BASE_URL = window.location.origin + '/api/oilprice';
let priceChart = null;

// 頁面載入時初始化
document.addEventListener('DOMContentLoaded', function() {
    loadLatestPrices();
    loadChart(30); // 預設顯示30天
    loadTrendAnalysis();
    
    // 設定日期欄位預設值
    const today = new Date().toISOString().split('T')[0];
    const lastMonth = new Date();
    lastMonth.setMonth(lastMonth.getMonth() - 1);
    document.getElementById('toDate').value = today;
    document.getElementById('fromDate').value = lastMonth.toISOString().split('T')[0];
    
    // 查詢表單事件
    document.getElementById('queryForm').addEventListener('submit', handleQuery);
});

// 載入最新油價
async function loadLatestPrices() {
    try {
        const response = await fetch(`${API_BASE_URL}/latest`);
        const prices = await response.json();
        
        const container = document.getElementById('latestPrices');
        container.innerHTML = prices.map(price => `
            <div class="col-md-3 col-sm-6 mb-3 fade-in">
                <div class="card price-card">
                    <div class="card-body text-center">
                        <h5 class="card-title">${price.oilType}</h5>
                        <div class="price-value">$${price.price}</div>
                        <div class="price-change ${getPriceClass(price.priceChange)}">
                            ${getPriceChangeText(price.priceChange)}
                        </div>
                        <small class="text-muted">更新日期: ${formatDate(price.date)}</small>
                    </div>
                </div>
            </div>
        `).join('');
    } catch (error) {
        console.error('載入最新油價失敗:', error);
        document.getElementById('latestPrices').innerHTML = 
            '<div class="col-12 alert alert-danger">載入油價資料失敗</div>';
    }
}

// 載入走勢圖
async function loadChart(days) {
    try {
        const response = await fetch(`${API_BASE_URL}/history?days=${days}`);
        const data = await response.json();
        
        // 按日期分組
        const groupedByDate = {};
        data.forEach(item => {
            const date = formatDate(item.date);
            if (!groupedByDate[date]) {
                groupedByDate[date] = {};
            }
            groupedByDate[date][item.oilType] = item.price;
        });
        
        const dates = Object.keys(groupedByDate).sort();
        const oilTypes = ['92無鉛汽油', '95無鉛汽油', '98無鉛汽油', '超級柴油'];
        const colors = ['#0d6efd', '#6610f2', '#6f42c1', '#20c997'];
        
        const datasets = oilTypes.map((type, index) => ({
            label: type,
            data: dates.map(date => groupedByDate[date][type]),
            borderColor: colors[index],
            backgroundColor: colors[index] + '20',
            tension: 0.1,
            fill: false
        }));
        
        const ctx = document.getElementById('priceChart').getContext('2d');
        
        if (priceChart) {
            priceChart.destroy();
        }
        
        priceChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: dates,
                datasets: datasets
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    title: {
                        display: true,
                        text: `油價走勢 - 最近 ${days} 天`,
                        font: { size: 16 }
                    },
                    legend: {
                        position: 'bottom'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: false,
                        title: {
                            display: true,
                            text: '價格 (元/公升)'
                        }
                    }
                }
            }
        });
        
        // 更新按鈕狀態
        document.querySelectorAll('.btn-group .btn').forEach(btn => {
            btn.classList.remove('active');
        });
        event.target.classList.add('active');
    } catch (error) {
        console.error('載入走勢圖失敗:', error);
    }
}

// 處理查詢表單
async function handleQuery(event) {
    event.preventDefault();
    
    const fromDate = document.getElementById('fromDate').value;
    const toDate = document.getElementById('toDate').value;
    
    try {
        const response = await fetch(`${API_BASE_URL}/range?from=${fromDate}&to=${toDate}`);
        const data = await response.json();
        
        if (data.length === 0) {
            document.getElementById('queryResults').innerHTML = 
                '<div class="alert alert-info">查無資料</div>';
            return;
        }
        
        const table = `
            <div class="table-responsive">
                <table class="table table-striped table-hover">
                    <thead class="table-primary">
                        <tr>
                            <th>日期</th>
                            <th>油種</th>
                            <th>價格</th>
                            <th>漲跌幅</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${data.map(item => `
                            <tr>
                                <td>${formatDate(item.date)}</td>
                                <td>${item.oilType}</td>
                                <td>$${item.price}</td>
                                <td class="${getPriceClass(item.priceChange)}">
                                    ${getPriceChangeText(item.priceChange)}
                                </td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        `;
        
        document.getElementById('queryResults').innerHTML = table;
    } catch (error) {
        console.error('查詢失敗:', error);
        document.getElementById('queryResults').innerHTML = 
            '<div class="alert alert-danger">查詢失敗</div>';
    }
}

// 載入趨勢分析
async function loadTrendAnalysis() {
    try {
        const response = await fetch(`${API_BASE_URL}/trend`);
        const trends = await response.json();
        
        const html = `
            <div class="row">
                ${trends.map(trend => `
                    <div class="col-md-6 mb-3">
                        <div class="card">
                            <div class="card-body">
                                <h5 class="card-title">${trend.oilType}</h5>
                                <div class="row">
                                    <div class="col-6">
                                        <p><strong>當前價格:</strong> $${trend.currentPrice?.toFixed(2)}</p>
                                        <p><strong>平均價格:</strong> $${trend.averagePrice?.toFixed(2)}</p>
                                        <p><strong>最高價:</strong> $${trend.maxPrice?.toFixed(2)}</p>
                                    </div>
                                    <div class="col-6">
                                        <p><strong>最低價:</strong> $${trend.minPrice?.toFixed(2)}</p>
                                        <p><strong>總變動:</strong> 
                                            <span class="${getPriceClass(trend.totalChange)}">
                                                ${trend.totalChange > 0 ? '+' : ''}${trend.totalChange?.toFixed(2)}
                                            </span>
                                        </p>
                                        <p><strong>資料筆數:</strong> ${trend.totalRecords}</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                `).join('')}
            </div>
        `;
        
        document.getElementById('trendAnalysis').innerHTML = html;
    } catch (error) {
        console.error('載入趨勢分析失敗:', error);
    }
}

// 載入價格預測
async function loadPrediction() {
    const oilType = document.getElementById('oilTypeSelect').value;
    
    try {
        const response = await fetch(`${API_BASE_URL}/predict?oilType=${encodeURIComponent(oilType)}&days=4`);
        const prediction = await response.json();
        
        const html = `
            <div class="alert alert-info">
                <h5>${prediction.oilType}</h5>
                <p><strong>當前價格:</strong> $${prediction.currentPrice}</p>
                <p><strong>趨勢:</strong> ${prediction.trend}</p>
            </div>
            <div class="table-responsive">
                <table class="table table-bordered">
                    <thead class="table-light">
                        <tr>
                            <th>預測日期</th>
                            <th>預測價格</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${prediction.predictions.map(pred => `
                            <tr>
                                <td>${formatDate(pred.date)}</td>
                                <td>$${pred.predictedPrice}</td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
            <small class="text-muted">※ 預測結果僅供參考，採用簡單線性回歸模型計算</small>
        `;
        
        document.getElementById('predictionResults').innerHTML = html;
    } catch (error) {
        console.error('載入預測失敗:', error);
        document.getElementById('predictionResults').innerHTML = 
            '<div class="alert alert-danger">預測失敗</div>';
    }
}

// 匯出 CSV
function exportCsv() {
    const days = 30;
    window.open(`${API_BASE_URL}/export/csv?days=${days}`, '_blank');
}

// 輔助函數
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('zh-TW', { year: 'numeric', month: '2-digit', day: '2-digit' });
}

function getPriceClass(change) {
    if (!change) return 'price-neutral';
    return change > 0 ? 'price-up' : change < 0 ? 'price-down' : 'price-neutral';
}

function getPriceChangeText(change) {
    if (!change || change === 0) return '持平 (0.0)';
    const sign = change > 0 ? '↑' : '↓';
    return `${sign} ${Math.abs(change).toFixed(1)}`;
}

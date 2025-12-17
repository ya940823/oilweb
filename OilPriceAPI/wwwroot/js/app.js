const API_BASE_URL = '/api/oilprices';
let priceChart = null;

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    loadLatestPrices();
    loadStatistics();
    loadChartData(30);
    loadHistoryTable();
});

// Upload XML file
async function uploadXmlFile() {
    const fileInput = document.getElementById('xmlFileInput');
    const file = fileInput.files[0];
    const resultDiv = document.getElementById('uploadResult');
    const spinner = document.getElementById('uploadSpinner');
    
    if (!file) {
        showAlert(resultDiv, 'danger', '請選擇一個 XML 檔案');
        return;
    }
    
    const formData = new FormData();
    formData.append('file', file);
    
    spinner.classList.remove('d-none');
    
    try {
        const response = await fetch(`${API_BASE_URL}/upload-xml`, {
            method: 'POST',
            body: formData
        });
        
        const data = await response.json();
        
        if (response.ok) {
            showAlert(resultDiv, 'success', 
                `✅ ${data.message}<br>總記錄數: ${data.totalRecords}<br>新增記錄: ${data.savedRecords}`);
            
            // Reload all data
            setTimeout(() => {
                loadLatestPrices();
                loadStatistics();
                loadChartData(30);
                loadHistoryTable();
            }, 1000);
        } else {
            showAlert(resultDiv, 'danger', `❌ 錯誤: ${data.error || '上傳失敗'}`);
        }
    } catch (error) {
        showAlert(resultDiv, 'danger', `❌ 網路錯誤: ${error.message}`);
    } finally {
        spinner.classList.add('d-none');
    }
}

// Load latest prices
async function loadLatestPrices() {
    try {
        const response = await fetch(`${API_BASE_URL}/latest`);
        
        if (!response.ok) {
            throw new Error('無法載入最新油價');
        }
        
        const data = await response.json();
        displayLatestPrices(data);
    } catch (error) {
        document.getElementById('latestPrices').innerHTML = 
            `<div class="col-12"><div class="alert alert-warning">無法載入最新油價資料</div></div>`;
    }
}

// Display latest prices
function displayLatestPrices(data) {
    const container = document.getElementById('latestPrices');
    const date = new Date(data.date).toLocaleDateString('zh-TW');
    
    container.innerHTML = `
        <div class="col-md-3 mb-3">
            <div class="price-card" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);">
                <h6>92 無鉛汽油</h6>
                <h3>$${data.oil92.toFixed(1)}</h3>
            </div>
        </div>
        <div class="col-md-3 mb-3">
            <div class="price-card" style="background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);">
                <h6>95 無鉛汽油</h6>
                <h3>$${data.oil95.toFixed(1)}</h3>
            </div>
        </div>
        <div class="col-md-3 mb-3">
            <div class="price-card" style="background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);">
                <h6>98 無鉛汽油</h6>
                <h3>$${data.oil98.toFixed(1)}</h3>
            </div>
        </div>
        <div class="col-md-3 mb-3">
            <div class="price-card" style="background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);">
                <h6>超級柴油</h6>
                <h3>$${data.diesel.toFixed(1)}</h3>
            </div>
        </div>
        <div class="col-12 mt-2">
            <p class="text-muted text-center mb-0"><small>更新日期: ${date}</small></p>
        </div>
    `;
}

// Load statistics
async function loadStatistics() {
    try {
        const response = await fetch(`${API_BASE_URL}/statistics?days=30`);
        
        if (!response.ok) {
            throw new Error('無法載入統計資料');
        }
        
        const data = await response.json();
        displayStatistics(data);
    } catch (error) {
        document.getElementById('statistics').innerHTML = 
            `<div class="alert alert-warning">無法載入統計資料</div>`;
    }
}

// Display statistics
function displayStatistics(data) {
    const container = document.getElementById('statistics');
    
    container.innerHTML = `
        <div class="row">
            <div class="col-md-3">
                <div class="stat-item">
                    <h6>92 無鉛</h6>
                    <div class="stat-values">
                        <div>
                            <span>平均</span>
                            <strong>$${data.oil92.average.toFixed(2)}</strong>
                        </div>
                        <div>
                            <span>最高</span>
                            <strong>$${data.oil92.max.toFixed(2)}</strong>
                        </div>
                        <div>
                            <span>最低</span>
                            <strong>$${data.oil92.min.toFixed(2)}</strong>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="stat-item">
                    <h6>95 無鉛</h6>
                    <div class="stat-values">
                        <div>
                            <span>平均</span>
                            <strong>$${data.oil95.average.toFixed(2)}</strong>
                        </div>
                        <div>
                            <span>最高</span>
                            <strong>$${data.oil95.max.toFixed(2)}</strong>
                        </div>
                        <div>
                            <span>最低</span>
                            <strong>$${data.oil95.min.toFixed(2)}</strong>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="stat-item">
                    <h6>98 無鉛</h6>
                    <div class="stat-values">
                        <div>
                            <span>平均</span>
                            <strong>$${data.oil98.average.toFixed(2)}</strong>
                        </div>
                        <div>
                            <span>最高</span>
                            <strong>$${data.oil98.max.toFixed(2)}</strong>
                        </div>
                        <div>
                            <span>最低</span>
                            <strong>$${data.oil98.min.toFixed(2)}</strong>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="stat-item">
                    <h6>柴油</h6>
                    <div class="stat-values">
                        <div>
                            <span>平均</span>
                            <strong>$${data.diesel.average.toFixed(2)}</strong>
                        </div>
                        <div>
                            <span>最高</span>
                            <strong>$${data.diesel.max.toFixed(2)}</strong>
                        </div>
                        <div>
                            <span>最低</span>
                            <strong>$${data.diesel.min.toFixed(2)}</strong>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;
}

// Load chart data
async function loadChartData(days) {
    try {
        const response = await fetch(`${API_BASE_URL}/history?days=${days}`);
        
        if (!response.ok) {
            throw new Error('無法載入圖表資料');
        }
        
        const data = await response.json();
        displayChart(data);
        
        // Update button active state
        document.querySelectorAll('.btn-outline-primary').forEach(btn => {
            btn.classList.remove('active');
        });
        event.target.classList.add('active');
    } catch (error) {
        console.error('Error loading chart:', error);
    }
}

// Display chart
function displayChart(data) {
    const ctx = document.getElementById('priceChart');
    
    // Sort data by date ascending
    data.sort((a, b) => new Date(a.date) - new Date(b.date));
    
    const labels = data.map(item => new Date(item.date).toLocaleDateString('zh-TW', { month: 'short', day: 'numeric' }));
    const oil92Data = data.map(item => item.oil92);
    const oil95Data = data.map(item => item.oil95);
    const oil98Data = data.map(item => item.oil98);
    const dieselData = data.map(item => item.diesel);
    
    if (priceChart) {
        priceChart.destroy();
    }
    
    priceChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: '92 無鉛',
                    data: oil92Data,
                    borderColor: 'rgb(102, 126, 234)',
                    backgroundColor: 'rgba(102, 126, 234, 0.1)',
                    tension: 0.4
                },
                {
                    label: '95 無鉛',
                    data: oil95Data,
                    borderColor: 'rgb(245, 87, 108)',
                    backgroundColor: 'rgba(245, 87, 108, 0.1)',
                    tension: 0.4
                },
                {
                    label: '98 無鉛',
                    data: oil98Data,
                    borderColor: 'rgb(79, 172, 254)',
                    backgroundColor: 'rgba(79, 172, 254, 0.1)',
                    tension: 0.4
                },
                {
                    label: '柴油',
                    data: dieselData,
                    borderColor: 'rgb(67, 233, 123)',
                    backgroundColor: 'rgba(67, 233, 123, 0.1)',
                    tension: 0.4
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    display: true,
                    position: 'top'
                },
                title: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: false,
                    ticks: {
                        callback: function(value) {
                            return '$' + value.toFixed(1);
                        }
                    }
                }
            }
        }
    });
}

// Load history table
async function loadHistoryTable() {
    try {
        const response = await fetch(`${API_BASE_URL}/history?days=30`);
        
        if (!response.ok) {
            throw new Error('無法載入歷史資料');
        }
        
        const data = await response.json();
        displayHistoryTable(data);
    } catch (error) {
        document.getElementById('historyTable').innerHTML = 
            `<tr><td colspan="6" class="text-center text-danger">無法載入歷史資料</td></tr>`;
    }
}

// Display history table
function displayHistoryTable(data) {
    const tbody = document.getElementById('historyTable');
    
    if (data.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center">無資料</td></tr>';
        return;
    }
    
    tbody.innerHTML = data.map(item => `
        <tr>
            <td>${new Date(item.date).toLocaleDateString('zh-TW')}</td>
            <td>$${item.oil92.toFixed(2)}</td>
            <td>$${item.oil95.toFixed(2)}</td>
            <td>$${item.oil98.toFixed(2)}</td>
            <td>$${item.diesel.toFixed(2)}</td>
            <td><span class="badge bg-info">${item.source}</span></td>
        </tr>
    `).join('');
}

// Helper function to show alerts
function showAlert(container, type, message) {
    container.innerHTML = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
}

// API Base URL
const API_BASE = '/api/oilprices';

// Global chart instance
let priceChart = null;
let currentDays = 7;

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    loadLatestPrices();
    loadChart(7);
    loadStatistics(7);
});

// Load latest oil prices
async function loadLatestPrices() {
    try {
        const response = await fetch(`${API_BASE}/latest`);
        if (!response.ok) {
            throw new Error('Failed to fetch latest prices');
        }
        
        const data = await response.json();
        
        // Update date
        const date = new Date(data.date);
        document.getElementById('latestPriceDate').textContent = 
            `更新日期：${date.toLocaleDateString('zh-TW')}`;
        
        // Update prices and changes
        updatePrice('gas92', data.gas92, data.gas92Change);
        updatePrice('gas95', data.gas95, data.gas95Change);
        updatePrice('gas98', data.gas98, data.gas98Change);
        updatePrice('diesel', data.diesel, data.dieselChange);
        
    } catch (error) {
        console.error('Error loading latest prices:', error);
        alert('無法載入最新油價資料');
    }
}

// Update price display with change indicator
function updatePrice(type, price, change) {
    const priceElement = document.getElementById(`${type}Price`);
    const changeElement = document.getElementById(`${type}Change`);
    
    priceElement.textContent = `NT$ ${price.toFixed(1)}`;
    
    if (change === null || change === undefined) {
        changeElement.textContent = '';
        changeElement.className = 'change';
    } else {
        const changeValue = change.toFixed(1);
        const arrow = change > 0 ? '▲' : change < 0 ? '▼' : '━';
        const changeClass = change > 0 ? 'up' : change < 0 ? 'down' : 'neutral';
        
        changeElement.textContent = `${arrow} ${Math.abs(changeValue)}`;
        changeElement.className = `change ${changeClass}`;
    }
}

// Load chart data
async function loadChart(days) {
    try {
        currentDays = days;
        const response = await fetch(`${API_BASE}/history?days=${days}`);
        if (!response.ok) {
            throw new Error('Failed to fetch history');
        }
        
        const data = await response.json();
        
        // Prepare chart data
        const labels = data.map(item => {
            const date = new Date(item.date);
            return date.toLocaleDateString('zh-TW', { month: 'short', day: 'numeric' });
        });
        
        const gas92Data = data.map(item => item.gas92);
        const gas95Data = data.map(item => item.gas95);
        const gas98Data = data.map(item => item.gas98);
        const dieselData = data.map(item => item.diesel);
        
        // Update chart
        updateChart(labels, gas92Data, gas95Data, gas98Data, dieselData);
        
        // Update button states
        document.querySelectorAll('.btn-group .btn').forEach(btn => {
            btn.classList.remove('active');
        });
        event.target.classList.add('active');
        
        // Load statistics
        loadStatistics(days);
        
    } catch (error) {
        console.error('Error loading chart:', error);
        alert('無法載入圖表資料');
    }
}

// Update or create chart
function updateChart(labels, gas92Data, gas95Data, gas98Data, dieselData) {
    const ctx = document.getElementById('priceChart').getContext('2d');
    
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
                    data: gas92Data,
                    borderColor: 'rgb(255, 99, 132)',
                    backgroundColor: 'rgba(255, 99, 132, 0.1)',
                    tension: 0.1
                },
                {
                    label: '95 無鉛',
                    data: gas95Data,
                    borderColor: 'rgb(54, 162, 235)',
                    backgroundColor: 'rgba(54, 162, 235, 0.1)',
                    tension: 0.1
                },
                {
                    label: '98 無鉛',
                    data: gas98Data,
                    borderColor: 'rgb(255, 206, 86)',
                    backgroundColor: 'rgba(255, 206, 86, 0.1)',
                    tension: 0.1
                },
                {
                    label: '柴油',
                    data: dieselData,
                    borderColor: 'rgb(75, 192, 192)',
                    backgroundColor: 'rgba(75, 192, 192, 0.1)',
                    tension: 0.1
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    position: 'top',
                },
                title: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return context.dataset.label + ': NT$ ' + context.parsed.y.toFixed(1);
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: false,
                    ticks: {
                        callback: function(value) {
                            return 'NT$ ' + value.toFixed(1);
                        }
                    }
                }
            }
        }
    });
}

// Load statistics
async function loadStatistics(days) {
    try {
        const response = await fetch(`${API_BASE}/statistics?days=${days}`);
        if (!response.ok) {
            throw new Error('Failed to fetch statistics');
        }
        
        const data = await response.json();
        
        const html = `
            <div class="row">
                <div class="col-md-3 col-sm-6 mb-3">
                    <h5>92 無鉛</h5>
                    <p class="mb-1">平均: NT$ ${data.gas92.average.toFixed(2)}</p>
                    <p class="mb-1">最低: NT$ ${data.gas92.min.toFixed(2)}</p>
                    <p class="mb-1">最高: NT$ ${data.gas92.max.toFixed(2)}</p>
                </div>
                <div class="col-md-3 col-sm-6 mb-3">
                    <h5>95 無鉛</h5>
                    <p class="mb-1">平均: NT$ ${data.gas95.average.toFixed(2)}</p>
                    <p class="mb-1">最低: NT$ ${data.gas95.min.toFixed(2)}</p>
                    <p class="mb-1">最高: NT$ ${data.gas95.max.toFixed(2)}</p>
                </div>
                <div class="col-md-3 col-sm-6 mb-3">
                    <h5>98 無鉛</h5>
                    <p class="mb-1">平均: NT$ ${data.gas98.average.toFixed(2)}</p>
                    <p class="mb-1">最低: NT$ ${data.gas98.min.toFixed(2)}</p>
                    <p class="mb-1">最高: NT$ ${data.gas98.max.toFixed(2)}</p>
                </div>
                <div class="col-md-3 col-sm-6 mb-3">
                    <h5>柴油</h5>
                    <p class="mb-1">平均: NT$ ${data.diesel.average.toFixed(2)}</p>
                    <p class="mb-1">最低: NT$ ${data.diesel.min.toFixed(2)}</p>
                    <p class="mb-1">最高: NT$ ${data.diesel.max.toFixed(2)}</p>
                </div>
            </div>
        `;
        
        document.getElementById('statistics').innerHTML = html;
        
    } catch (error) {
        console.error('Error loading statistics:', error);
        document.getElementById('statistics').innerHTML = 
            '<div class="card-body"><p class="text-danger">無法載入統計資料</p></div>';
    }
}

// Export data to CSV
async function exportData() {
    try {
        const response = await fetch(`${API_BASE}/export?days=${currentDays}`);
        if (!response.ok) {
            throw new Error('Failed to export data');
        }
        
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `oil-prices-${currentDays}days.csv`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
        
    } catch (error) {
        console.error('Error exporting data:', error);
        alert('無法匯出資料');
    }
}

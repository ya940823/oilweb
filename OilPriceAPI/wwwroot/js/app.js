let priceChart = null;
const API_BASE = '/api/oilprices';

// Initialize app
document.addEventListener('DOMContentLoaded', function() {
    loadLatestPrices();
    loadChart(30);
    loadStatistics(30);
});

// Load latest prices
async function loadLatestPrices() {
    try {
        const response = await fetch(`${API_BASE}/latest`);
        const prices = await response.json();
        
        const container = document.getElementById('latestPrices');
        container.innerHTML = '';

        // Group by fuel type
        const fuelTypes = {};
        prices.forEach(price => {
            if (!fuelTypes[price.fuelType]) {
                fuelTypes[price.fuelType] = [];
            }
            fuelTypes[price.fuelType].push(price);
        });

        // Create cards for each fuel type
        for (const [fuelType, priceList] of Object.entries(fuelTypes)) {
            const avgPrice = (priceList.reduce((sum, p) => sum + p.price, 0) / priceList.length).toFixed(2);
            
            const card = `
                <div class="col-md-3 mb-3">
                    <div class="card price-card">
                        <div class="card-body text-center">
                            <h6 class="card-title text-muted">${fuelType}</h6>
                            <div class="price-value">$${avgPrice}</div>
                            <small class="text-muted">平均價格</small>
                        </div>
                    </div>
                </div>
            `;
            container.innerHTML += card;
        }
    } catch (error) {
        console.error('Error loading latest prices:', error);
        document.getElementById('latestPrices').innerHTML = 
            '<div class="col-12"><div class="alert alert-warning">無法載入最新油價資料</div></div>';
    }
}

// Load chart
async function loadChart(days) {
    try {
        const response = await fetch(`${API_BASE}/history?days=${days}`);
        const prices = await response.json();

        // Group by date and fuel type
        const dateMap = {};
        prices.forEach(price => {
            const dateStr = new Date(price.date).toLocaleDateString('zh-TW');
            if (!dateMap[dateStr]) {
                dateMap[dateStr] = {};
            }
            if (!dateMap[dateStr][price.fuelType]) {
                dateMap[dateStr][price.fuelType] = [];
            }
            dateMap[dateStr][price.fuelType].push(price.price);
        });

        // Calculate average for each date and fuel type
        const labels = Object.keys(dateMap).sort();
        const datasets = {};

        labels.forEach(date => {
            for (const [fuelType, priceList] of Object.entries(dateMap[date])) {
                if (!datasets[fuelType]) {
                    datasets[fuelType] = [];
                }
                const avg = priceList.reduce((sum, p) => sum + p, 0) / priceList.length;
                datasets[fuelType].push(avg.toFixed(2));
            }
        });

        // Prepare Chart.js data
        const colors = ['#0d6efd', '#198754', '#ffc107', '#dc3545'];
        const chartData = {
            labels: labels,
            datasets: Object.entries(datasets).map(([fuelType, data], index) => ({
                label: fuelType,
                data: data,
                borderColor: colors[index % colors.length],
                backgroundColor: colors[index % colors.length] + '20',
                tension: 0.1
            }))
        };

        const ctx = document.getElementById('priceChart').getContext('2d');
        
        if (priceChart) {
            priceChart.destroy();
        }

        priceChart = new Chart(ctx, {
            type: 'line',
            data: chartData,
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: {
                    legend: {
                        position: 'top',
                    },
                    title: {
                        display: true,
                        text: `近 ${days} 天油價走勢`
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

        // Update active button
        document.querySelectorAll('.card-header .btn-group button').forEach(btn => {
            btn.classList.remove('active');
        });
        event.target.classList.add('active');
    } catch (error) {
        console.error('Error loading chart:', error);
    }
}

// Load statistics
async function loadStatistics(days) {
    try {
        const response = await fetch(`${API_BASE}/statistics?days=${days}`);
        const stats = await response.json();

        const container = document.getElementById('statistics');
        container.innerHTML = '';

        stats.forEach(stat => {
            const item = `
                <div class="stat-item">
                    <div class="d-flex justify-content-between">
                        <span class="stat-label">${stat.fuelType}</span>
                    </div>
                    <div class="small">
                        平均: <span class="stat-value">$${stat.average.toFixed(2)}</span> | 
                        最高: <span class="stat-value">$${stat.max.toFixed(2)}</span> | 
                        最低: <span class="stat-value">$${stat.min.toFixed(2)}</span>
                    </div>
                </div>
            `;
            container.innerHTML += item;
        });
    } catch (error) {
        console.error('Error loading statistics:', error);
        document.getElementById('statistics').innerHTML = 
            '<div class="alert alert-warning">無法載入統計資料</div>';
    }
}

// Export data
function exportData(days) {
    window.location.href = `${API_BASE}/export?days=${days}`;
}

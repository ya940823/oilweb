const API_BASE_URL = '/api/oilprices';
let currentDays = 30;

// Initialize app on page load
document.addEventListener('DOMContentLoaded', function() {
    loadLatestPrices();
    loadChart(30);
    loadStatistics(30);
});

// Load latest oil prices
async function loadLatestPrices() {
    try {
        const response = await fetch(`${API_BASE_URL}/latest`);
        if (!response.ok) {
            console.error('Failed to load latest prices');
            return;
        }
        
        const data = await response.json();
        
        document.getElementById('price92').textContent = `$${data.price92.toFixed(1)}`;
        document.getElementById('price95').textContent = `$${data.price95.toFixed(1)}`;
        document.getElementById('price98').textContent = `$${data.price98.toFixed(1)}`;
        document.getElementById('priceDiesel').textContent = `$${data.priceDiesel.toFixed(1)}`;
        
        // Get previous day's prices for comparison
        const historyResponse = await fetch(`${API_BASE_URL}/history?days=2`);
        if (historyResponse.ok) {
            const historyData = await historyResponse.json();
            if (historyData.length >= 2) {
                const previous = historyData[historyData.length - 2];
                const current = historyData[historyData.length - 1];
                
                updatePriceChange('change92', previous.price92, current.price92);
                updatePriceChange('change95', previous.price95, current.price95);
                updatePriceChange('change98', previous.price98, current.price98);
                updatePriceChange('changeDiesel', previous.priceDiesel, current.priceDiesel);
            }
        }
    } catch (error) {
        console.error('Error loading latest prices:', error);
    }
}

function updatePriceChange(elementId, previousPrice, currentPrice) {
    const element = document.getElementById(elementId);
    const change = currentPrice - previousPrice;
    
    if (change > 0) {
        element.textContent = `▲ $${change.toFixed(1)}`;
        element.className = 'card-text small price-up';
    } else if (change < 0) {
        element.textContent = `▼ $${Math.abs(change).toFixed(1)}`;
        element.className = 'card-text small price-down';
    } else {
        element.textContent = '─ 持平';
        element.className = 'card-text small';
    }
}

// Load chart with historical data and predictions
async function loadChart(days) {
    currentDays = days;
    
    try {
        const response = await fetch(`${API_BASE_URL}/history-with-predictions?days=${days}`);
        if (!response.ok) {
            console.error('Failed to load chart data');
            return;
        }
        
        const data = await response.json();
        
        // Draw chart using canvas
        drawChart(data);
        
        // Update button states
        document.querySelectorAll('.btn-group button').forEach(btn => {
            btn.classList.remove('active');
            if (btn.textContent.includes(days + '天')) {
                btn.classList.add('active');
            }
        });
        
    } catch (error) {
        console.error('Error loading chart:', error);
    }
}

function drawChart(data) {
    const canvas = document.getElementById('priceChart');
    const ctx = canvas.getContext('2d');
    
    // Clear canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    
    // Chart dimensions
    const padding = 60;
    const chartWidth = canvas.width - padding * 2;
    const chartHeight = canvas.height - padding * 2;
    
    // Find min/max values
    const allPrices = data.flatMap(d => [d.price92, d.price95, d.price98, d.priceDiesel]);
    const minPrice = Math.floor(Math.min(...allPrices) - 1);
    const maxPrice = Math.ceil(Math.max(...allPrices) + 1);
    
    // Helper functions
    function getX(index) {
        return padding + (index / (data.length - 1)) * chartWidth;
    }
    
    function getY(price) {
        return padding + chartHeight - ((price - minPrice) / (maxPrice - minPrice)) * chartHeight;
    }
    
    // Draw axes
    ctx.strokeStyle = '#333';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(padding, padding);
    ctx.lineTo(padding, padding + chartHeight);
    ctx.lineTo(padding + chartWidth, padding + chartHeight);
    ctx.stroke();
    
    // Draw grid lines
    ctx.strokeStyle = '#ddd';
    ctx.lineWidth = 1;
    for (let i = 0; i <= 5; i++) {
        const y = padding + (chartHeight / 5) * i;
        ctx.beginPath();
        ctx.moveTo(padding, y);
        ctx.lineTo(padding + chartWidth, y);
        ctx.stroke();
        
        // Y-axis labels
        const price = maxPrice - ((maxPrice - minPrice) / 5) * i;
        ctx.fillStyle = '#666';
        ctx.font = '12px Arial';
        ctx.textAlign = 'right';
        ctx.fillText('$' + price.toFixed(1), padding - 10, y + 4);
    }
    
    // Draw title
    ctx.fillStyle = '#333';
    ctx.font = 'bold 16px Arial';
    ctx.textAlign = 'center';
    ctx.fillText(`油價走勢圖（最近${currentDays}天 + 未來30天預測）`, canvas.width / 2, 30);
    
    // Find index where predictions start
    const firstPredictionIndex = data.findIndex(d => d.isPrediction);
    
    // Draw lines for each fuel type
    const fuelTypes = [
        { key: 'price92', color: '#0d6efd', label: '92無鉛' },
        { key: 'price95', color: '#198754', label: '95無鉛' },
        { key: 'price98', color: '#ffc107', label: '98無鉛' },
        { key: 'priceDiesel', color: '#dc3545', label: '超級柴油' }
    ];
    
    fuelTypes.forEach(fuel => {
        // Draw historical data (solid line)
        ctx.strokeStyle = fuel.color;
        ctx.lineWidth = 2;
        ctx.setLineDash([]);
        ctx.beginPath();
        
        for (let i = 0; i < Math.min(firstPredictionIndex + 1, data.length); i++) {
            const x = getX(i);
            const y = getY(data[i][fuel.key]);
            if (i === 0) {
                ctx.moveTo(x, y);
            } else {
                ctx.lineTo(x, y);
            }
        }
        ctx.stroke();
        
        // Draw prediction data (dashed line)
        if (firstPredictionIndex > 0) {
            ctx.setLineDash([5, 5]);
            ctx.beginPath();
            ctx.moveTo(getX(firstPredictionIndex - 1), getY(data[firstPredictionIndex - 1][fuel.key]));
            
            for (let i = firstPredictionIndex; i < data.length; i++) {
                const x = getX(i);
                const y = getY(data[i][fuel.key]);
                ctx.lineTo(x, y);
            }
            ctx.stroke();
        }
    });
    
    // Draw legend
    ctx.setLineDash([]);
    let legendX = padding + 20;
    const legendY = padding + 20;
    
    fuelTypes.forEach((fuel, index) => {
        ctx.fillStyle = fuel.color;
        ctx.fillRect(legendX, legendY + index * 20, 15, 3);
        ctx.fillStyle = '#333';
        ctx.font = '12px Arial';
        ctx.textAlign = 'left';
        ctx.fillText(fuel.label, legendX + 20, legendY + index * 20 + 4);
    });
    
    // Draw X-axis labels (dates)
    ctx.fillStyle = '#666';
    ctx.font = '10px Arial';
    ctx.textAlign = 'center';
    const labelStep = Math.ceil(data.length / 10); // Show ~10 labels
    data.forEach((d, i) => {
        if (i % labelStep === 0) {
            const date = new Date(d.date);
            const label = (date.getMonth() + 1) + '/' + date.getDate();
            ctx.fillText(label, getX(i), padding + chartHeight + 20);
        }
    });
    
    // Add prediction marker
    if (firstPredictionIndex > 0 && firstPredictionIndex < data.length) {
        ctx.strokeStyle = '#ff6b6b';
        ctx.lineWidth = 2;
        ctx.setLineDash([10, 5]);
        const x = getX(firstPredictionIndex - 0.5);
        ctx.beginPath();
        ctx.moveTo(x, padding);
        ctx.lineTo(x, padding + chartHeight);
        ctx.stroke();
        
        ctx.fillStyle = '#ff6b6b';
        ctx.font = 'bold 12px Arial';
        ctx.textAlign = 'center';
        ctx.fillText('↓ 預測開始', x, padding - 10);
    }
}

// Load statistics
async function loadStatistics(days) {
    try {
        const response = await fetch(`${API_BASE_URL}/statistics?days=${days}`);
        if (!response.ok) {
            console.error('Failed to load statistics');
            return;
        }
        
        const data = await response.json();
        
        document.getElementById('stat92Avg').textContent = `$${data.price92.average.toFixed(2)}`;
        document.getElementById('stat92Max').textContent = `$${data.price92.max.toFixed(1)}`;
        document.getElementById('stat92Min').textContent = `$${data.price92.min.toFixed(1)}`;
        
        document.getElementById('stat95Avg').textContent = `$${data.price95.average.toFixed(2)}`;
        document.getElementById('stat95Max').textContent = `$${data.price95.max.toFixed(1)}`;
        document.getElementById('stat95Min').textContent = `$${data.price95.min.toFixed(1)}`;
        
        document.getElementById('stat98Avg').textContent = `$${data.price98.average.toFixed(2)}`;
        document.getElementById('stat98Max').textContent = `$${data.price98.max.toFixed(1)}`;
        document.getElementById('stat98Min').textContent = `$${data.price98.min.toFixed(1)}`;
        
        document.getElementById('statDieselAvg').textContent = `$${data.diesel.average.toFixed(2)}`;
        document.getElementById('statDieselMax').textContent = `$${data.diesel.max.toFixed(1)}`;
        document.getElementById('statDieselMin').textContent = `$${data.diesel.min.toFixed(1)}`;
        
    } catch (error) {
        console.error('Error loading statistics:', error);
    }
}

// Export data to CSV
function exportData(days) {
    window.location.href = `${API_BASE_URL}/export?days=${days}`;
}

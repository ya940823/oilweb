const API_BASE_URL = '/api/oilprices';
let currentDays = 30;
let allChartData = []; // Store all data for filtering
let currentWeeksFilter = 0; // 0 means show all data

// Initialize app on page load
document.addEventListener('DOMContentLoaded', function() {
    loadLatestPrices();
    loadChart(0); // 0 means load all historical data
    loadStatistics(0); // 0 means calculate statistics from all historical data
    load10LiterComparison(); // Load 10 liter price comparison
});

// Seed sample data function
async function seedSampleData() {
    const button = event.target;
    const originalText = button.innerHTML;
    button.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> 新增中...';
    button.disabled = true;
    
    try {
        const response = await fetch(`${API_BASE_URL}/seed-sample-data`, {
            method: 'POST'
        });
        
        if (!response.ok) {
            throw new Error('Failed to seed data');
        }
        
        const result = await response.json();
        
        // Hide the warning
        document.getElementById('noDataWarning').style.display = 'none';
        
        // Show success message
        alert(`✅ 成功！\n新增 ${result.newRecords} 筆記錄\n資料庫總計 ${result.totalRecords} 筆記錄\n\n頁面即將自動重新載入...`);
        
        // Reload the page to show new data
        setTimeout(() => {
            window.location.reload();
        }, 1500);
        
    } catch (error) {
        console.error('Error seeding data:', error);
        alert('❌ 新增範例資料失敗。請查看控制台了解詳情。');
        button.innerHTML = originalText;
        button.disabled = false;
    }
}

// Load latest oil prices
async function loadLatestPrices() {
    try {
        const response = await fetch(`${API_BASE_URL}/latest`);
        if (!response.ok) {
            console.error('Failed to load latest prices');
            // Show no data warning if 404 (no data found)
            if (response.status === 404) {
                document.getElementById('noDataWarning').style.display = 'block';
            }
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

// Filter chart by weeks
function filterByWeeks(weeks) {
    currentWeeksFilter = parseInt(weeks);
    
    if (currentWeeksFilter === 0 || allChartData.length === 0) {
        // Show all data
        drawChart(allChartData);
        loadStatistics(0); // Update statistics for all data
    } else {
        // Filter to last N weeks of historical data + all predictions
        const daysToShow = currentWeeksFilter * 7;
        const firstPredictionIndex = allChartData.findIndex(d => d.isPrediction);
        
        if (firstPredictionIndex === -1) {
            // No predictions, just filter historical by date
            const cutoffDate = new Date();
            cutoffDate.setDate(cutoffDate.getDate() - daysToShow);
            const filteredData = allChartData.filter(d => new Date(d.date) >= cutoffDate);
            drawChart(filteredData);
        } else {
            // Get last N weeks of historical by date + all predictions
            const historicalData = allChartData.slice(0, firstPredictionIndex);
            const predictionData = allChartData.slice(firstPredictionIndex);
            
            // Filter historical by date (not by count)
            const cutoffDate = new Date();
            cutoffDate.setDate(cutoffDate.getDate() - daysToShow);
            const filteredHistorical = historicalData.filter(d => new Date(d.date) >= cutoffDate);
            const filteredData = [...filteredHistorical, ...predictionData];
            drawChart(filteredData);
        }
        
        // Update statistics based on selected weeks
        loadStatistics(daysToShow);
    }
}

// Load chart with historical data and predictions
async function loadChart(days) {
    currentDays = days;
    
    try {
        // Always fetch all historical data (days parameter ignored on server side)
        const response = await fetch(`${API_BASE_URL}/history-with-predictions?days=${days}`);
        if (!response.ok) {
            console.error('Failed to load chart data');
            return;
        }
        
        const data = await response.json();
        allChartData = data; // Store for filtering
        
        // Draw chart using canvas
        drawChart(data);
        
        // Setup hover interaction
        setupChartHover();
        
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
    
    // Store chart data for hover detection
    window.currentChartData = {
        data: data,
        padding: padding,
        chartWidth: chartWidth,
        chartHeight: chartHeight,
        minPrice: minPrice,
        maxPrice: maxPrice
    };
    
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
    ctx.fillText('油價走勢圖', canvas.width / 2, 30);
    
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
        
        // Update statistics title based on filter
        const title = days === 0 ? '統計資訊（所有歷史資料）' : `統計資訊（最近 ${Math.floor(days / 7)} 週）`;
        document.getElementById('statisticsTitle').textContent = title;
        
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

// Setup chart hover interaction
function setupChartHover() {
    const canvas = document.getElementById('priceChart');
    const tooltip = document.getElementById('chartTooltip');
    
    canvas.addEventListener('mousemove', function(e) {
        if (!window.currentChartData) return;
        
        const rect = canvas.getBoundingClientRect();
        const mouseX = e.clientX - rect.left;
        const mouseY = e.clientY - rect.top;
        
        const { data, padding, chartWidth, chartHeight, minPrice, maxPrice } = window.currentChartData;
        
        // Check if mouse is within chart area
        if (mouseX < padding || mouseX > padding + chartWidth ||
            mouseY < padding || mouseY > padding + chartHeight) {
            tooltip.style.display = 'none';
            return;
        }
        
        // Find nearest data point
        const relativeX = (mouseX - padding) / chartWidth;
        const dataIndex = Math.round(relativeX * (data.length - 1));
        
        if (dataIndex < 0 || dataIndex >= data.length) {
            tooltip.style.display = 'none';
            return;
        }
        
        const point = data[dataIndex];
        
        // Find which fuel type is closest to mouse Y
        const fuelTypes = [
            { key: 'price92', label: '92無鉛', price: point.price92 },
            { key: 'price95', label: '95無鉛', price: point.price95 },
            { key: 'price98', label: '98無鉛', price: point.price98 },
            { key: 'priceDiesel', label: '超級柴油', price: point.priceDiesel }
        ];
        
        let closestFuel = fuelTypes[0];
        let minDistance = Infinity;
        
        fuelTypes.forEach(fuel => {
            const y = padding + chartHeight - ((fuel.price - minPrice) / (maxPrice - minPrice)) * chartHeight;
            const distance = Math.abs(y - mouseY);
            if (distance < minDistance) {
                minDistance = distance;
                closestFuel = fuel;
            }
        });
        
        // Show tooltip
        const date = new Date(point.date);
        const dateStr = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
        const typeStr = point.isPrediction ? '預測值' : '實際值';
        
        tooltip.innerHTML = `${dateStr}<br>${closestFuel.label}: $${closestFuel.price.toFixed(1)} (${typeStr})`;
        tooltip.style.display = 'block';
        tooltip.style.left = (mouseX + 15) + 'px';
        tooltip.style.top = (mouseY - 15) + 'px';
    });
    
    canvas.addEventListener('mouseleave', function() {
        tooltip.style.display = 'none';
    });
}

// Query price prediction
async function queryPrediction() {
    const weeks = parseInt(document.getElementById('futureWeek').value);
    const fuelType = document.getElementById('fuelTypeSelect').value;
    
    if (isNaN(weeks) || weeks < 1 || weeks > 4) {
        alert('請輸入1-4之間的週數');
        return;
    }
    
    // Calculate target date (weeks * 7 days from today)
    const targetDate = new Date();
    targetDate.setDate(targetDate.getDate() + (weeks * 7));
    
    // Find prediction from chart data
    if (!allChartData || allChartData.length === 0) {
        alert('無法獲取預測資料，請先載入圖表');
        return;
    }
    
    // Find the prediction data point closest to target date
    const targetTime = targetDate.getTime();
    let closestPoint = null;
    let minDiff = Infinity;
    
    allChartData.filter(d => d.isPrediction).forEach(point => {
        const pointTime = new Date(point.date).getTime();
        const diff = Math.abs(pointTime - targetTime);
        if (diff < minDiff) {
            minDiff = diff;
            closestPoint = point;
        }
    });
    
    if (!closestPoint) {
        alert('無法找到對應的預測資料');
        return;
    }
    
    // Display result
    const fuelLabels = {
        'price92': '92無鉛',
        'price95': '95無鉛',
        'price98': '98無鉛',
        'priceDiesel': '超級柴油'
    };
    
    const predictedPrice = closestPoint[fuelType];
    const dateStr = new Date(closestPoint.date).toLocaleDateString('zh-TW');
    
    document.getElementById('predictionTableBody').innerHTML = `
        <tr>
            <td>第 ${weeks} 週</td>
            <td>${dateStr}</td>
            <td>${fuelLabels[fuelType]}</td>
            <td class="fw-bold text-primary">$${predictedPrice.toFixed(1)}</td>
        </tr>
    `;
    
    document.getElementById('predictionResult').style.display = 'block';
}

// Load 10 liter price comparison with last week
async function load10LiterComparison() {
    try {
        const response = await fetch(`${API_BASE_URL}/history?days=0`); // Get all data
        if (!response.ok) {
            console.error('Failed to load comparison data');
            return;
        }
        
        const data = await response.json();
        
        if (data.length < 2) {
            // Not enough data to compare (need at least 2 records)
            return;
        }
        
        // Use record-based comparison: compare current record (latest) with previous record
        // Since data is weekly, comparing with previous record = comparing with last week
        const currentData = data[data.length - 1];        // Most recent record (this week)
        const lastWeekData = data[data.length - 2];       // Previous record (last week)
        

        
        // Calculate difference per 10 liters
        const diff92 = currentData.price92 && lastWeekData.price92 ? (currentData.price92 - lastWeekData.price92) * 10 : null;
        const diff95 = currentData.price95 && lastWeekData.price95 ? (currentData.price95 - lastWeekData.price95) * 10 : null;
        const diff98 = currentData.price98 && lastWeekData.price98 ? (currentData.price98 - lastWeekData.price98) * 10 : null;
        const diffDiesel = currentData.priceDiesel && lastWeekData.priceDiesel ? (currentData.priceDiesel - lastWeekData.priceDiesel) * 10 : null;
        
        // Update UI
        document.getElementById('compare92').innerHTML = diff92 !== null ? formatComparisonText(diff92) : '--';
        document.getElementById('compare95').innerHTML = diff95 !== null ? formatComparisonText(diff95) : '--';
        document.getElementById('compare98').innerHTML = diff98 !== null ? formatComparisonText(diff98) : '--';
        document.getElementById('compareDiesel').innerHTML = diffDiesel !== null ? formatComparisonText(diffDiesel) : '--';
        
    } catch (error) {
        console.error('Error loading 10L comparison:', error);
    }
}

function formatComparisonText(diff) {
    if (diff > 0) {
        return `<span class="text-danger">▲ $${diff.toFixed(1)}</span>`;
    } else if (diff < 0) {
        return `<span class="text-success">▼ $${Math.abs(diff).toFixed(1)}</span>`;
    } else {
        return '<span class="text-muted">─ $0.0</span>';
    }
}

// Export data to CSV
function exportData(days) {
    window.location.href = `${API_BASE_URL}/export?days=${days}`;
}

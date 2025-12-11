const API_BASE_URL = '/api/oilprices';
let chartInstance = null;
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
        
        // Separate historical and predicted data
        const historicalData = data.filter(item => !item.isPrediction);
        const predictedData = data.filter(item => item.isPrediction);
        
        // Get the last historical data point to connect with predictions
        const lastHistorical = historicalData.length > 0 ? historicalData[historicalData.length - 1] : null;
        
        // Prepare labels (dates)
        const labels = data.map(item => {
            const date = new Date(item.date);
            return `${date.getMonth() + 1}/${date.getDate()}`;
        });
        
        // Create datasets for each fuel type
        const datasets = [
            createDataset('92無鉛', historicalData, predictedData, lastHistorical, 'price92', '#0d6efd'),
            createDataset('95無鉛', historicalData, predictedData, lastHistorical, 'price95', '#198754'),
            createDataset('98無鉛', historicalData, predictedData, lastHistorical, 'price98', '#ffc107'),
            createDataset('超級柴油', historicalData, predictedData, lastHistorical, 'priceDiesel', '#dc3545')
        ];
        
        // Destroy existing chart if it exists
        if (chartInstance) {
            chartInstance.destroy();
        }
        
        // Create new chart
        const ctx = document.getElementById('priceChart').getContext('2d');
        chartInstance = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: datasets
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                interaction: {
                    mode: 'index',
                    intersect: false,
                },
                plugins: {
                    title: {
                        display: true,
                        text: `油價走勢圖（最近${days}天 + 未來30天預測）`,
                        font: {
                            size: 18
                        }
                    },
                    legend: {
                        display: true,
                        position: 'top'
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                let label = context.dataset.label || '';
                                if (label) {
                                    label += ': ';
                                }
                                if (context.parsed.y !== null) {
                                    label += '$' + context.parsed.y.toFixed(1);
                                }
                                // Add prediction indicator
                                const dataIndex = context.dataIndex;
                                if (dataIndex >= historicalData.length) {
                                    label += ' (預測)';
                                }
                                return label;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: false,
                        title: {
                            display: true,
                            text: '價格 (元/公升)'
                        }
                    },
                    x: {
                        title: {
                            display: true,
                            text: '日期'
                        }
                    }
                }
            }
        });
        
        // Update button states
        document.querySelectorAll('.btn-group button').forEach(btn => {
            btn.classList.remove('active');
        });
        event?.target?.classList.add('active');
        
    } catch (error) {
        console.error('Error loading chart:', error);
    }
}

function createDataset(label, historicalData, predictedData, lastHistorical, priceKey, color) {
    // Historical data
    const historicalValues = historicalData.map(item => item[priceKey]);
    
    // Predicted data (including the last historical point to connect the line)
    const predictedValues = predictedData.map(item => item[priceKey]);
    
    // Create full dataset arrays
    const allData = [...historicalValues, ...predictedValues];
    
    // Create segment styling - solid for historical, dashed for predictions
    const segments = [];
    const historicalLength = historicalData.length;
    
    return {
        label: label,
        data: allData,
        borderColor: color,
        backgroundColor: color + '33', // Add transparency
        tension: 0.3,
        fill: false,
        pointRadius: 3,
        pointHoverRadius: 5,
        segment: {
            borderDash: ctx => {
                // If we're in the prediction range, use dashed line
                return ctx.p0DataIndex >= historicalLength - 1 ? [5, 5] : undefined;
            }
        }
    };
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

// Refresh data from API
async function refreshData() {
    const refreshIcon = document.getElementById('refreshIcon');
    refreshIcon.style.animation = 'spin 1s linear infinite';
    
    try {
        const response = await fetch(`${API_BASE_URL}/refresh?days=90`, {
            method: 'POST'
        });
        
        if (response.ok) {
            // Reload all data
            await loadLatestPrices();
            await loadChart(currentDays);
            await loadStatistics(30);
            
            alert('資料更新成功！');
        } else {
            alert('資料更新失敗，請稍後再試。');
        }
    } catch (error) {
        console.error('Error refreshing data:', error);
        alert('資料更新失敗，請稍後再試。');
    } finally {
        refreshIcon.style.animation = '';
    }
}

// Export data to CSV
function exportData(days) {
    window.location.href = `${API_BASE_URL}/export?days=${days}`;
}

// Add CSS animation for refresh icon
const style = document.createElement('style');
style.textContent = `
    @keyframes spin {
        from { transform: rotate(0deg); }
        to { transform: rotate(360deg); }
    }
`;
document.head.appendChild(style);

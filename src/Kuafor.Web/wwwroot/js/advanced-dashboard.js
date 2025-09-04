function initializeAdvancedDashboard() {
document.addEventListener('DOMContentLoaded', function() {
    initializeCharts();
    initializeRealTimeUpdates();
    initializeTooltips();
});

let mainChart;
let customerSegmentChart;

function initializeCharts() {
    // Ana grafik (Ciro/Randevu)
    const ctx = document.getElementById('mainChart').getContext('2d');
    mainChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: window.dashboardData.revenueChart.map(item => item.label),
            datasets: [{
                label: 'Ciro (₺)',
                data: window.dashboardData.revenueChart.map(item => item.value),
                borderColor: '#007bff',
                backgroundColor: 'rgba(0, 123, 255, 0.1)',
                tension: 0.4,
                fill: true
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return '₺' + value.toLocaleString();
                        }
                    }
                }
            },
            interaction: {
                intersect: false,
                mode: 'index'
            }
        }
    });

    // Müşteri segmentasyonu grafiği
    const segmentCtx = document.getElementById('customerSegmentChart').getContext('2d');
    customerSegmentChart = new Chart(segmentCtx, {
        type: 'doughnut',
        data: {
            labels: window.dashboardData.customerSegments.map(item => item.segment),
            datasets: [{
                data: window.dashboardData.customerSegments.map(item => item.count),
                backgroundColor: [
                    '#FF6384',
                    '#36A2EB',
                    '#FFCE56',
                    '#4BC0C0'
                ],
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 20,
                        usePointStyle: true
                    }
                }
            }
        }
    });
}

function showChart(type) {
    const buttons = document.querySelectorAll('.btn-group .btn');
    buttons.forEach(btn => btn.classList.remove('active'));
    event.target.classList.add('active');

    if (type === 'revenue') {
        mainChart.data.datasets[0].label = 'Ciro (₺)';
        mainChart.data.datasets[0].data = window.dashboardData.revenueChart.map(item => item.value);
        mainChart.data.datasets[0].borderColor = '#007bff';
        mainChart.data.datasets[0].backgroundColor = 'rgba(0, 123, 255, 0.1)';
    } else if (type === 'appointments') {
        mainChart.data.datasets[0].label = 'Randevu Sayısı';
        mainChart.data.datasets[0].data = window.dashboardData.appointmentChart.map(item => item.value);
        mainChart.data.datasets[0].borderColor = '#28a745';
        mainChart.data.datasets[0].backgroundColor = 'rgba(40, 167, 69, 0.1)';
    }
    
    mainChart.update();
}

function refreshDashboard() {
    // Dashboard'u yenile
    location.reload();
}

function changePeriod(days) {
    // Dönem değiştirme
    console.log('Dönem değiştirildi:', days, 'gün');
    // Burada AJAX çağrısı yapılabilir
}

function initializeRealTimeUpdates() {
    // Her 30 saniyede bir dashboard'u güncelle
    setInterval(() => {
        updateKPIs();
    }, 30000);
}

function updateKPIs() {
    // KPI'ları güncelle (AJAX çağrısı)
    fetch('/Admin/Dashboard/GetKPIs')
        .then(response => response.json())
        .then(data => {
            // KPI değerlerini güncelle
            updateKPICard(0, data.todayAppointments);
            updateKPICard(1, '₺' + data.todayRevenue.toLocaleString());
            updateKPICard(2, data.activeStylists);
            updateKPICard(3, data.totalCustomers);
        })
        .catch(error => console.error('KPI güncelleme hatası:', error));
}

function updateKPICard(index, value) {
    const cards = document.querySelectorAll('.kpi-card .fs-4');
    if (cards[index]) {
        cards[index].textContent = value;
    }
}

function initializeTooltips() {
    // Bootstrap tooltip'leri başlat
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// Animasyonlar
function animateCounters() {
    const counters = document.querySelectorAll('.fs-4.fw-bold');
    counters.forEach(counter => {
        const target = parseInt(counter.textContent.replace(/[^\d]/g, ''));
        animateCounter(counter, 0, target, 2000);
    });
}

function animateCounter(element, start, end, duration) {
    const startTime = performance.now();
    const isCurrency = element.textContent.includes('₺');
    
    function updateCounter(currentTime) {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / duration, 1);
        const current = Math.floor(progress * (end - start) + start);
        
        if (isCurrency) {
            element.textContent = '₺' + current.toLocaleString();
        } else {
            element.textContent = current.toLocaleString();
        }
        
        if (progress < 1) {
            requestAnimationFrame(updateCounter);
        }
    }
    
    requestAnimationFrame(updateCounter);
}
}
// Sayfa yüklendiğinde animasyonları başla
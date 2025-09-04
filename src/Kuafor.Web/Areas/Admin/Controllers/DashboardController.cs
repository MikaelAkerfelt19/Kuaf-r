using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kuafor.Web.Models.Admin;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] 
    public class DashboardController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IServiceService _serviceService;
        private readonly IStylistService _stylistService;
        private readonly IBranchService _branchService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IAdisyonService _adisyonService;

        public DashboardController(
            IAppointmentService appointmentService,
            IServiceService serviceService,
            IStylistService stylistService,
            IBranchService branchService,
            ICustomerService customerService,
            IProductService productService,
            IAdisyonService adisyonService)
        {
            _appointmentService = appointmentService;
            _serviceService = serviceService;
            _stylistService = stylistService;
            _branchService = branchService;
            _customerService = customerService;
            _productService = productService;
            _adisyonService = adisyonService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var lastMonthStart = monthStart.AddMonths(-1);
            var lastMonthEnd = monthStart.AddDays(-1);

            // Temel veriler
            var todayAppointments = await _appointmentService.GetByDateRangeAsync(today, today.AddDays(1));
            var weekAppointments = await _appointmentService.GetByDateRangeAsync(weekStart, weekEnd);
            var monthAppointments = await _appointmentService.GetByDateRangeAsync(monthStart, today.AddDays(1));
            var lastMonthAppointments = await _appointmentService.GetByDateRangeAsync(lastMonthStart, lastMonthEnd);
            
            var activeStylists = await _stylistService.GetActiveAsync();
            var activeBranches = await _branchService.GetActiveAsync();
            var allCustomers = await _customerService.GetAllAsync();
            var topServices = await GetTopServicesAsync(7);

            // Gelişmiş KPI'lar
            var todayRevenue = todayAppointments.Sum(a => a.FinalPrice);
            var weekRevenue = weekAppointments.Sum(a => a.FinalPrice);
            var monthRevenue = monthAppointments.Sum(a => a.FinalPrice);
            var lastMonthRevenue = lastMonthAppointments.Sum(a => a.FinalPrice);

            var vm = new AdminDashboardViewModel
            {
                Kpis = new List<KpiVm>
                {
                    new KpiVm 
                    { 
                        Label = "Bugün Randevu", 
                        Value = todayAppointments.Count().ToString(), 
                        SubText = $"Son 7g ort: {weekAppointments.Count() / 7}", 
                        Badge = CalculateTrendBadge(todayAppointments.Count(), weekAppointments.Count() / 7),
                        Icon = "bi bi-calendar-check",
                        Color = "primary"
                    },
                    new KpiVm 
                    { 
                        Label = "Bugünkü Ciro", 
                        Value = "₺" + todayRevenue.ToString("N0"), 
                        SubText = "Tahmini", 
                        Badge = CalculateTrendBadge(todayRevenue, weekRevenue / 7),
                        Icon = "bi bi-currency-dollar",
                        Color = "success"
                    },
                    new KpiVm 
                    { 
                        Label = "Aktif Kuaför", 
                        Value = activeStylists.Count().ToString(), 
                        SubText = $"Toplam {activeStylists.Count()}",
                        Icon = "bi bi-people",
                        Color = "info"
                    },
                    new KpiVm 
                    { 
                        Label = "Toplam Müşteri", 
                        Value = allCustomers.Count().ToString(), 
                        SubText = "Kayıtlı müşteri",
                        Icon = "bi bi-person-heart",
                        Color = "warning"
                    }
                },
                Upcoming = todayAppointments.Take(5).Select(a => new UpcomingApptRow(
                    a.Id, a.StartAt, $"{a.Customer.FirstName} {a.Customer.LastName}", 
                    a.Service.Name, $"{a.Stylist.FirstName} {a.Stylist.LastName}", a.Branch.Name)).ToList(),
                TopServices = topServices.Select(s => new TopServiceRow(s.Name, s.AppointmentCount, "+")).ToList(),
                
                // Gelişmiş özellikler
                RevenueChart = await GetRevenueChartDataAsync(30),
                AppointmentChart = await GetAppointmentChartDataAsync(30),
                TopStylists = await GetTopStylistsAsync(5),
                BranchPerformances = await GetBranchPerformancesAsync(),
                CustomerSegments = await GetCustomerSegmentsAsync(),
                RecentActivities = await GetRecentActivitiesAsync(10),
                InventoryAlerts = await GetInventoryAlertsAsync(),
                FinancialSummaries = await GetFinancialSummariesAsync(),
                
                // Trend analizleri
                RevenueTrend = CalculateTrendAnalysis(monthRevenue, lastMonthRevenue),
                AppointmentTrend = CalculateTrendAnalysis(monthAppointments.Count(), lastMonthAppointments.Count()),
                CustomerTrend = CalculateTrendAnalysis(allCustomers.Count(), allCustomers.Count()) // Geçmiş veri yoksa aynı
            };
            
            return View(vm);
        }

        private string CalculateTrendBadge(decimal current, decimal previous)
        {
            if (previous == 0) return "+0%";
            var change = ((current - previous) / previous) * 100;
            return $"{(change >= 0 ? "+" : "")}{change:F1}%";
        }

        private TrendAnalysis CalculateTrendAnalysis(decimal current, decimal previous)
        {
            if (previous == 0) 
                return new TrendAnalysis 
                { 
                    CurrentValue = current, 
                    PreviousValue = previous, 
                    ChangePercentage = 0, 
                    Trend = "stable",
                    TrendIcon = "bi bi-dash"
                };

            var changePercentage = ((current - previous) / previous) * 100;
            var trend = changePercentage > 0 ? "up" : changePercentage < 0 ? "down" : "stable";
            var trendIcon = changePercentage > 0 ? "bi bi-arrow-up" : changePercentage < 0 ? "bi bi-arrow-down" : "bi bi-dash";

            return new TrendAnalysis
            {
                CurrentValue = current,
                PreviousValue = previous,
                ChangePercentage = changePercentage,
                Trend = trend,
                TrendIcon = trendIcon
            };
        }

        private async Task<List<ServiceStats>> GetTopServicesAsync(int days)
        {
            var fromDate = DateTime.Today.AddDays(-days);
            var appointments = await _appointmentService.GetByDateRangeAsync(fromDate, DateTime.Today.AddDays(1));
            
            return appointments
                .GroupBy(a => a.ServiceId)
                .Select(g => new ServiceStats
                {
                    Name = g.First().Service?.Name ?? "Bilinmeyen Hizmet",
                    AppointmentCount = g.Count()
                })
                .OrderByDescending(s => s.AppointmentCount)
                .Take(5)
                .ToList();
        }

        private async Task<List<ChartDataPoint>> GetRevenueChartDataAsync(int days)
        {
            var chartData = new List<ChartDataPoint>();
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-days);

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var nextDate = date.AddDays(1);
                var appointments = await _appointmentService.GetByDateRangeAsync(date, nextDate);
                var revenue = appointments.Sum(a => a.FinalPrice);

                chartData.Add(new ChartDataPoint
                {
                    Label = date.ToString("dd/MM"),
                    Value = revenue,
                    Color = "#007bff"
                });
            }

            return chartData;
        }

        private async Task<List<ChartDataPoint>> GetAppointmentChartDataAsync(int days)
        {
            var chartData = new List<ChartDataPoint>();
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-days);

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var nextDate = date.AddDays(1);
                var appointments = await _appointmentService.GetByDateRangeAsync(date, nextDate);

                chartData.Add(new ChartDataPoint
                {
                    Label = date.ToString("dd/MM"),
                    Value = appointments.Count(),
                    Color = "#28a745"
                });
            }

            return chartData;
        }

        private async Task<List<StylistPerformance>> GetTopStylistsAsync(int count)
        {
            var stylists = await _stylistService.GetActiveAsync();
            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var appointments = await _appointmentService.GetByDateRangeAsync(monthStart, DateTime.Today.AddDays(1));

            return stylists
                .Select(s => new StylistPerformance
                {
                    Id = s.Id,
                    Name = $"{s.FirstName} {s.LastName}",
                    AppointmentCount = appointments.Count(a => a.StylistId == s.Id),
                    Revenue = appointments.Where(a => a.StylistId == s.Id).Sum(a => a.FinalPrice),
                    Rating = (double)s.Rating,
                    Trend = "+5%" // TODO: Gerçek trend hesaplama
                })
                .OrderByDescending(s => s.Revenue)
                .Take(count)
                .ToList();
        }

        private async Task<List<BranchPerformance>> GetBranchPerformancesAsync()
        {
            var branches = await _branchService.GetActiveAsync();
            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var appointments = await _appointmentService.GetByDateRangeAsync(monthStart, DateTime.Today.AddDays(1));
            var stylists = await _stylistService.GetActiveAsync();

            return branches
                .Select(b => new BranchPerformance
                {
                    Id = b.Id,
                    Name = b.Name,
                    AppointmentCount = appointments.Count(a => a.BranchId == b.Id),
                    Revenue = appointments.Where(a => a.BranchId == b.Id).Sum(a => a.FinalPrice),
                    ActiveStylists = stylists.Count(s => s.BranchId == b.Id),
                    UtilizationRate = 85.5 // TODO: Gerçek kullanım oranı hesaplama
                })
                .OrderByDescending(b => b.Revenue)
                .ToList();
        }

        private async Task<List<CustomerSegment>> GetCustomerSegmentsAsync()
        {
            var customers = await _customerService.GetAllAsync();
            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var appointments = await _appointmentService.GetByDateRangeAsync(monthStart, DateTime.Today.AddDays(1));

            var totalCustomers = customers.Count();
            var totalRevenue = appointments.Sum(a => a.FinalPrice);

            return new List<CustomerSegment>
            {
                new CustomerSegment { Segment = "VIP Müşteriler", Count = (int)(totalCustomers * 0.1), Revenue = totalRevenue * 0.4m, Percentage = "10%" },
                new CustomerSegment { Segment = "Sadık Müşteriler", Count = (int)(totalCustomers * 0.3), Revenue = totalRevenue * 0.4m, Percentage = "30%" },
                new CustomerSegment { Segment = "Yeni Müşteriler", Count = (int)(totalCustomers * 0.2), Revenue = totalRevenue * 0.1m, Percentage = "20%" },
                new CustomerSegment { Segment = "Risk Altındaki", Count = (int)(totalCustomers * 0.4), Revenue = totalRevenue * 0.1m, Percentage = "40%" }
            };
        }

        private async Task<List<RecentActivity>> GetRecentActivitiesAsync(int count)
        {
            var activities = new List<RecentActivity>();
            var today = DateTime.Today;
            var appointments = await _appointmentService.GetByDateRangeAsync(today, today.AddDays(1));

            // Son randevular
            foreach (var appointment in appointments.Take(3))
            {
                activities.Add(new RecentActivity
                {
                    Timestamp = appointment.CreatedAt,
                    Type = "Randevu",
                    Description = $"{appointment.Customer.FirstName} {appointment.Customer.LastName} - {appointment.Service.Name}",
                    Icon = "bi bi-calendar-plus",
                    Color = "success"
                });
            }

            // Stok uyarıları
            var inventoryAlerts = await GetInventoryAlertsAsync();
            foreach (var alert in inventoryAlerts.Take(2))
            {
                activities.Add(new RecentActivity
                {
                    Timestamp = DateTime.UtcNow,
                    Type = "Stok Uyarısı",
                    Description = $"{alert.ProductName} stokta azalıyor ({alert.CurrentStock} adet)",
                    Icon = "bi bi-exclamation-triangle",
                    Color = "warning"
                });
            }

            return activities.OrderByDescending(a => a.Timestamp).Take(count).ToList();
        }

        private async Task<List<InventoryAlert>> GetInventoryAlertsAsync()
        {
            var products = await _productService.GetAllAsync();
            return products
                .Where(p => p.Stock <= p.MinimumStock)
                .Select(p => new InventoryAlert
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    CurrentStock = p.Stock,
                    MinimumStock = p.MinimumStock,
                    AlertLevel = p.Stock == 0 ? "danger" : p.Stock <= p.MinimumStock * 0.5 ? "warning" : "info"
                })
                .ToList();
        }

        private async Task<List<FinancialSummary>> GetFinancialSummariesAsync()
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var todayAppointments = await _appointmentService.GetByDateRangeAsync(today, today.AddDays(1));
            var weekAppointments = await _appointmentService.GetByDateRangeAsync(weekStart, weekStart.AddDays(7));
            var monthAppointments = await _appointmentService.GetByDateRangeAsync(monthStart, today.AddDays(1));

            return new List<FinancialSummary>
            {
                new FinancialSummary
                {
                    Period = "Bugün",
                    Revenue = todayAppointments.Sum(a => a.FinalPrice),
                    Expenses = todayAppointments.Sum(a => a.FinalPrice) * 0.3m, // %30 maliyet varsayımı
                    Profit = todayAppointments.Sum(a => a.FinalPrice) * 0.7m,
                    ProfitMargin = 70
                },
                new FinancialSummary
                {
                    Period = "Bu Hafta",
                    Revenue = weekAppointments.Sum(a => a.FinalPrice),
                    Expenses = weekAppointments.Sum(a => a.FinalPrice) * 0.3m,
                    Profit = weekAppointments.Sum(a => a.FinalPrice) * 0.7m,
                    ProfitMargin = 70
                },
                new FinancialSummary
                {
                    Period = "Bu Ay",
                    Revenue = monthAppointments.Sum(a => a.FinalPrice),
                    Expenses = monthAppointments.Sum(a => a.FinalPrice) * 0.3m,
                    Profit = monthAppointments.Sum(a => a.FinalPrice) * 0.7m,
                    ProfitMargin = 70
                }
            };
        }
    }

    public class ServiceStats
    {
        public string Name { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
    }
}
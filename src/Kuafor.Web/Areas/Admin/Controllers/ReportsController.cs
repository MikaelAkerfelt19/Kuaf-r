using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Reports;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Admin;

namespace Kuafor.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/[controller]")]
public class ReportsController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IStylistService _stylistService;
    private readonly IBranchService _branchService;
    private readonly ICustomerService _customerService;
    private readonly IInventoryService _inventoryService;
    private readonly IReportingService _reportingService;

    public ReportsController(
        IAppointmentService appointmentService,
        IStylistService stylistService,
        IBranchService branchService,
        ICustomerService customerService,
        IInventoryService inventoryService,
        IReportingService reportingService)
    {
        _appointmentService = appointmentService;
        _stylistService = stylistService;
        _branchService = branchService;
        _customerService = customerService;
        _inventoryService = inventoryService;
        _reportingService = reportingService;
    }

    // GET: /Admin/Reports
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index()
    {
        // ReportsViewModel oluştur
        var reportsViewModel = new ReportsViewModel
        {
            TotalAppointments = 0,
            TodayAppointments = 0,
            WeekAppointments = 0,
            TotalRevenue = 0,
            TodayRevenue = 0,
            WeeklyRevenue = 0,
            WeekRevenueEstimate = 0,
            ActiveStylists = 0,
            ActiveBranches = 0,
            ActiveServices = 0,
            Next7Days = new List<DayBucket>(),
            TopStylistsByWeek = new List<TopStylistSummary>(),
            TopStylistsByMonth = new List<TopStylistSummary>(),
            BranchPerformance = new List<BranchPerformanceSummary>()
        };

        // Dashboard raporunu al ve ReportsViewModel'e dönüştür
        var dashboardReport = await _reportingService.GetDashboardReportAsync();
        
        reportsViewModel.TotalAppointments = dashboardReport.TotalAppointments;
        reportsViewModel.TotalRevenue = dashboardReport.TotalRevenue;
        reportsViewModel.ActiveStylists = dashboardReport.ActiveStylists;
        reportsViewModel.ActiveBranches = dashboardReport.ActiveBranches;
        
        // Bugün ve bu hafta için ayrı hesaplamalar
        var today = DateTime.Today;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var weekEnd = weekStart.AddDays(6);
        
        var todayAppointments = await _appointmentService.GetByDateRangeAsync(today, today.AddDays(1));
        var weekAppointments = await _appointmentService.GetByDateRangeAsync(weekStart, weekEnd);
        
        reportsViewModel.TodayAppointments = todayAppointments.Count();
        reportsViewModel.WeekAppointments = weekAppointments.Count();
        reportsViewModel.TodayRevenue = todayAppointments.Sum(a => a.FinalPrice);
        reportsViewModel.WeeklyRevenue = weekAppointments.Sum(a => a.FinalPrice);
        
        // Önümüzdeki 7 gün için randevular
        var next7Days = new List<DayBucket>();
        for (int i = 0; i < 7; i++)
        {
            var date = today.AddDays(i);
            var dayAppointments = await _appointmentService.GetByDateRangeAsync(date, date.AddDays(1));
            next7Days.Add(new DayBucket
            {
                Day = date,
                Count = dayAppointments.Count()
            });
        }
        reportsViewModel.Next7Days = next7Days;
        
        // Bu hafta en yoğun kuaförler
        var stylists = await _stylistService.GetAllAsync();
        var topStylistsByWeek = stylists
            .Select(s => new TopStylistSummary
            {
                Name = $"{s.FirstName} {s.LastName}",
                Count = weekAppointments.Count(a => a.StylistId == s.Id)
            })
            .OrderByDescending(s => s.Count)
            .Take(5)
            .ToList();
        reportsViewModel.TopStylistsByWeek = topStylistsByWeek;
        
        return View(reportsViewModel);
    }

    // GET: /Admin/Reports/Revenue
    [Route("Revenue")]
    public async Task<IActionResult> Revenue(DateTime? from, DateTime? to)
    {
        var fromDate = from ?? DateTime.Today.AddMonths(-1);
        var toDate = to ?? DateTime.Today;

        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;
        
        var revenueReport = await _reportingService.GetRevenueReportAsync(fromDate, toDate);
        return View(revenueReport);
    }

    // GET: /Admin/Reports/Appointments
    [Route("Appointments")]
    public async Task<IActionResult> Appointments(DateTime? from, DateTime? to)
    {
        var fromDate = from ?? DateTime.Today.AddMonths(-1);
        var toDate = to ?? DateTime.Today;

        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;

        var appointmentReport = await _reportingService.GetAppointmentReportAsync(fromDate, toDate);
        return View(appointmentReport);
    }

    // GET: /Admin/Reports/Customers
    [Route("Customers")]
    public async Task<IActionResult> Customers(DateTime? from, DateTime? to)
    {
        var fromDate = from ?? DateTime.Today.AddMonths(-1);
        var toDate = to ?? DateTime.Today;

        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;

        var customerReport = await _reportingService.GetCustomerReportAsync(fromDate, toDate);
        return View(customerReport);
    }

    // GET: /Admin/Reports/Stylists
    [Route("Stylists")]
    public async Task<IActionResult> Stylists(DateTime? from, DateTime? to)
    {
        var fromDate = from ?? DateTime.Today.AddMonths(-1);
        var toDate = to ?? DateTime.Today;
        
        var stylistReport = await _reportingService.GetStylistReportAsync(fromDate, toDate);
        return View(stylistReport);
    }

    // GET: /Admin/Reports/Branches
    [Route("Branches")]
    public async Task<IActionResult> Branches(DateTime? from, DateTime? to)
    {
        var fromDate = from ?? DateTime.Today.AddMonths(-1);
        var toDate = to ?? DateTime.Today;
        
        var branchReport = await _reportingService.GetBranchReportAsync(fromDate, toDate);
        return View(branchReport);
    }

    // API: Rapor verilerini JSON olarak döndür
    [HttpGet]
    [Route("GetReportData")]
    public async Task<IActionResult> GetReportData(string reportType, DateTime? from, DateTime? to)
    {
        try
    {
        var fromDate = from ?? DateTime.Today.AddDays(-30);
        var toDate = to ?? DateTime.Today;

            if (string.IsNullOrEmpty(reportType))
            {
                return BadRequest("Rapor tipi belirtilmedi");
            }

        switch (reportType.ToLower())
        {
            case "revenue":
                    return Json(await _reportingService.GetRevenueChartDataAsync(fromDate, toDate));
            case "appointments":
                    return Json(await _reportingService.GetAppointmentChartDataAsync(fromDate, toDate));
            case "customers":
                    return Json(await _reportingService.GetCustomerChartDataAsync(fromDate, toDate));
            default:
                return BadRequest("Geçersiz rapor tipi");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetReportData Error: {ex.Message}");
            return StatusCode(500, new { error = "Rapor verisi alınırken hata oluştu" });
        }
    }

    // Export işlemleri
    [HttpPost]
    public async Task<IActionResult> ExportReport(string reportType, string format, DateTime? from, DateTime? to)
    {
        try
        {
            var fromDate = from ?? DateTime.Today.AddDays(-30);
            var toDate = to ?? DateTime.Today;

            byte[] fileBytes;
            string fileName;
            string contentType;

            switch (reportType.ToLower())
            {
                case "revenue":
                    var revenueReport = await _reportingService.GetRevenueReportAsync(fromDate, toDate);
                    fileName = $"Gelir_Raporu_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}";
                    break;
                case "appointments":
                    var appointmentReport = await _reportingService.GetAppointmentReportAsync(fromDate, toDate);
                    fileName = $"Randevu_Raporu_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}";
                    break;
                case "customers":
                    var customerReport = await _reportingService.GetCustomerReportAsync(fromDate, toDate);
                    fileName = $"Musteri_Raporu_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}";
                    break;
                default:
                    return BadRequest("Geçersiz rapor tipi");
            }

            switch (format.ToLower())
            {
                case "excel":
                    fileBytes = await _reportingService.ExportToExcel(new[] { new { } }, fileName);
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileName += ".xlsx";
                    break;
                case "pdf":
                    fileBytes = await _reportingService.ExportToPdfAsync(new[] { new { } }, fileName, reportType);
                    contentType = "application/pdf";
                    fileName += ".pdf";
                    break;
                case "csv":
                    fileBytes = await _reportingService.ExportToCsv(new[] { new { } }, fileName);
                    contentType = "text/csv";
                    fileName += ".csv";
                    break;
                default:
                    return BadRequest("Geçersiz format");
            }

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Export Error: {ex.Message}");
            return StatusCode(500, new { error = "Export işlemi sırasında hata oluştu" });
        }
    }

    private async Task<List<Models.Admin.Reports.BranchPerformance>> GetBranchPerformanceDataAsync()
    {
        var branches = await _branchService.GetAllAsync();
        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var appointments = await _appointmentService.GetByDateRangeAsync(monthStart, DateTime.Today.AddDays(1));
        var stylists = await _stylistService.GetAllAsync();

        return branches.Select(b => new Models.Admin.Reports.BranchPerformance
        {
            Id = b.Id,
            Name = b.Name,
            AppointmentCount = appointments.Count(a => a.BranchId == b.Id),
            TotalRevenue = appointments.Where(a => a.BranchId == b.Id).Sum(a => a.FinalPrice),
            AverageRating = stylists.Any() ? Math.Round((double)stylists.Average(s => s.Rating), 1) : 0
        }).ToList();
    }

    private async Task<CustomerAnalytics> GetCustomerAnalyticsAsync()
    {
        var customers = await _customerService.GetAllAsync();
        var appointments = await _appointmentService.GetAllAsync();

        return new CustomerAnalytics
        {
            TotalCustomers = customers.Count(),
            NewCustomersThisMonth = customers.Count(c => c.CreatedAt >= DateTime.Today.AddDays(-30)),
            ActiveCustomers = customers.Count(c => appointments.Any(a => a.CustomerId == c.Id && a.StartAt >= DateTime.Today.AddDays(-30))),
            AverageAppointmentsPerCustomer = customers.Any() ? (double)appointments.Count() / customers.Count() : 0
        };
    }

    private async Task<FinancialAnalytics> GetFinancialAnalyticsAsync()
    {
        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var appointments = await _appointmentService.GetByDateRangeAsync(monthStart, DateTime.Today.AddDays(1));

        // Önceki ayın verilerini al
        var previousMonthStart = monthStart.AddMonths(-1);
        var previousMonthEnd = monthStart.AddDays(-1);
        var previousMonthAppointments = await _appointmentService.GetByDateRangeAsync(previousMonthStart, previousMonthEnd);
        
        var currentRevenue = appointments.Sum(a => a.FinalPrice);
        var previousRevenue = previousMonthAppointments.Sum(a => a.FinalPrice);
        var revenueGrowth = previousRevenue > 0 ? ((currentRevenue - previousRevenue) / previousRevenue) * 100 : 0;
        
        return new FinancialAnalytics
        {
            MonthlyRevenue = currentRevenue,
            AverageTicketValue = appointments.Any() ? appointments.Average(a => a.FinalPrice) : 0,
            RevenueGrowth = Math.Round((double)revenueGrowth, 2),
            ProfitMargin = 70.0 // Bu değer gerçek maliyet hesaplaması gerektirir
        };
    }

    private async Task<InventoryAnalytics> GetInventoryAnalyticsAsync()
    {
        var inventoryReport = await _inventoryService.GetInventoryReportAsync();
        var inventoryValue = await _inventoryService.GetInventoryValueAsync();

        // Devir hızı hesaplama (basit hesaplama)
        var turnoverRate = inventoryValue > 0 ? (inventoryValue / 12) : 0; // Aylık ortalama
        
        return new InventoryAnalytics
        {
            TotalProducts = inventoryReport.Count,
            TotalValue = inventoryValue,
            LowStockCount = inventoryReport.Count(p => p.StockStatus == "Low"),
            OutOfStockCount = inventoryReport.Count(p => p.StockStatus == "Out"),
            TurnoverRate = Math.Round((double)turnoverRate, 2)
        };
    }

    private async Task<PerformanceMetrics> GetPerformanceMetricsAsync()
    {
        var appointments = await _appointmentService.GetAllAsync();
        var stylists = await _stylistService.GetAllAsync();
        
        // Gerçek verilerden hesaplama
        var completedAppointments = appointments.Where(a => a.Status == Models.Enums.AppointmentStatus.Completed).ToList();
        var noShowAppointments = appointments.Where(a => a.Status == Models.Enums.AppointmentStatus.NoShow).ToList();
        
        var averageServiceTime = completedAppointments.Any() ? 
            completedAppointments.Average(a => a.Service.DurationMin) : 0;
            
        var customerSatisfaction = completedAppointments.Any() ? 
            completedAppointments.Where(a => a.CustomerRating.HasValue).Average(a => (double)a.CustomerRating!.Value) : 0;
            
        var noShowRate = appointments.Any() ? 
            (noShowAppointments.Count() / (double)appointments.Count()) * 100 : 0;
            
        // Kullanım oranı hesaplama (basit)
        var utilizationRate = stylists.Any() ? 
            (completedAppointments.Count() / (double)(stylists.Count() * 30)) * 100 : 0; // 30 günlük ortalama

        return new PerformanceMetrics
        {
            AverageServiceTime = (int)Math.Round(averageServiceTime, 0),
            CustomerSatisfaction = Math.Round(customerSatisfaction, 1),
            UtilizationRate = Math.Round(utilizationRate, 1),
            NoShowRate = Math.Round(noShowRate, 1)
        };
    }

    private async Task<object> GetRevenueChartData(DateTime from, DateTime to)
    {
        var appointments = await _appointmentService.GetByDateRangeAsync(from, to);
        return appointments
            .GroupBy(a => a.StartAt.Date)
            .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), revenue = g.Sum(a => a.FinalPrice) })
            .OrderBy(x => x.date);
    }

    private async Task<object> GetAppointmentChartData(DateTime from, DateTime to)
    {
        var appointments = await _appointmentService.GetByDateRangeAsync(from, to);
        return appointments
            .GroupBy(a => a.StartAt.Date)
            .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), count = g.Count() })
            .OrderBy(x => x.date);
    }

    private async Task<object> GetCustomerChartData(DateTime from, DateTime to)
    {
        var customers = await _customerService.GetAllAsync();
        return customers
            .Where(c => c.CreatedAt >= from && c.CreatedAt <= to)
            .GroupBy(c => c.CreatedAt.Date)
            .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), count = g.Count() })
            .OrderBy(x => x.date);
    }

    private List<Models.Admin.CustomerSegment> GetCustomerSegments(IEnumerable<Models.Entities.Customer> customers, IEnumerable<Models.Entities.Appointment> appointments)
    {
        // Müşteri segmentasyonu logic'i
        return new List<Models.Admin.CustomerSegment>
        {
            new Models.Admin.CustomerSegment { Segment = "VIP", Count = customers.Count() / 10, Revenue = 0, Percentage = "10%" },
            new Models.Admin.CustomerSegment { Segment = "Sadık", Count = customers.Count() / 3, Revenue = 0, Percentage = "30%" },
            new Models.Admin.CustomerSegment { Segment = "Yeni", Count = customers.Count() / 5, Revenue = 0, Percentage = "20%" },
            new Models.Admin.CustomerSegment { Segment = "Risk", Count = customers.Count() / 2, Revenue = 0, Percentage = "40%" }
        };
    }

    private List<TopCustomer> GetTopCustomers(IEnumerable<Models.Entities.Customer> customers, IEnumerable<Models.Entities.Appointment> appointments)
    {
        return customers
            .Select(c => new TopCustomer
            {
                Name = $"{c.FirstName} {c.LastName}",
                AppointmentCount = appointments.Count(a => a.CustomerId == c.Id),
                TotalSpent = appointments.Where(a => a.CustomerId == c.Id).Sum(a => a.FinalPrice),
                LastVisit = appointments.Where(a => a.CustomerId == c.Id).Max(a => a.StartAt)
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(10)
            .ToList();
    }
}

// Yeni ViewModel'ler
public class FinancialReportViewModel
{
    public string Period { get; set; } = "";
    public decimal TotalRevenue { get; set; }
    public int TotalAppointments { get; set; }
    public decimal AverageTicketValue { get; set; }
    public List<BranchRevenue> BranchRevenues { get; set; } = new();
    public List<StylistRevenue> StylistRevenues { get; set; } = new();
}

public class CustomerReportViewModel
{
    public int TotalCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int ActiveCustomers { get; set; }
    public List<Models.Admin.CustomerSegment> CustomerSegments { get; set; } = new();
    public List<TopCustomer> TopCustomers { get; set; } = new();
}

public class InventoryReportViewModel
{
    public int TotalProducts { get; set; }
    public decimal TotalValue { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public List<StockMovement> RecentMovements { get; set; } = new();
    public List<CategoryBreakdown> CategoryBreakdown { get; set; } = new();
}

public class BranchRevenue
{
    public string BranchName { get; set; } = "";
    public decimal Revenue { get; set; }
    public int AppointmentCount { get; set; }
}

public class StylistRevenue
{
    public string StylistName { get; set; } = "";
    public decimal Revenue { get; set; }
    public int AppointmentCount { get; set; }
    public decimal Rating { get; set; }
}

public class TopCustomer
{
    public string Name { get; set; } = "";
    public int AppointmentCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime LastVisit { get; set; }
}

public class CategoryBreakdown
{
    public string Category { get; set; } = "";
    public int ProductCount { get; set; }
    public decimal TotalValue { get; set; }
    public double AverageStock { get; set; }
}

public class CustomerAnalytics
{
    public int TotalCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int ActiveCustomers { get; set; }
    public double AverageAppointmentsPerCustomer { get; set; }
}

public class FinancialAnalytics
{
    public decimal MonthlyRevenue { get; set; }
    public decimal AverageTicketValue { get; set; }
    public double RevenueGrowth { get; set; }
    public double ProfitMargin { get; set; }
}

public class InventoryAnalytics
{
    public int TotalProducts { get; set; }
    public decimal TotalValue { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public double TurnoverRate { get; set; }
}

public class PerformanceMetrics
{
    public int AverageServiceTime { get; set; }
    public double CustomerSatisfaction { get; set; }
    public double UtilizationRate { get; set; }
    public double NoShowRate { get; set; }
}

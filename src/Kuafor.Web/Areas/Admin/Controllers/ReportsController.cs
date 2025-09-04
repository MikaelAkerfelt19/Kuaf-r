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

    public ReportsController(
        IAppointmentService appointmentService,
        IStylistService stylistService,
        IBranchService branchService,
        ICustomerService customerService,
        IInventoryService inventoryService)
    {
        _appointmentService = appointmentService;
        _stylistService = stylistService;
        _branchService = branchService;
        _customerService = customerService;
        _inventoryService = inventoryService;
    }

    // GET: /Admin/Reports
    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var weekEnd = weekStart.AddDays(7);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var lastMonthStart = monthStart.AddMonths(-1);
        var lastMonthEnd = monthStart.AddDays(-1);

        var vm = new ReportsViewModel
        {
            // Gerçek veritabanı verileri
            TotalAppointments = await _appointmentService.GetCountAsync(),
            TodayAppointments = (await _appointmentService.GetByDateAsync(today)).Count(),
            WeekAppointments = (await _appointmentService.GetByDateRangeAsync(weekStart, weekEnd)).Count(),
            TotalRevenue = await _appointmentService.GetTotalRevenueAsync(),
            TodayRevenue = await _appointmentService.GetRevenueByDateAsync(today),
            WeeklyRevenue = await _appointmentService.GetRevenueByDateRangeAsync(weekStart, weekEnd),
            WeekRevenueEstimate = await _appointmentService.GetRevenueByDateRangeAsync(weekStart, weekEnd),
            
            // Top stylists - gerçek veritabanından
            TopStylistsByWeek = (await _stylistService.GetTopStylistsByWeekAsync(5))
                .Select(s => new Models.Admin.Reports.TopStylist { Name = $"{s.FirstName} {s.LastName}", Count = 0 }).ToList(),
            TopStylistsByMonth = (await _stylistService.GetTopStylistsByMonthAsync(5))
                .Select(s => new Models.Admin.Reports.TopStylist { Name = $"{s.FirstName} {s.LastName}", Count = 0 }).ToList(),
            
            // Branch performance - gerçek veritabanından
            BranchPerformance = await GetBranchPerformanceDataAsync(),
            
            // Yeni gelişmiş raporlar
            CustomerAnalytics = await GetCustomerAnalyticsAsync(),
            FinancialAnalytics = await GetFinancialAnalyticsAsync(),
            InventoryAnalytics = await GetInventoryAnalyticsAsync(),
            PerformanceMetrics = await GetPerformanceMetricsAsync()
        };

        return View(vm);
    }

    // GET: /Admin/Reports/Financial
    public async Task<IActionResult> Financial(DateTime? from, DateTime? to)
    {
        var fromDate = from ?? DateTime.Today.AddMonths(-1);
        var toDate = to ?? DateTime.Today;

        var appointments = await _appointmentService.GetByDateRangeAsync(fromDate, toDate);
        var branches = await _branchService.GetAllAsync();
        var stylists = await _stylistService.GetAllAsync();

        var financialData = new FinancialReportViewModel
        {
            Period = $"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}",
            TotalRevenue = appointments.Sum(a => a.FinalPrice),
            TotalAppointments = appointments.Count(),
            AverageTicketValue = appointments.Any() ? appointments.Average(a => a.FinalPrice) : 0,
            BranchRevenues = branches.Select(b => new BranchRevenue
            {
                BranchName = b.Name,
                Revenue = appointments.Where(a => a.BranchId == b.Id).Sum(a => a.FinalPrice),
                AppointmentCount = appointments.Count(a => a.BranchId == b.Id)
            }).ToList(),
            StylistRevenues = stylists.Select(s => new StylistRevenue
            {
                StylistName = $"{s.FirstName} {s.LastName}",
                Revenue = appointments.Where(a => a.StylistId == s.Id).Sum(a => a.FinalPrice),
                AppointmentCount = appointments.Count(a => a.StylistId == s.Id),
                Rating = s.Rating
            }).OrderByDescending(s => s.Revenue).ToList()
        };

        return View(financialData);
    }

    // GET: /Admin/Reports/Customer
    public async Task<IActionResult> Customer()
    {
        var customers = await _customerService.GetAllAsync();
        var appointments = await _appointmentService.GetAllAsync();

        var customerData = new CustomerReportViewModel
        {
            TotalCustomers = customers.Count(),
            NewCustomersThisMonth = customers.Count(c => c.CreatedAt >= DateTime.Today.AddDays(-30)),
            ActiveCustomers = customers.Count(c => appointments.Any(a => a.CustomerId == c.Id && a.StartAt >= DateTime.Today.AddDays(-30))),
            CustomerSegments = GetCustomerSegments(customers, appointments),
            TopCustomers = GetTopCustomers(customers, appointments)
        };

        return View(customerData);
    }

    // GET: /Admin/Reports/Inventory
    public async Task<IActionResult> Inventory()
    {
        var inventoryReport = await _inventoryService.GetInventoryReportAsync();
        var stockMovements = await _inventoryService.GetStockMovementsAsync(DateTime.Today.AddDays(-30), DateTime.Today);
        var inventoryValue = await _inventoryService.GetInventoryValueAsync();

        var inventoryData = new InventoryReportViewModel
        {
            TotalProducts = inventoryReport.Count,
            TotalValue = inventoryValue,
            LowStockProducts = inventoryReport.Count(p => p.StockStatus == "Low"),
            OutOfStockProducts = inventoryReport.Count(p => p.StockStatus == "Out"),
            RecentMovements = stockMovements.Take(20).ToList(),
            CategoryBreakdown = inventoryReport.GroupBy(p => p.Category)
                .Select(g => new CategoryBreakdown
                {
                    Category = g.Key,
                    ProductCount = g.Count(),
                    TotalValue = g.Sum(p => p.TotalValue),
                    AverageStock = g.Average(p => p.CurrentStock)
                }).ToList()
        };

        return View(inventoryData);
    }

    // API: Rapor verilerini JSON olarak döndür
    [HttpGet]
    public async Task<IActionResult> GetReportData(string reportType, DateTime? from, DateTime? to)
    {
        var fromDate = from ?? DateTime.Today.AddDays(-30);
        var toDate = to ?? DateTime.Today;

        switch (reportType.ToLower())
        {
            case "revenue":
                return Json(await GetRevenueChartData(fromDate, toDate));
            case "appointments":
                return Json(await GetAppointmentChartData(fromDate, toDate));
            case "customers":
                return Json(await GetCustomerChartData(fromDate, toDate));
            default:
                return BadRequest("Geçersiz rapor tipi");
        }
    }

    // Export işlemleri
    [HttpPost]
    public IActionResult ExportReport(string reportType, string format, DateTime? from, DateTime? to)
    {
        // TODO: Excel/PDF export implementasyonu
        return Json(new { success = false, message = "Export özelliği yakında eklenecek" });
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
            AverageRating = 4.5 // TODO: Gerçek rating hesaplama
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

        return new FinancialAnalytics
        {
            MonthlyRevenue = appointments.Sum(a => a.FinalPrice),
            AverageTicketValue = appointments.Any() ? appointments.Average(a => a.FinalPrice) : 0,
            RevenueGrowth = 15.5, // TODO: Gerçek büyüme hesaplama
            ProfitMargin = 70.0 // TODO: Gerçek kar marjı hesaplama
        };
    }

    private async Task<InventoryAnalytics> GetInventoryAnalyticsAsync()
    {
        var inventoryReport = await _inventoryService.GetInventoryReportAsync();
        var inventoryValue = await _inventoryService.GetInventoryValueAsync();

        return new InventoryAnalytics
        {
            TotalProducts = inventoryReport.Count,
            TotalValue = inventoryValue,
            LowStockCount = inventoryReport.Count(p => p.StockStatus == "Low"),
            OutOfStockCount = inventoryReport.Count(p => p.StockStatus == "Out"),
            TurnoverRate = 12.5 // TODO: Gerçek devir hızı hesaplama
        };
    }

    private async Task<PerformanceMetrics> GetPerformanceMetricsAsync()
    {
        var appointments = await _appointmentService.GetAllAsync();
        var stylists = await _stylistService.GetAllAsync();

        return new PerformanceMetrics
        {
            AverageServiceTime = 45, // TODO: Gerçek ortalama hizmet süresi
            CustomerSatisfaction = 4.7, // TODO: Gerçek müşteri memnuniyeti
            UtilizationRate = 85.5, // TODO: Gerçek kullanım oranı
            NoShowRate = 5.2 // TODO: Gerçek gelmeme oranı
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

    private List<CustomerSegment> GetCustomerSegments(IEnumerable<Models.Entities.Customer> customers, IEnumerable<Models.Entities.Appointment> appointments)
    {
        // Müşteri segmentasyonu logic'i
        return new List<CustomerSegment>
        {
            new CustomerSegment { Segment = "VIP", Count = customers.Count() / 10, Revenue = 0, Percentage = "10%" },
            new CustomerSegment { Segment = "Sadık", Count = customers.Count() / 3, Revenue = 0, Percentage = "30%" },
            new CustomerSegment { Segment = "Yeni", Count = customers.Count() / 5, Revenue = 0, Percentage = "20%" },
            new CustomerSegment { Segment = "Risk", Count = customers.Count() / 2, Revenue = 0, Percentage = "40%" }
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
    public List<CustomerSegment> CustomerSegments { get; set; } = new();
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

using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class FinancialController : Controller
    {
        private readonly IFinancialAnalyticsService _financialService;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<FinancialController> _logger;

        public FinancialController(
            IFinancialAnalyticsService financialService,
            IAppointmentService appointmentService,
            ILogger<FinancialController> logger)
        {
            _financialService = financialService;
            _appointmentService = appointmentService;
            _logger = logger;
        }

        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            // Finansal dashboard sayfası
            try
            {
                var startDate = DateTime.Now.AddMonths(-1);
                var endDate = DateTime.Now;
                
                var viewModel = new FinancialDashboardViewModel
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalRevenue = await _appointmentService.GetRevenueByDateRangeAsync(startDate, endDate),
                    TotalAppointments = (await _appointmentService.GetByDateRangeAsync(startDate, endDate)).Count(),
                    // Diğer istatistikler buraya eklenebilir
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Finansal dashboard yüklenirken hata oluştu");
                return View(new FinancialDashboardViewModel());
            }
        }

        [HttpGet("cash-flow")]
        [Route("CashFlow")]
        public IActionResult CashFlow(DateTime? startDate, DateTime? endDate)
        {
            // Nakit akış raporu sayfası
            try
            {
                var start = startDate ?? DateTime.Now.AddMonths(-3);
                var end = endDate ?? DateTime.Now;

                var viewModel = new CashFlowViewModel
                {
                    StartDate = start,
                    EndDate = end,
                    // Nakit akış verileri buraya eklenecek
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Nakit akış raporu yüklenirken hata oluştu");
                return View(new CashFlowViewModel());
            }
        }

        [HttpGet("budget-management")]
        [Route("BudgetManagement")]
        public async Task<IActionResult> BudgetManagement()
        {
            // Bütçe yönetimi sayfası
            try
            {
                var budgets = await _financialService.GetAllBudgetsAsync();
                return View(budgets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bütçe yönetimi sayfası yüklenirken hata oluştu");
                return View(new List<Budget>());
            }
        }

        [HttpGet("expense-tracking")]
        [Route("ExpenseTracking")]
        public IActionResult ExpenseTracking()
        {
            // Gider takibi sayfası
            try
            {
                var startDate = DateTime.Now.AddMonths(-1);
                var endDate = DateTime.Now;

                var viewModel = new ExpenseTrackingViewModel
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    // Gider verileri buraya eklenecek
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gider takibi sayfası yüklenirken hata oluştu");
                return View(new ExpenseTrackingViewModel());
            }
        }

        [HttpPost("create-budget")]
        public async Task<IActionResult> CreateBudget([FromBody] Budget budget)
        {
            // Yeni bütçe oluşturur
            try
            {
                var createdBudget = await _financialService.CreateBudgetAsync(budget);
                return Json(new { success = true, budgetId = createdBudget.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bütçe oluşturulurken hata oluştu");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("api/revenue-trend")]
        public async Task<IActionResult> GetRevenueTrend(DateTime startDate, DateTime endDate)
        {
            // Gelir trendi verilerini JSON olarak döndürür
            try
            {
                var appointments = await _appointmentService.GetByDateRangeAsync(startDate, endDate);
                var dailyRevenue = appointments
                    .GroupBy(a => a.StartAt.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        Revenue = g.Sum(a => a.FinalPrice),
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date);

                return Json(new { success = true, data = dailyRevenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gelir trendi verileri alınırken hata oluştu");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // ViewModel sınıfları
    public class FinancialDashboardViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalAppointments { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal AverageTicketValue => TotalAppointments > 0 ? TotalRevenue / TotalAppointments : 0;
        public decimal NetProfit => TotalRevenue - TotalExpenses;
    }

    public class CashFlowViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetCashFlow => TotalIncome - TotalExpenses;
    }

    public class ExpenseTrackingViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ExpenseItem> Expenses { get; set; } = new();
    }

    public class ExpenseItem
    {
        public DateTime Date { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}

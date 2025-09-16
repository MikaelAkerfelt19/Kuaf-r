using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Admin;
using BranchPerformanceAdmin = Kuafor.Web.Models.Admin.BranchPerformance;

namespace Kuafor.Web.Areas.Admin.Controllers;

    [Area("Admin")]
[Route("Admin/[controller]")]
    public class DashboardController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IStylistService _stylistService;
        private readonly IBranchService _branchService;
        private readonly ICustomerService _customerService;
    private readonly IReportingService _reportingService;

        public DashboardController(
            IAppointmentService appointmentService,
            IStylistService stylistService,
            IBranchService branchService,
            ICustomerService customerService,
        IReportingService reportingService)
        {
            _appointmentService = appointmentService;
            _stylistService = stylistService;
            _branchService = branchService;
            _customerService = customerService;
        _reportingService = reportingService;
        }

    // GET: /Admin/Dashboard
        [HttpGet]
    [Route("")]
    [Route("Index")]
        public async Task<IActionResult> Index()
        {
        try
        {
            // Dashboard raporunu al
            var dashboardReport = await _reportingService.GetDashboardReportAsync();
            
            // AdminDashboardViewModel oluştur
            var viewModel = new AdminDashboardViewModel
            {
                TotalAppointments = dashboardReport.TotalAppointments,
                TotalRevenue = dashboardReport.TotalRevenue,
                ActiveStylists = dashboardReport.ActiveStylists,
                ActiveBranches = dashboardReport.ActiveBranches,
                TodayAppointments = 0,
                TodayRevenue = 0,
                WeekAppointments = 0,
                WeekRevenue = 0,
                RecentAppointments = new List<object>(),
                TopStylists = new List<StylistPerformance>(),
                BranchPerformance = new List<BranchPerformanceAdmin>()
            };

            // Bugün ve bu hafta için ayrı hesaplamalar
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var weekEnd = weekStart.AddDays(6);

            var todayAppointments = await _appointmentService.GetByDateRangeAsync(today, today.AddDays(1));
            var weekAppointments = await _appointmentService.GetByDateRangeAsync(weekStart, weekEnd);
            
            viewModel.TodayAppointments = todayAppointments.Count();
            viewModel.WeekAppointments = weekAppointments.Count();
            viewModel.TodayRevenue = todayAppointments.Sum(a => a.FinalPrice);
            viewModel.WeekRevenue = weekAppointments.Sum(a => a.FinalPrice);

            return View(viewModel);
        }
        catch (Exception)
        {
            // Hata durumunda boş model ile view döndür
            var viewModel = new AdminDashboardViewModel
            {
                TotalAppointments = 0,
                TotalRevenue = 0,
                ActiveStylists = 0,
                ActiveBranches = 0,
                TodayAppointments = 0,
                TodayRevenue = 0,
                WeekAppointments = 0,
                WeekRevenue = 0,
                RecentAppointments = new List<object>(),
                TopStylists = new List<StylistPerformance>(),
                BranchPerformance = new List<BranchPerformanceAdmin>()
            };

            return View(viewModel);
        }
    }
}
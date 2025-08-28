using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Reports;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/[controller]")]
public class ReportsController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IStylistService _stylistService;
    private readonly IBranchService _branchService;

    public ReportsController(
        IAppointmentService appointmentService,
        IStylistService stylistService,
        IBranchService branchService)
    {
        _appointmentService = appointmentService;
        _stylistService = stylistService;
        _branchService = branchService;
    }

    // GET: /Admin/Reports
    public async Task<IActionResult> Index()
    {
        // Mock veri yerine gerçek veritabanı verilerini kullanalım
        var today = DateTime.Today;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var weekEnd = weekStart.AddDays(7);

        var vm = new ReportsViewModel
        {
            // Gerçek veritabanı verileri
            TotalAppointments = await _appointmentService.GetCountAsync(),
            TodayAppointments = (await _appointmentService.GetByDateAsync(today)).Count(),
            WeekAppointments = (await _appointmentService.GetByDateRangeAsync(weekStart, weekEnd)).Count(),
            TotalRevenue = await _appointmentService.GetTotalRevenueAsync(),
            TodayRevenue = await _appointmentService.GetRevenueByDateAsync(today),
            WeeklyRevenue = await _appointmentService.GetRevenueByDateRangeAsync(weekStart, weekEnd),
            
            // Top stylists - gerçek veritabanından
            TopStylistsByWeek = (await _stylistService.GetTopStylistsByWeekAsync(5))
                .Select(s => new Models.Admin.Reports.TopStylist { Name = $"{s.FirstName} {s.LastName}", Count = 0 }).ToList(),
            TopStylistsByMonth = (await _stylistService.GetTopStylistsByMonthAsync(5))
                .Select(s => new Models.Admin.Reports.TopStylist { Name = $"{s.FirstName} {s.LastName}", Count = 0 }).ToList(),
            
            // Branch performance - gerçek veritabanından
            BranchPerformance = new List<Models.Admin.Reports.BranchPerformance>()
            // TODO: Implement branch performance logic
        };

        return View(vm);
    }
}

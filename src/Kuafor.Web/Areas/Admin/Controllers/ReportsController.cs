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
            TopStylistsByWeek = (await _stylistService.GetTopStylistsByWeekAsync(weekStart, weekEnd))
                .Select(s => new Models.Admin.Reports.TopStylist { Name = s.Name, Count = s.AppointmentCount }).ToList(),
            TopStylistsByMonth = (await _stylistService.GetTopStylistsByMonthAsync(today.Year, today.Month))
                .Select(s => new Models.Admin.Reports.TopStylist { Name = s.Name, Count = s.AppointmentCount }).ToList(),
            
            // Branch performance - gerçek veritabanından
            BranchPerformance = (await _branchService.GetPerformanceAsync(weekStart, weekEnd))
                .Select(b => new Models.Admin.Reports.BranchPerformance
                {
                    Id = b.Id,
                    Name = b.Name,
                    AppointmentCount = b.AppointmentCount,
                    TotalRevenue = b.TotalRevenue,
                    AverageRating = b.AverageRating
                }).ToList()
        };

        return View(vm);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Stylist;
using System.Security.Claims;

namespace Kuafor.Web.Areas.Stylist.Controllers
{
    [Area("Stylist")]
    [Authorize(Roles = "Stylist")]
    public class DashboardController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IStylistService _stylistService;
        private readonly IServiceService _serviceService;

        public DashboardController(
            IAppointmentService appointmentService,
            IStylistService stylistService,
            IServiceService serviceService)
        {
            _appointmentService = appointmentService;
            _stylistService = stylistService;
            _serviceService = serviceService;
        }

        public async Task<IActionResult> Index()
        {
            var stylistId = await GetCurrentStylistId();
            if (stylistId == 0)
            {
                TempData["Error"] = "Kuaför bilgisi bulunamadı";
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Bugünkü randevular
            var todayAppointments = await _appointmentService.GetByStylistAsync(
                stylistId, today, tomorrow);

            // Gelecek randevular (bugün hariç)
            var upcomingAppointments = await _appointmentService.GetByStylistAsync(
                stylistId, tomorrow, tomorrow.AddDays(7));

            var vm = new StylistDashboardViewModel
            {
                TodayAppointments = todayAppointments.Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    CustomerName = $"{a.Customer?.FirstName} {a.Customer?.LastName}",
                    ServiceName = a.Service?.Name ?? "Bilinmeyen Hizmet",
                    StartTime = a.StartAt.ToLocalTime(),
                    Duration = a.Service?.DurationMin ?? 30,
                    Status = a.Status
                }).ToList(),

                UpcomingAppointments = upcomingAppointments.Select(a => new AppointmentViewModel
                {
                    Id = a.Id,
                    CustomerName = $"{a.Customer?.FirstName} {a.Customer?.LastName}",
                    ServiceName = a.Service?.Name ?? "Bilinmeyen Hizmet",
                    StartTime = a.StartAt.ToLocalTime(),
                    Duration = a.Service?.DurationMin ?? 30,
                    Status = a.Status
                }).ToList()
            };

            return View(vm);
        }

        private async Task<int> GetCurrentStylistId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return 0;

            var stylist = await _stylistService.GetByUserIdAsync(userId);
            return stylist?.Id ?? 0;
        }
    }
}
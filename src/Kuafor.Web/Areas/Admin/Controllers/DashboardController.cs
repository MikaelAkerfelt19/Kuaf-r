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

        public DashboardController(
            IAppointmentService appointmentService,
            IServiceService serviceService,
            IStylistService stylistService)
        {
            _appointmentService = appointmentService;
            _serviceService = serviceService;
            _stylistService = stylistService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var weekEnd = weekStart.AddDays(7);

            var todayAppointments = await _appointmentService.GetByDateRangeAsync(today, today.AddDays(1));
            var weekAppointments = await _appointmentService.GetByDateRangeAsync(weekStart, weekEnd);
            var activeStylists = await _stylistService.GetActiveAsync();
            var topServices = await GetTopServicesAsync(7);

            var vm = new AdminDashboardViewModel
            {
                Kpis =
                {
                    new KpiVm { Label = "Bugün Randevu", Value = todayAppointments.Count().ToString(), SubText = $"Son 7g ort: {weekAppointments.Count() / 7}", Badge = "+12%" },
                    new KpiVm { Label = "Haftalık Randevu", Value = weekAppointments.Count().ToString(), SubText = "Hedef: 120", Badge = "-6%" },
                    new KpiVm { Label = "Aktif Kuaför", Value = activeStylists.Count().ToString(), SubText = $"Toplam {activeStylists.Count()}" },
                    new KpiVm { Label = "Bugünkü Ciro", Value = "₺" + todayAppointments.Sum(a => a.Service?.Price ?? 0).ToString(), SubText = "Tahmini", Badge = "+5%" }
                },
                Upcoming = todayAppointments.Take(3).Select(a => new UpcomingApptRow(
                    a.Id, a.StartAt, $"{a.Customer.FirstName} {a.Customer.LastName}", 
                    a.Service.Name, $"{a.Stylist.FirstName} {a.Stylist.LastName}", a.Branch.Name)).ToList(),
                TopServices = topServices.Select(s => new TopServiceRow(s.Name, s.AppointmentCount, "+")).ToList()
            };
            
            return View(vm);
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
    }

    public class ServiceStats
    {
        public string Name { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kuafor.Web.Models;
using Kuafor.Web.Services.Interfaces;
using System.Security.Claims;

namespace Kuafor.Web.Areas.Customer.Controllers
{
    // Not: Bu controller sadece dashboard görünümünü göstermek içindir.
    // ViewModel, veri erişimi vb. kısımlar eklenecek.
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class HomeController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IServiceService _serviceService;
        private readonly ICustomerService _customerService;

        public HomeController(IAppointmentService appointmentService, IServiceService serviceService, ICustomerService customerService)
        {
            _appointmentService = appointmentService;
            _serviceService = serviceService;
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var customerId = await GetCurrentCustomerIdAsync();
            var upcomingAppointments = await _appointmentService.GetUpcomingAsync(customerId);

            var vm = new CustomerDashboardViewModel
            {
                UpcomingAppointments = upcomingAppointments.Select(a => new AppointmentSummaryVm
                {
                    Id = a.Id,
                    StartAt = a.StartAt,
                    EndAt = a.EndAt,
                    ServiceName = a.Service.Name,
                    StylistName = $"{a.Stylist.FirstName} {a.Stylist.LastName}",
                    BranchName = a.Branch.Name,
                    Status = a.Status.ToString()
                }).ToList(),
                TotalAppointments = upcomingAppointments.Count()
            };

            return View(vm);
        }

        private async Task<int> GetCurrentCustomerIdAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return 0;

            var customer = await _customerService.GetByUserIdAsync(userId);
            return customer?.Id ?? 0;
        }
    }
}
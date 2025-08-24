using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Appointments;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IServiceService _serviceService;
        private readonly IStylistService _stylistService;
        private readonly ICustomerService _customerService;
        private readonly IBranchService _branchService;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IServiceService serviceService,
            IStylistService stylistService,
            ICustomerService customerService,
            IBranchService branchService)
        {
            _appointmentService = appointmentService;
            _serviceService = serviceService;
            _stylistService = stylistService;
            _customerService = customerService;
            _branchService = branchService;
        }

        // GET: /Admin/Appointments
        public async Task<IActionResult> Index()
        {
            var appointments = await _appointmentService.GetAllAsync();
            
            // Mock referans verileri yerine gerçek veritabanı verilerini kullanalım
            var services = await _serviceService.GetAllAsync();
            var stylists = await _stylistService.GetAllAsync();
            var customers = await _customerService.GetAllAsync();
            var branches = await _branchService.GetAllAsync();

            var vm = appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                StartAt = a.StartAt,
                DurationMin = 30, // Default duration
                CustomerName = $"{a.Customer?.FirstName} {a.Customer?.LastName}",
                ServiceName = a.Service?.Name ?? "",
                Status = a.Status,
                Price = a.TotalPrice,
                Note = a.Notes,
                BranchId = a.BranchId,
                StylistId = a.StylistId
            }).ToList();

            ViewBag.Services = services;
            ViewBag.Stylists = stylists;
            ViewBag.Customers = customers;
            ViewBag.Branches = branches;

            return View(vm);
        }

        // POST: /Admin/Appointments/Reschedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(RescheduleForm form)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return Redirect("/Admin/Appointments");
            }

            try
            {
                var appointment = await _appointmentService.RescheduleAsync(form.Id, form.NewStartAt);
                TempData["Success"] = "Randevu başarıyla yeniden planlandı.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }

            return Redirect("/Admin/Appointments");
        }

        // POST: /Admin/Appointments/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason = null)
        {
            try
            {
                var appointment = await _appointmentService.CancelAsync(id, reason);
                TempData["Success"] = "Randevu başarıyla iptal edildi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }

            return Redirect("/Admin/Appointments");
        }

        // POST: /Admin/Appointments/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var appointment = await _appointmentService.GetByIdAsync(id);
                if (appointment != null)
                {
                    appointment.Status = status;
                    appointment.UpdatedAt = DateTime.UtcNow;
                    await _appointmentService.UpdateAsync(appointment);
                    TempData["Success"] = "Randevu durumu güncellendi.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }

            return Redirect("/Admin/Appointments");
        }

        // GET: /Admin/Appointments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: /Admin/Appointments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            ViewBag.Services = await _serviceService.GetAllAsync();
            ViewBag.Stylists = await _stylistService.GetAllAsync();
            ViewBag.Customers = await _customerService.GetAllAsync();
            ViewBag.Branches = await _branchService.GetAllAsync();

            return View(appointment);
        }

        // POST: /Admin/Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _appointmentService.UpdateAsync(appointment);
                    TempData["Success"] = "Randevu başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Hata: {ex.Message}";
                }
            }

            ViewBag.Services = await _serviceService.GetAllAsync();
            ViewBag.Stylists = await _stylistService.GetAllAsync();
            ViewBag.Customers = await _customerService.GetAllAsync();
            ViewBag.Branches = await _branchService.GetAllAsync();

            return View(appointment);
        }

        // POST: /Admin/Appointments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _appointmentService.DeleteAsync(id);
                TempData["Success"] = "Randevu başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

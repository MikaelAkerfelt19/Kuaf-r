using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Appointments;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Enums;

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
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var appointments = await _appointmentService.GetAllAsync();
            
            // Mock referans verileri yerine gerçek veritabanı verilerini kullanalım
            var services = await _serviceService.GetAllAsync();
            var stylists = await _stylistService.GetAllAsync();
            var customers = await _customerService.GetAllAsync();
            var branches = await _branchService.GetAllAsync();

            var appointmentDtos = appointments.Select(a => new AppointmentDto
            {
                Id = a.Id,
                StartAt = a.StartAt,
                DurationMin = 30, // Default duration
                CustomerName = $"{a.Customer?.FirstName} {a.Customer?.LastName}",
                ServiceName = a.Service?.Name ?? "",
                Status = a.Status.ToString(),
                Price = a.TotalPrice,
                Note = a.Notes,
                BranchId = a.BranchId,
                StylistId = a.StylistId
            }).ToList();

            // AppointmentsPageViewModel oluştur
            var viewModel = new AppointmentsPageViewModel
            {
                Items = appointmentDtos,
                Filter = new AppointmentFilter(), // Varsayılan filtre
                BranchNames = branches.ToDictionary(b => b.Id, b => b.Name),
                StylistNames = stylists.ToDictionary(s => s.Id, s => $"{s.FirstName} {s.LastName}")
            };

            return View(viewModel);
        }

        // POST: /Admin/Appointments/Reschedule
        [HttpGet]
        [Route("Reschedule")]   
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
        [Route("Cancel")]
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

        // GET: /Admin/Appointments/Cancel/5 - Confirmation sayfası
        [HttpGet]
        [Route("Cancel/{id:int}")]
        public async Task<IActionResult> Cancel(int id)
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

        // POST: /Admin/Appointments/Cancel/5 - Gerçek iptal işlemi
        [HttpPost]
        [Route("Cancel/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id, string? reason = null)
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

            return RedirectToAction(nameof(Index));
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
                    // String status'u enum'a çevir
                    if (Enum.TryParse<AppointmentStatus>(status, out var statusEnum))
                    {
                        appointment.Status = statusEnum;
                        appointment.UpdatedAt = DateTime.UtcNow;
                        await _appointmentService.UpdateAsync(appointment);
                        TempData["Success"] = $"Randevu durumu '{status}' olarak güncellendi.";
                    }
                    else
                    {
                        TempData["Error"] = "Geçersiz durum değeri.";
                    }
                }
                else
                {
                    TempData["Error"] = "Randevu bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Appointments/Details/5
        [HttpGet]
        [Route("Details/{id:int}")] 
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
        [HttpGet]
        [Route("Edit/{id:int}")] 
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

        // GET: /Admin/Appointments/Delete/5 - Confirmation sayfası
        [HttpGet]
        [Route("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
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

        // POST: /Admin/Appointments/DeleteConfirmed
        [HttpPost]
        [Route("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string? reason = null)
        {
            try
            {
                var appointment = await _appointmentService.GetByIdAsync(id);
                if (appointment == null)
                {
                    TempData["Error"] = "Randevu bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // İptal edilmiş randevuları silmeye izin ver
                if (appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.Completed)
                {
                    await _appointmentService.DeleteAsync(id);
                    TempData["Success"] = "Randevu başarıyla silindi.";
                }
                else
                {
                    TempData["Error"] = "Sadece iptal edilmiş veya tamamlanmış randevular silinebilir.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = $"Randevu silinemedi: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Beklenmeyen hata: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Appointments/Delete/5 (eski endpoint - geriye uyumluluk için)
        [HttpPost]
        [Route("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? reason = null)
        {
            return await DeleteConfirmed(id, reason);
        }
    }
}

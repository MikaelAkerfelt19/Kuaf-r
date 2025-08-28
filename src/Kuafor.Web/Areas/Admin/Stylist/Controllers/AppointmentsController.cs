using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Stylist;
using Kuafor.Web.Models.Enums;
using System.Security.Claims;

namespace Kuafor.Web.Areas.Stylist.Controllers
{
    [Area("Stylist")]
    [Authorize(Roles = "Stylist")]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IStylistService _stylistService;
        private readonly IServiceService _serviceService;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IStylistService stylistService,
            IServiceService serviceService)
        {
            _appointmentService = appointmentService;
            _stylistService = stylistService;
            _serviceService = serviceService;
        }

        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
            {
                TempData["Error"] = "Randevu bulunamadı";
                return RedirectToAction("Index", "Dashboard");
            }

            var stylistId = await GetCurrentStylistId();
            if (appointment.StylistId != stylistId)
            {
                TempData["Error"] = "Bu randevuyu görüntüleme yetkiniz yok";
                return RedirectToAction("Index", "Dashboard");
            }

            var vm = new AppointmentDetailViewModel
            {
                Id = appointment.Id,
                CustomerName = $"{appointment.Customer?.FirstName} {appointment.Customer?.LastName}",
                CustomerPhone = appointment.Customer?.Phone,
                CustomerEmail = appointment.Customer?.Email,
                ServiceName = appointment.Service?.Name ?? "Bilinmeyen Hizmet",
                StartTime = appointment.StartAt.ToLocalTime(),
                Duration = appointment.Service?.DurationMin ?? 30,
                Status = appointment.Status.ToString(),
                Notes = appointment.Notes
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetByIdAsync(id);
                if (appointment == null)
                {
                    TempData["Error"] = "Randevu bulunamadı";
                    return RedirectToAction("Index", "Dashboard");
                }

                var stylistId = await GetCurrentStylistId();
                if (appointment.StylistId != stylistId)
                {
                    TempData["Error"] = "Bu randevuyu tamamlama yetkiniz yok";
                    return RedirectToAction("Index", "Dashboard");
                }

                appointment.Status = AppointmentStatus.Completed;
                await _appointmentService.UpdateAsync(appointment);

                TempData["Success"] = "Randevu başarıyla tamamlandı";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Randevu tamamlanamadı: " + ex.Message;
                return RedirectToAction("Index", "Dashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id, string reason)
        {
            try
            {
                await _appointmentService.CancelAsync(id, reason);
                TempData["Success"] = "Randevu iptal edildi";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Randevu iptal edilemedi: " + ex.Message;
            }
            return RedirectToAction("Index", "Dashboard");
        }

        private async Task<int> GetCurrentStylistId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return 0;

            var stylist = await _stylistService.GetByUserIdAsync(userId);
            return stylist?.Id ?? 0;
        }

        // Bu hizmeti sunan kuaförleri bul
        public async Task<IEnumerable<Models.Entities.Stylist>> GetByServiceAsync(int serviceId)
        {
            return await _stylistService.GetByServiceAsync(serviceId);
        }
    }
}

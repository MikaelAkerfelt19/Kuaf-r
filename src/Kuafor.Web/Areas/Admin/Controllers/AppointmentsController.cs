using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Appointments;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Enums;
using OfficeOpenXml;

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

        [HttpPost("create-recurring")]
        public async Task<IActionResult> CreateRecurringAppointment([FromBody] RecurringAppointmentRequest request)
        {
            // Tekrarlayan randevu oluşturur
            try
            {
                var baseAppointment = await _appointmentService.GetByIdAsync(request.BaseAppointmentId);
                if (baseAppointment != null)
                {
                    await _appointmentService.CreateRepeatingAppointmentAsync(baseAppointment, request.RepeatCount, request.RepeatType);
                    return Json(new { success = true, message = "Tekrarlayan randevular oluşturuldu" });
                }
                return Json(new { success = false, message = "Temel randevu bulunamadı" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("available-slots")]
        public async Task<IActionResult> GetAvailableSlots(int stylistId, DateTime date, int serviceDuration = 60)
        {
            // Uygun zaman dilimlerini getirir
            try
            {
                var slots = await _appointmentService.GetAvailableTimeSlotsAsync(stylistId, date, serviceDuration);
                return Json(new { success = true, slots = slots });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics(DateTime startDate, DateTime endDate)
        {
            // Randevu istatistiklerini getirir
            try
            {
                var stats = await _appointmentService.GetAppointmentStatisticsAsync(startDate, endDate);
                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportToExcel(DateTime? startDate, DateTime? endDate)
        {
            // Randevuları Excel olarak export eder
            try
            {
                var start = startDate ?? DateTime.Now.AddMonths(-1);
                var end = endDate ?? DateTime.Now;
                
                var appointments = await _appointmentService.GetByDateRangeAsync(start, end);
                var stream = await GenerateAppointmentsExcel(appointments);
                
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                           $"randevular_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Excel export hatası: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // Yardımcı metodlar
        private async Task<MemoryStream> GenerateAppointmentsExcel(IEnumerable<Appointment> appointments)
        {
            // Excel dosyası oluşturur ve randevu verilerini yazar
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Randevular");
            
            // Header yazma
            worksheet.Cells[1, 1].Value = "Tarih";
            worksheet.Cells[1, 2].Value = "Saat";
            worksheet.Cells[1, 3].Value = "Müşteri";
            worksheet.Cells[1, 4].Value = "Hizmet";
            worksheet.Cells[1, 5].Value = "Kuaför";
            worksheet.Cells[1, 6].Value = "Durum";
            worksheet.Cells[1, 7].Value = "Fiyat";
            worksheet.Cells[1, 8].Value = "Notlar";
            
            int row = 2;
            foreach (var appointment in appointments)
            {
                worksheet.Cells[row, 1].Value = appointment.StartAt.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 2].Value = appointment.StartAt.ToString("HH:mm");
                worksheet.Cells[row, 3].Value = appointment.Customer?.FullName ?? "";
                worksheet.Cells[row, 4].Value = appointment.Service?.Name ?? "";
                worksheet.Cells[row, 5].Value = appointment.Stylist?.Name ?? "";
                worksheet.Cells[row, 6].Value = appointment.Status.ToString();
                worksheet.Cells[row, 7].Value = appointment.FinalPrice;
                worksheet.Cells[row, 8].Value = appointment.Notes ?? "";
                row++;
            }
            
            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();
            
            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);
            stream.Position = 0;
            return stream;
        }

        // Yardımcı model sınıfları
        public class RecurringAppointmentRequest
        {
            public int BaseAppointmentId { get; set; }
            public int RepeatCount { get; set; }
            public string RepeatType { get; set; } = "Weekly"; // Weekly, Monthly
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Appointments;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Kuafor.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IServiceService _serviceService;
        private readonly IStylistService _stylistService;
        private readonly IBranchService _branchService;
        private readonly ICustomerService _customerService;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IServiceService serviceService,
            IStylistService stylistService,
            IBranchService branchService,
            ICustomerService customerService)
        {
            _appointmentService = appointmentService;
            _serviceService = serviceService;
            _stylistService = stylistService;
            _branchService = branchService;
            _customerService = customerService;
        }

        public async Task<IActionResult> New(int? serviceId, int? stylistId, string? start)
        {
            var vm = new AppointmentWizardViewModel
            {
                Services = (await _serviceService.GetAllAsync()).Select(s => new ServiceVm(s.Id, s.Name, $"{s.DurationMin} dakika", "")).ToList(),
                Stylists = (await _stylistService.GetAllAsync()).Select(s => new StylistVm(s.Id, $"{s.FirstName} {s.LastName}", (double)s.Rating, s.Bio ?? "")).ToList(),
                TimeSlots = await BuildAvailableSlotsAsync(stylistId, serviceId)
            };

            if (serviceId.HasValue) { vm.SelectedServiceId = serviceId; vm.Step = WizardStep.Stylist; }
            if (stylistId.HasValue) { vm.SelectedStylistId = stylistId; vm.Step = WizardStep.Time; }
            if (!string.IsNullOrEmpty(start) && DateTime.TryParse(start, out var dt))
            { vm.SelectedStart = dt; vm.Step = WizardStep.Confirm; }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(int serviceId, int stylistId, DateTime start)
        {
            try
            {
                var service = await _serviceService.GetByIdAsync(serviceId);
                var stylist = await _stylistService.GetByIdAsync(stylistId);
                
                if (service == null || stylist == null)
                    return BadRequest("Geçersiz hizmet veya kuaför");

                var customerId = await GetCurrentCustomerId();
                if (customerId == 0)
                    return BadRequest("Müşteri bilgisi bulunamadı");

                var appointment = new Appointment
                {
                    ServiceId = serviceId,
                    StylistId = stylistId,
                    BranchId = stylist.BranchId,
                    CustomerId = customerId,
                    StartAt = start.ToUniversalTime(),
                    EndAt = start.AddMinutes(service.DurationMin).ToUniversalTime(),
                    Status = AppointmentStatus.Confirmed
                };

                var created = await _appointmentService.CreateAsync(appointment);
                
                TempData["Success"] = $"Randevu oluşturuldu: {start:dd MMM dddd, HH:mm} · {service.Name} · {stylist.FirstName} {stylist.LastName}";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("New", new { serviceId, stylistId, start = start.ToString("yyyy-MM-ddTHH:mm") });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _appointmentService.CancelAsync(id, "Müşteri tarafından iptal edildi");
                TempData["Success"] = "Randevu iptal edildi";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Randevu iptal edilemedi: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == 0)
            {
                TempData["Error"] = "Müşteri bilgisi bulunamadı";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            
            var appointments = await _appointmentService.GetByCustomerAsync(customerId);
            
            var upcoming = appointments.Where(a => a.StartAt > DateTime.UtcNow && a.Status != AppointmentStatus.Cancelled).OrderBy(a => a.StartAt);
            var past = appointments.Where(a => a.StartAt <= DateTime.UtcNow || a.Status == AppointmentStatus.Cancelled).OrderByDescending(a => a.StartAt);

            ViewBag.Upcoming = upcoming;
            ViewBag.Past = past;
            return View();
        }

        // GET: /Customer/Appointments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == 0)
            {
                TempData["Error"] = "Müşteri bilgisi bulunamadı";
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null || appointment.CustomerId != customerId)
            {
                TempData["Error"] = "Randevu bulunamadı";
                return RedirectToAction("Index");
            }

            // Randevu detayları için gerekli bilgileri yükle
            var service = await _serviceService.GetByIdAsync(appointment.ServiceId);
            var stylist = await _stylistService.GetByIdAsync(appointment.StylistId);
            var branch = await _branchService.GetByIdAsync(appointment.BranchId);

            if (service == null || stylist == null || branch == null)
            {
                TempData["Error"] = "Randevu bilgileri eksik";
                return RedirectToAction("Index");
            }

            var vm = new AppointmentDetailViewModel
            {
                Appointment = appointment,
                Service = service,
                Stylist = stylist,
                Branch = branch,
            };

            return View(vm);
        }

        // GET: /Customer/Appointments/Reschedule/5
        public async Task<IActionResult> Reschedule(int id)
        {
            var customerId = await GetCurrentCustomerId();
            if (customerId == 0)
            {
                TempData["Error"] = "Müşteri bilgisi bulunamadı";
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null || appointment.CustomerId != customerId)
            {
                TempData["Error"] = "Randevu bulunamadı";
                return RedirectToAction("Index");
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                TempData["Error"] = "İptal edilmiş randevu yeniden planlanamaz";
                return RedirectToAction("Index");
            }

            // Mevcut zaman dilimlerini al
            var availableSlots = await BuildAvailableSlotsAsync(appointment.StylistId, appointment.ServiceId);
            
            var vm = new RescheduleViewModel
            {
                AppointmentId = id,
                CurrentStartTime = appointment.StartAt,
                CurrentEndTime = appointment.EndAt,
                ServiceName = (await _serviceService.GetByIdAsync(appointment.ServiceId))?.Name ?? "",
                StylistName = ((await _stylistService.GetByIdAsync(appointment.StylistId))?.FirstName ?? "") + " " + 
                             ((await _stylistService.GetByIdAsync(appointment.StylistId))?.LastName ?? ""),
                AvailableTimeSlots = availableSlots
            };

            return View(vm);
        }

        // POST: /Customer/Appointments/Reschedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(RescheduleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return RedirectToAction("Reschedule", new { id = model.AppointmentId });
            }

            try
            {
                var customerId = await GetCurrentCustomerId();
                if (customerId == 0)
                {
                    TempData["Error"] = "Müşteri bilgisi bulunamadı";
                    return RedirectToAction("Index", "Home", new { area = "" });
                }

                var appointment = await _appointmentService.GetByIdAsync(model.AppointmentId);
                if (appointment == null || appointment.CustomerId != customerId)
                {
                    TempData["Error"] = "Randevu bulunamadı";
                    return RedirectToAction("Index");
                }

                // Zaman çakışması kontrolü
                if (await _appointmentService.HasTimeConflictAsync(
                    appointment.StylistId, model.NewStartTime, model.NewStartTime.AddMinutes((int)(appointment.EndAt - appointment.StartAt).TotalMinutes)))
                {
                    TempData["Error"] = "Seçilen zaman diliminde başka bir randevu bulunmaktadır";
                    return RedirectToAction("Reschedule", new { id = model.AppointmentId });
                }

                // Randevuyu güncelle
                appointment.StartAt = model.NewStartTime.ToUniversalTime();
                appointment.EndAt = model.NewStartTime.AddMinutes((int)(appointment.EndAt - appointment.StartAt).TotalMinutes).ToUniversalTime();
                appointment.Status = AppointmentStatus.Rescheduled;
                appointment.UpdatedAt = DateTime.UtcNow;

                await _appointmentService.UpdateAsync(appointment);
                
                TempData["Success"] = "Randevu başarıyla yeniden planlandı";
                return RedirectToAction("Details", new { id = model.AppointmentId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Randevu yeniden planlanamadı: " + ex.Message;
                return RedirectToAction("Reschedule", new { id = model.AppointmentId });
            }
        }

        private async Task<List<TimeSlotVm>> BuildAvailableSlotsAsync(int? stylistId, int? serviceId)
        {
            if (!stylistId.HasValue || !serviceId.HasValue)
                return new List<TimeSlotVm>();

            var service = await _serviceService.GetByIdAsync(serviceId.Value);
            var stylist = await _stylistService.GetByIdAsync(stylistId.Value);
            
            if (service == null || stylist == null)
                return new List<TimeSlotVm>();

            var start = DateTime.Today.AddDays(2).AddHours(10);
            var slots = new List<TimeSlotVm>();
            
            for (int i = 0; i < 16; i++)
            {
                var slotStart = start.AddMinutes(30 * i);
                var slotEnd = slotStart.AddMinutes(service.DurationMin);
                
                var isAvailable = !await _appointmentService.HasTimeConflictAsync(
                    stylistId.Value, slotStart, slotEnd);
                    
                slots.Add(new TimeSlotVm(slotStart, isAvailable));
            }
            
            return slots;
        }

        private async Task<int> GetCurrentCustomerId()
        {
            // Identity User ID'den Customer ID'yi bul
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return 0;

            var customer = await _customerService.GetByUserIdAsync(userId);
            return customer?.Id ?? 0;
        }
    }
}

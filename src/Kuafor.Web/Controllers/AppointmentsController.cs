using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Enums;
using Kuafor.Web.Models.Appointments;
using System.Security.Claims;
using Kuafor.Web.Services;
namespace Kuafor.Web.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IServiceService _serviceService;
    private readonly IStylistService _stylistService;
    private readonly IBranchService _branchService;
    private readonly ICustomerService _customerService;
    private readonly ITimeZoneService _timeZoneService;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IServiceService serviceService,
        IStylistService stylistService,
        IBranchService branchService,
        ICustomerService customerService,
        ITimeZoneService timeZoneService)
    {
        _appointmentService = appointmentService;
        _serviceService = serviceService;
        _stylistService = stylistService;
        _branchService = branchService;
        _customerService = customerService;
        _timeZoneService = timeZoneService;
    }

    // GET: /Appointments
    public async Task<IActionResult> Index()
    {
        var customerId = await GetCurrentCustomerId();
        if (customerId == null)
        {
            TempData["Error"] = "Müşteri bilgileriniz bulunamadı. Lütfen profil bilgilerinizi tamamlayın.";
            return RedirectToAction("Index", "Profile", new { area = "Customer" });
        }

        var appointments = await _appointmentService.GetByCustomerAsync(customerId.Value);
        
        // Timezone düzeltmesi: UTC karşılaştırması yerine tutarlı zaman kullan
        var nowUtc = DateTime.UtcNow;
        var upcoming = appointments.Where(a => a.StartAt > nowUtc && a.Status != AppointmentStatus.Cancelled).OrderBy(a => a.StartAt);
        var past = appointments.Where(a => a.StartAt <= nowUtc || a.Status == AppointmentStatus.Cancelled).OrderByDescending(a => a.StartAt);

        ViewBag.Upcoming = upcoming;
        ViewBag.Past = past;
        return View(appointments);
    }

    // GET: /Appointments/New
    [AllowAnonymous]
    public async Task<IActionResult> New()
    {
        var viewModel = new CreateAppointmentViewModel
        {
            Services = await _serviceService.GetAllAsync(),
            Branches = await _branchService.GetAllAsync()
        };

        return View(viewModel);
    }

    // POST: /Appointments/New
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> New(CreateAppointmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Services = await _serviceService.GetAllAsync();
            model.Branches = await _branchService.GetAllAsync();
            return View(model);
        }

        try
        {
            // Müşteri kontrolü
            var customerId = await GetCurrentCustomerId();
            if (customerId == null)
            {
                TempData["Error"] = "Randevu oluşturmak için giriş yapmanız gerekiyor.";
                return RedirectToAction("Login", "Account");
            }

            // Timezone düzeltmesi: Local time'ı UTC'ye çevir
            var startUtc = _timeZoneService.ConvertToUtc(model.StartAt);
            var endUtc = _timeZoneService.ConvertToUtc(model.StartAt.AddMinutes(model.DurationMin));

            // Çakışma kontrolü - UTC zamanları ile
            var existingAppointments = await _appointmentService.GetByStylistAsync(
                model.StylistId, startUtc.Date, startUtc.Date.AddDays(1));
            
            var hasSimpleConflict = existingAppointments.Any(a => 
                a.Status != Models.Enums.AppointmentStatus.Cancelled &&
                a.StartAt < endUtc && a.EndAt > startUtc);

            if (hasSimpleConflict)
            {
                ModelState.AddModelError("", "Seçilen tarih ve saatte kuaför müsait değil. Lütfen başka bir zaman seçin.");
                model.Services = await _serviceService.GetAllAsync();
                model.Branches = await _branchService.GetAllAsync();
                return View(model);
            }

            var appointment = new Appointment
            {
                ServiceId = model.ServiceId,
                StylistId = model.StylistId,
                BranchId = model.BranchId,
                CustomerId = customerId.Value,
                StartAt = startUtc,
                EndAt = endUtc,
                Notes = model.Notes,
                Status = AppointmentStatus.Confirmed
            };

            await _appointmentService.CreateAsync(appointment);

            // Başarı mesajında local time kullan
            var localStart = _timeZoneService.ConvertToLocalTime(appointment.StartAt);
            TempData["Success"] = $"Randevunuz başarıyla oluşturuldu: {localStart:dd MMM dddd, HH:mm} · {model.SelectedServiceName}";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Randevu oluşturulurken bir hata oluştu: " + ex.Message);
            model.Services = await _serviceService.GetAllAsync();
            model.Branches = await _branchService.GetAllAsync();
            return View(model);
        }
    }

    // GET: /Appointments/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null)
        {
            TempData["Error"] = "Randevu bulunamadı.";
            return RedirectToAction("Index");
        }

        var customerId = await GetCurrentCustomerId();
        if (appointment.CustomerId != customerId)
        {
            TempData["Error"] = "Bu randevuyu görüntüleme yetkiniz yok.";
            return RedirectToAction("Index");
        }

        return View(appointment);
    }

    // POST: /Appointments/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null)
        {
            TempData["Error"] = "Randevu bulunamadı.";
            return RedirectToAction("Index");
        }

        var customerId = await GetCurrentCustomerId();
        if (appointment.CustomerId != customerId)
        {
            TempData["Error"] = "Bu randevuyu iptal etme yetkiniz yok.";
            return RedirectToAction("Index");
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            TempData["Warning"] = "Bu randevu zaten iptal edilmiş.";
            return RedirectToAction("Index");
        }

        // Timezone düzeltmesi: Local time ile karşılaştır
        var localStart = _timeZoneService.ConvertToLocalTime(appointment.StartAt);
        if (localStart <= DateTime.Now.AddHours(2))
        {
            TempData["Error"] = "Randevu saatinden 2 saat öncesine kadar iptal edebilirsiniz.";
            return RedirectToAction("Index");
        }

        await _appointmentService.CancelAsync(id);
        TempData["Success"] = "Randevunuz başarıyla iptal edildi.";
        return RedirectToAction("Index");
    }

    // API endpoint for getting available time slots
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableSlots(int stylistId, DateTime date, int durationMin)
    {
        try
        {
            // Timezone düzeltmesi: Local date'i UTC'ye çevir
            var dateUtc = _timeZoneService.ConvertToUtc(date);
            var slots = await _appointmentService.GetAvailableSlotsAsync(stylistId, dateUtc, durationMin);
            return Json(slots);
        }
        catch (Exception)
        {
            return Json(new List<string>());
        }
    }

    // API endpoint for getting stylists by branch
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetStylistsByBranch(int branchId)
    {
        try
        {
            var stylists = await _stylistService.GetByBranchAsync(branchId);
            return Json(stylists.Select(s => new { id = s.Id, name = $"{s.FirstName} {s.LastName}" }));
        }
        catch (Exception)
        {
            return Json(new List<object>());
        }
    }

    private async Task<int?> GetCurrentCustomerId()
    {
        if (User.Identity?.IsAuthenticated != true)
            return null;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return null;

        var customer = await _customerService.GetByUserIdAsync(userId);
        if (customer == null)
        {
            // Customer kaydı yoksa otomatik oluştur
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var userName = User.Identity?.Name ?? "";
            var nameParts = userName.Split(' ');
            
            customer = new Models.Entities.Customer
            {
                UserId = userId,
                FirstName = nameParts.Length > 0 ? nameParts[0] : "Müşteri",
                LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "",
                Email = userEmail,
                Phone = "",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            await _customerService.CreateAsync(customer);
        }

        return customer?.Id;
    }
}

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

    // GET: /Appointments/New - Redirect to Customer Area
    [AllowAnonymous]
    public IActionResult New()
    {
        return RedirectToAction("New", "Appointments", new { area = "Customer" });
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
        return customer?.Id;
    }
}

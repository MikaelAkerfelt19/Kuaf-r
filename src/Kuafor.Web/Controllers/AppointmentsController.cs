using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Appointments;
using System.Security.Claims;

namespace Kuafor.Web.Controllers;

[Authorize]
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

            // Çakışma kontrolü
            var hasConflict = await _appointmentService.HasConflictAsync(
                model.StylistId, model.StartAt, model.StartAt.AddMinutes(model.DurationMin));

            if (hasConflict)
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
                StartAt = model.StartAt.ToUniversalTime(),
                EndAt = model.StartAt.AddMinutes(model.DurationMin).ToUniversalTime(),
                Notes = model.Notes,
                Status = "Scheduled"
            };

            await _appointmentService.CreateAsync(appointment);

            TempData["Success"] = "Randevunuz başarıyla oluşturuldu. Onay için bekleyiniz.";
            return RedirectToAction("Index");
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "Randevu oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
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

        if (appointment.Status == "Cancelled")
        {
            TempData["Warning"] = "Bu randevu zaten iptal edilmiş.";
            return RedirectToAction("Index");
        }

        if (appointment.StartAt <= DateTime.UtcNow.AddHours(2))
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
            var slots = await _appointmentService.GetAvailableSlotsAsync(stylistId, date, durationMin);
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
        if (!User.Identity?.IsAuthenticated == true)
            return null;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return null;

        var customer = await _customerService.GetByUserIdAsync(userId);
        return customer?.Id;
    }
}
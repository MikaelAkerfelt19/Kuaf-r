using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Appointments;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Services;
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
        private readonly IStylistWorkingHoursService _stylistWorkingHoursService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ICouponService _couponService;
        public AppointmentsController(
            IAppointmentService appointmentService,
            IServiceService serviceService,
            IStylistService stylistService,
            IBranchService branchService,
            ICustomerService customerService,
            IStylistWorkingHoursService stylistWorkingHoursService,
            ITimeZoneService timeZoneService,
            ICouponService couponService) 
        {
            _appointmentService = appointmentService;
            _serviceService = serviceService;
            _stylistService = stylistService;
            _branchService = branchService;
            _customerService = customerService;
            _stylistWorkingHoursService = stylistWorkingHoursService;
            _timeZoneService = timeZoneService;
            _couponService = couponService; 
        }

        public async Task<IActionResult> New(int? branchId, int? serviceId, int? stylistId, string? start)
        {
            var vm = new AppointmentWizardViewModel
            {
                Branches = (await _branchService.GetAllAsync()).Select(b => new BranchVm(b.Id, b.Name, b.Address ?? "")).ToList(),
                Services = (await _serviceService.GetAllAsync()).Select(s => new ServiceVm(s.Id, s.Name, $"{s.DurationMin} dakika", s.Description ?? "", s.Price)).ToList(),
                Stylists = (await _stylistService.GetAllAsync()).Select(s => new StylistVm(s.Id, $"{s.FirstName} {s.LastName}", (double)s.Rating, s.Bio ?? "", s.BranchId)).ToList(),
                TimeSlots = await BuildAvailableSlotsAsync(stylistId, serviceId)
            };

            // Step belirleme mantığı
            if (branchId.HasValue) 
            { 
                vm.SelectedBranchId = branchId; 
                vm.Stylists = vm.Stylists.Where(s => s.BranchId == branchId).ToList(); // Şubeye göre filtrele
                vm.Step = WizardStep.Service; 
            }
            if (serviceId.HasValue) { vm.SelectedServiceId = serviceId; vm.Step = WizardStep.Stylist; }
            if (stylistId.HasValue) { vm.SelectedStylistId = stylistId; vm.Step = WizardStep.Time; }
            if (!string.IsNullOrEmpty(start) && DateTime.TryParse(start, out var dt))
            { 
                // DateTime'ı local time olarak işaretle
                if (dt.Kind == DateTimeKind.Unspecified)
                {
                    dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
                }
                vm.SelectedStart = dt; 
                vm.Step = WizardStep.Confirm; 
                Console.WriteLine($"DEBUG: Selected start time: {dt:yyyy-MM-dd HH:mm:ss} (Kind: {dt.Kind})");
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(int branchId, int serviceId, int stylistId, DateTime start, string? notes, string? couponCode, decimal discountAmount = 0)
        {
            try
            {
                var service = await _serviceService.GetByIdAsync(serviceId);
                var stylist = await _stylistService.GetByIdAsync(stylistId);
                var branch = await _branchService.GetByIdAsync(branchId);
                
                if (service == null || stylist == null || branch == null)
                    return BadRequest("Geçersiz hizmet, kuaför veya şube");

                var customerId = await GetCurrentCustomerId();
                if (customerId == 0)
                    return BadRequest("Müşteri bilgisi bulunamadı");

                // Timezone dönüşümü düzeltildi
                // DateTime'ın Kind property'sini kontrol et
                if (start.Kind == DateTimeKind.Unspecified)
                {
                    start = DateTime.SpecifyKind(start, DateTimeKind.Local);
                }
                
                var startUtc = _timeZoneService.ConvertToUtc(start);
                var endUtc = _timeZoneService.ConvertToUtc(start.AddMinutes(service.DurationMin));

                // Fiyat hesaplama
                var totalPrice = service.Price;
                var finalPrice = totalPrice - discountAmount;

                var appointment = new Appointment
                {
                    ServiceId = serviceId,
                    StylistId = stylistId,
                    BranchId = branchId,
                    CustomerId = customerId,
                    StartAt = startUtc,
                    EndAt = endUtc,
                    Notes = notes,
                    Status = AppointmentStatus.Confirmed,
                    TotalPrice = totalPrice,
                    DiscountAmount = discountAmount,
                    FinalPrice = finalPrice
                };

                var created = await _appointmentService.CreateAsync(appointment);
                
                // Kupon uygulandıysa kupon kullanımını kaydet
                if (!string.IsNullOrEmpty(couponCode) && discountAmount > 0)
                {
                    try
                    {
                        var coupon = await _couponService.GetByCodeAsync(couponCode);
                        if (coupon != null)
                        {
                            await _couponService.ApplyCouponAsync(
                                coupon.Id,
                                customerId,
                                created.Id,
                                discountAmount,
                                "Randevu oluşturma sırasında uygulandı"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        // Kupon uygulama hatası randevu oluşturmayı engellemez
                        Console.WriteLine($"Kupon uygulama hatası: {ex.Message}");
                    }
                }
                
                // Başarı mesajında local time kullan
                var localStart = _timeZoneService.ConvertToLocalTime(created.StartAt);
                var successMessage = $"Randevu oluşturuldu: {localStart:dd MMM dddd, HH:mm} · {service.Name} · {stylist.FirstName} {stylist.LastName} · {branch.Name}";
                
                if (discountAmount > 0)
                {
                    successMessage += $" · İndirim: {discountAmount:C}";
                }
                
                TempData["Success"] = successMessage;
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("New", new { branchId, serviceId, stylistId, start = start.ToString("yyyy-MM-ddTHH:mm") });
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
            
            // Timezone düzeltmesi: UTC karşılaştırması yerine local time kullan
            var nowUtc = DateTime.UtcNow;
            var upcoming = appointments.Where(a => a.StartAt > nowUtc && a.Status != AppointmentStatus.Cancelled).OrderBy(a => a.StartAt);
            var past = appointments.Where(a => a.StartAt <= nowUtc || a.Status == AppointmentStatus.Cancelled).OrderByDescending(a => a.StartAt);

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

                // Zaman çakışması kontrolü - UTC dönüşümü ile
                var newStartUtc = _timeZoneService.ConvertToUtc(model.NewStartTime);
                var newEndUtc = _timeZoneService.ConvertToUtc(model.NewStartTime.AddMinutes((int)(appointment.EndAt - appointment.StartAt).TotalMinutes));
                
                if (await _appointmentService.HasTimeConflictAsync(
                    appointment.StylistId, newStartUtc, newEndUtc))
                {
                    TempData["Error"] = "Seçilen zaman diliminde başka bir randevu bulunmaktadır";
                    return RedirectToAction("Reschedule", new { id = model.AppointmentId });
                }

                // Randevuyu güncelle - UTC dönüşümü ile
                appointment.StartAt = newStartUtc;
                appointment.EndAt = newEndUtc;
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

            var slots = new List<TimeSlotVm>();
            var startDate = DateTime.Today.AddDays(1);
            var endDate = startDate.AddDays(7);
            
            // Performans optimizasyonu: Önce tüm çalışma saatlerini ve mevcut randevuları al
            var allWorkingHours = await _stylistWorkingHoursService.GetByStylistAsync(stylistId.Value);
            var existingAppointments = await _appointmentService.GetByDateRangeAsync(startDate, endDate.AddDays(1));
            var stylistAppointments = existingAppointments
                .Where(a => a.StylistId == stylistId.Value && a.Status != Models.Enums.AppointmentStatus.Cancelled)
                .ToList();
            
            // 7 gün için slot oluştur
            for (int day = 1; day <= 7; day++)
            {
                var date = DateTime.Today.AddDays(day);
                
                // Bu günün çalışma saatlerini bul
                var workingHours = allWorkingHours.FirstOrDefault(w => w.DayOfWeek == date.DayOfWeek);
                if (workingHours == null || !workingHours.IsWorkingDay)
                    continue;
                    
                // Bu günün mevcut randevularını al - UTC dönüşümü ile
                var dateUtc = date.Date.ToUniversalTime();
                var nextDayUtc = dateUtc.AddDays(1);
                var dayAppointments = stylistAppointments
                    .Where(a => a.StartAt >= dateUtc && a.StartAt < nextDayUtc)
                    .ToList();
                    
                // Çalışma saatleri içinde slotlar oluştur
                var currentTime = date.Date.Add(workingHours.OpenTime);
                var endTime = date.Date.Add(workingHours.CloseTime);
                
                while (currentTime.AddMinutes(service.DurationMin) <= endTime)
                {
                    // Öğle arası kontrolü - sadece çalışma süresi 4 saatten fazlaysa
                    var workDuration = workingHours.CloseTime - workingHours.OpenTime;
                    if (workDuration.TotalHours >= 4 && 
                        workingHours.BreakStart.HasValue && workingHours.BreakEnd.HasValue &&
                        workingHours.BreakStart.Value < workingHours.CloseTime &&
                        workingHours.BreakEnd.Value > workingHours.OpenTime)
                    {
                        var breakStart = date.Date.Add(workingHours.BreakStart.Value);
                        var breakEnd = date.Date.Add(workingHours.BreakEnd.Value);
                        
                        if (currentTime < breakEnd && currentTime.AddMinutes(service.DurationMin) > breakStart)
                        {
                            currentTime = breakEnd;
                            continue;
                        }
                    }
                    
                    // Minimum bildirim süresi kontrolü (30 dakika)
                    var minAdvanceTime = DateTime.Now.AddMinutes(30);
                    if (currentTime <= minAdvanceTime)
                    {
                        currentTime = currentTime.AddMinutes(30);
                        continue;
                    }
                    
                    // Çakışma kontrolü - memory'de yap, DB query'siz - UTC dönüşümü ile
                    var slotEnd = currentTime.AddMinutes(service.DurationMin);
                    var currentTimeUtc = currentTime.ToUniversalTime();
                    var slotEndUtc = slotEnd.ToUniversalTime();
                    var hasConflict = dayAppointments.Any(a => 
                        a.StartAt < slotEndUtc && a.EndAt > currentTimeUtc);
                        
                    // TimeSlotVm'e local time geç (view'da doğru gösterim için)
                    slots.Add(new TimeSlotVm(currentTime, !hasConflict));
                    currentTime = currentTime.AddMinutes(15); // 15 dakikalık aralıklar
                }
            }
            
            return slots.OrderBy(s => s.Start).ToList();
        }

        private async Task<int> GetCurrentCustomerId()
        {
            // Identity User ID'den Customer ID'yi bul
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("DEBUG: User ID bulunamadı");
                return 0;
            }

            Console.WriteLine($"DEBUG: User ID: {userId}");
            
            var customer = await _customerService.GetByUserIdAsync(userId);
            if (customer == null)
            {
                Console.WriteLine("DEBUG: Customer kaydı bulunamadı, otomatik oluşturuluyor...");
                
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
                Console.WriteLine($"DEBUG: Customer oluşturuldu - ID: {customer.Id}");
            }
            else
            {
                Console.WriteLine($"DEBUG: Customer bulundu - ID: {customer.Id}");
            }

            return customer?.Id ?? 0;
        }

        [HttpPost("validate-coupon")]
        public async Task<IActionResult> ValidateCoupon([FromBody] CouponValidationRequest request)
        {
            try
            {
                var customerId = await GetCurrentCustomerId();
                var result = await _couponService.ValidateAsync(request.Code, request.BasketTotal, customerId);

                return Json(new
                {
                    success = result.IsValid,
                    message = result.IsValid ? "Kupon geçerli" : result.Reason,
                    discount = result.Discount,
                    discountAmount = result.Discount?.Amount ?? 0
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Kupon doğrulama sırasında hata oluştu: " + ex.Message
                });
            }
        }

        [HttpPost("apply-coupon")]
        public async Task<IActionResult> ApplyCoupon([FromBody] CouponApplyRequest request)
        {
            try
            {
                var customerId = await GetCurrentCustomerId();
                
                // Önce kuponu doğrula
                var validation = await _couponService.ValidateAsync(request.Code, request.BasketTotal, customerId);

                if (!validation.IsValid)
                {
                    return Json(new
                    {
                        success = false,
                        message = validation.Reason
                    });
                }

                // Kuponu uygula
                var coupon = await _couponService.GetByCodeAsync(request.Code);
                if (coupon == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Kupon bulunamadı"
                    });
                }

                var usage = await _couponService.ApplyCouponAsync(
                    coupon.Id,
                    customerId,
                    request.AppointmentId,
                    validation.Discount!.Amount,
                    request.Notes
                );

                return Json(new
                {
                    success = true,
                    message = "Kupon başarıyla uygulandı",
                    discount = validation.Discount,
                    usageId = usage.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Kupon uygulanırken hata oluştu: " + ex.Message
                });
            }
        }
    }

    public class CouponValidationRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal BasketTotal { get; set; }
    }

    public class CouponApplyRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal BasketTotal { get; set; }
        public int AppointmentId { get; set; }
        public string? Notes { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Stylists;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using StylistEntity = Kuafor.Web.Models.Entities.Stylist;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class StylistsController : Controller
    {
        private readonly IStylistService _stylistService;
        private readonly IBranchService _branchService;
        private readonly IWorkingHoursService _workingHoursService;

        public StylistsController(
            IStylistService stylistService, 
            IBranchService branchService,
            IWorkingHoursService workingHoursService)
        {
            _stylistService = stylistService;
            _branchService = branchService;
            _workingHoursService = workingHoursService;
        }

        // GET: /Admin/Stylists
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var stylists = await _stylistService.GetAllAsync();
            var branches = await _branchService.GetAllAsync();

            var list = stylists.Select(s => new StylistDto
            {
                Id = s.Id,
                Name = $"{s.FirstName} {s.LastName}",
                Rating = s.Rating,
                Bio = s.Bio,
                BranchId = s.BranchId,
                IsActive = s.IsActive
            }).OrderBy(s => s.Id).ToList();

            ViewBag.Branches = branches.Select(b => new BranchDto
            {
                Id = b.Id,
                Name = b.Name,
                IsActive = b.IsActive,
                Address = b.Address ?? string.Empty,
                Phone = b.Phone ?? string.Empty
            }).ToList();

            return View(list);
        }

        // GET: /Admin/Stylists/Details/5
        [HttpGet]
        [Route("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var stylist = await _stylistService.GetByIdAsync(id);
            if (stylist == null)
            {
                return NotFound();
            }

            var branch = await _branchService.GetByIdAsync(stylist.BranchId);
            ViewBag.Branch = branch;

            return View(stylist);
        }

        // GET: /Admin/Stylists/Create
        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            var branches = await _branchService.GetAllAsync();
            ViewBag.Branches = branches;
            return View();
        }

        // POST: /Admin/Stylists/Create
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StylistEntity stylist)
        {
            try
            {
                // Debug: Gelen model bilgilerini logla
                Console.WriteLine($"DEBUG: FirstName={stylist.FirstName}, LastName={stylist.LastName}, BranchId={stylist.BranchId}, IsActive={stylist.IsActive}");
                
                // ModelState hatalarını logla
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("DEBUG: ModelState geçersiz. Hatalar:");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($"  - {error.ErrorMessage}");
                    }
                }

                // Basit validation kontrolü
                if (string.IsNullOrWhiteSpace(stylist.FirstName) || 
                    string.IsNullOrWhiteSpace(stylist.LastName) || 
                    stylist.BranchId <= 0)
                {
                    TempData["Error"] = "Lütfen tüm zorunlu alanları doldurun.";
                    Console.WriteLine("DEBUG: Validation hatası - zorunlu alanlar eksik");
                }
                else
                {
                    // Default değerleri set et
                    stylist.CreatedAt = DateTime.UtcNow;
                    stylist.IsActive = true;
                    
                    Console.WriteLine("DEBUG: Kuaför oluşturuluyor...");
                    await _stylistService.CreateAsync(stylist);
                    Console.WriteLine("DEBUG: Kuaför başarıyla oluşturuldu!");
                    
                    // Form'dan çalışma saatlerini al ve kaydet
                    var workingHoursList = new List<WorkingHours>();
                    
                    for (int i = 0; i < 7; i++) // 7 gün
                    {
                        var dayOfWeekStr = Request.Form[$"workingHours[{i}].DayOfWeek"];
                        var isWorkingDayStr = Request.Form[$"workingHours[{i}].IsWorkingDay"];
                        var openTimeStr = Request.Form[$"workingHours[{i}].OpenTime"];
                        var closeTimeStr = Request.Form[$"workingHours[{i}].CloseTime"];
                        var breakStartStr = Request.Form[$"workingHours[{i}].BreakStart"];
                        var breakEndStr = Request.Form[$"workingHours[{i}].BreakEnd"];
                        
                        if (int.TryParse(dayOfWeekStr, out int dayOfWeekInt))
                        {
                            var workingHour = new WorkingHours
                            {
                                BranchId = stylist.BranchId,
                                DayOfWeek = (DayOfWeek)dayOfWeekInt,
                                IsWorkingDay = isWorkingDayStr.ToString().Contains("true"),
                                OpenTime = TimeSpan.TryParse(openTimeStr, out var openTime) ? openTime : TimeSpan.FromHours(9),
                                CloseTime = TimeSpan.TryParse(closeTimeStr, out var closeTime) ? closeTime : TimeSpan.FromHours(18),
                                BreakStart = TimeSpan.TryParse(breakStartStr, out var breakStart) ? breakStart : TimeSpan.FromHours(12),
                                BreakEnd = TimeSpan.TryParse(breakEndStr, out var breakEnd) ? breakEnd : TimeSpan.FromHours(13),
                                CreatedAt = DateTime.UtcNow
                            };
                            
                            workingHoursList.Add(workingHour);
                            Console.WriteLine($"DEBUG: Gün {workingHour.DayOfWeek}, Çalışıyor: {workingHour.IsWorkingDay}, Açılış: {workingHour.OpenTime}");
                        }
                    }

                    // Çalışma saatlerini kaydet
                    if (workingHoursList.Any())
                    {
                        await _workingHoursService.SetBranchWorkingHoursAsync(stylist.BranchId, workingHoursList);
                        Console.WriteLine("DEBUG: Çalışma saatleri kaydedildi!");
                    }
                    
                    // Detaylı başarı mesajı
                    var branch = await _branchService.GetByIdAsync(stylist.BranchId);
                    TempData["Success"] = $"🎉 Yeni kuaför başarıyla eklendi!\n" +
                                         $"👤 {stylist.FirstName} {stylist.LastName}\n" +
                                         $"🏢 {branch?.Name ?? "Şube"}\n" +
                                         $"⭐ {stylist.Rating} puan\n" +
                                         $"⏰ Çalışma saatleri belirlendi";
                    
                    Console.WriteLine($"SUCCESS: Kuaför eklendi - {stylist.FirstName} {stylist.LastName} ({branch?.Name})");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine($"ERROR StackTrace: {ex.StackTrace}");
            }

            // Hata durumunda formu tekrar göster
            var branches = await _branchService.GetAllAsync();
            ViewBag.Branches = branches;
            return View(stylist);
        }

        // GET: /Admin/Stylists/Edit/5
        [HttpGet]
        [Route("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var stylist = await _stylistService.GetByIdAsync(id);
            if (stylist == null)
            {
                return NotFound();
            }

            var branches = await _branchService.GetAllAsync();
            ViewBag.Branches = branches;

            // Çalışma saatlerini getir
            var workingHours = await _workingHoursService.GetByBranchAsync(stylist.BranchId);
            ViewBag.WorkingHours = workingHours.ToList();

            return View(stylist);
        }

        // POST: /Admin/Stylists/Edit/5
        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StylistEntity stylist)
        {
            if (id != stylist.Id)
            {
                return NotFound();
            }

            try
            {
                // Model validation'ını manuel kontrol et
                if (string.IsNullOrWhiteSpace(stylist.FirstName))
                {
                    ModelState.AddModelError("FirstName", "Ad alanı zorunludur.");
                }
                
                if (string.IsNullOrWhiteSpace(stylist.LastName))
                {
                    ModelState.AddModelError("LastName", "Soyad alanı zorunludur.");
                }
                
                if (stylist.BranchId <= 0)
                {
                    ModelState.AddModelError("BranchId", "Şube seçimi zorunludur.");
                }

                if (ModelState.IsValid)
                {
                   
                    stylist.UpdatedAt = DateTime.UtcNow;
                    
                    await _stylistService.UpdateAsync(stylist);
                    
                    // Detaylı güncelleme mesajı
                    var branch = await _branchService.GetByIdAsync(stylist.BranchId);
                    TempData["Success"] = $"✅ Kuaför bilgileri başarıyla güncellendi!\n" +
                                         $"👤 {stylist.FirstName} {stylist.LastName}\n" +
                                         $"🏢 {branch?.Name ?? "Şube"}\n" +
                                         $"⭐ {stylist.Rating} puan\n" +
                                         $"📧 {stylist.Email}\n" +
                                         $"📱 {stylist.Phone}";
                    
                    Console.WriteLine($"SUCCESS: Kuaför güncellendi - {stylist.FirstName} {stylist.LastName} ({branch?.Name})");
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Güncelleme hatası: {ex.Message}";
                Console.WriteLine($"Update Error: {ex.Message}");
            }

            var branches = await _branchService.GetAllAsync();
            ViewBag.Branches = branches;

            // Çalışma saatlerini tekrar getir
            var workingHours = await _workingHoursService.GetByBranchAsync(stylist.BranchId);
            ViewBag.WorkingHours = workingHours.ToList();

            return View(stylist);
        }

        // POST: /Admin/Stylists/UpdateWorkingHours/5
        [HttpPost]
        [Route("UpdateWorkingHours/{stylistId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWorkingHours(int stylistId)
        {
            try
            {
                var stylist = await _stylistService.GetByIdAsync(stylistId);
                if (stylist == null)
                {
                    TempData["Error"] = "Kuaför bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Form'dan gelen değerleri manuel olarak parse et
                var workingHoursList = new List<WorkingHours>();
                
                for (int i = 0; i < 7; i++) // 7 gün
                {
                    var dayOfWeekStr = Request.Form[$"workingHours[{i}].DayOfWeek"];
                    var isWorkingDayStr = Request.Form[$"workingHours[{i}].IsWorkingDay"];
                    var openTimeStr = Request.Form[$"workingHours[{i}].OpenTime"];
                    var closeTimeStr = Request.Form[$"workingHours[{i}].CloseTime"];
                    var breakStartStr = Request.Form[$"workingHours[{i}].BreakStart"];
                    var breakEndStr = Request.Form[$"workingHours[{i}].BreakEnd"];
                    
                    if (int.TryParse(dayOfWeekStr, out int dayOfWeekInt))
                    {
                        var workingHour = new WorkingHours
                        {
                            BranchId = stylist.BranchId,
                            DayOfWeek = (DayOfWeek)dayOfWeekInt,
                            IsWorkingDay = isWorkingDayStr.ToString().Contains("true"),
                            OpenTime = TimeSpan.TryParse(openTimeStr, out var openTime) ? openTime : TimeSpan.FromHours(9),
                            CloseTime = TimeSpan.TryParse(closeTimeStr, out var closeTime) ? closeTime : TimeSpan.FromHours(18),
                            BreakStart = TimeSpan.TryParse(breakStartStr, out var breakStart) ? breakStart : TimeSpan.FromHours(12),
                            BreakEnd = TimeSpan.TryParse(breakEndStr, out var breakEnd) ? breakEnd : TimeSpan.FromHours(13),
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        workingHoursList.Add(workingHour);
                        
                        Console.WriteLine($"DEBUG: Gün {workingHour.DayOfWeek}, Çalışıyor: {workingHour.IsWorkingDay}, Açılış: {workingHour.OpenTime}, Kapanış: {workingHour.CloseTime}");
                    }
                }

                await _workingHoursService.SetBranchWorkingHoursAsync(stylist.BranchId, workingHoursList);
                
                TempData["Success"] = $"✅ {stylist.FirstName} {stylist.LastName} için çalışma saatleri başarıyla güncellendi!";
                Console.WriteLine($"DEBUG: Çalışma saatleri başarıyla güncellendi. Kuaför: {stylist.FirstName} {stylist.LastName}");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Çalışma saatleri güncellenirken hata: {ex.Message}";
                Console.WriteLine($"ERROR: UpdateWorkingHours - {ex.Message}");
                Console.WriteLine($"ERROR StackTrace: {ex.StackTrace}");
            }

            return RedirectToAction(nameof(Edit), new { id = stylistId });
        }

        // GET: /Admin/Stylists/Delete/5 - Confirmation sayfası
        [HttpGet]
        [Route("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var stylist = await _stylistService.GetByIdAsync(id);
            if (stylist == null)
            {
                return NotFound();
            }

            return View(stylist);
        }

        // POST: /Admin/Stylists/Delete/5 - Gerçek silme işlemi
        [HttpPost]
        [Route("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _stylistService.DeleteAsync(id);
                TempData["Success"] = "Kuaför başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Silme hatası: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
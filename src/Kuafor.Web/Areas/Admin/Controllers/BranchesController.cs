using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Branches;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class BranchesController : Controller
    {
        private readonly IBranchService _branchService;
        private readonly IAppointmentService _appointmentService;
        private readonly IStylistService _stylistService;
        private readonly object _lock = new object();
        private static List<BranchDto> _branches = new List<BranchDto>();

        public BranchesController(
            IBranchService branchService,
            IAppointmentService appointmentService,
            IStylistService stylistService)
        {
            _branchService = branchService;
            _appointmentService = appointmentService;
            _stylistService = stylistService;
        }

        // GET: /Admin/Branches
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var branches = await _branchService.GetAllAsync();
            var list = branches.Select(b => new BranchDto
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address ?? string.Empty,
                Phone = b.Phone ?? string.Empty,
                IsActive = b.IsActive
            }).OrderBy(b => b.Id).ToList();
            
            return View(list);
        }

        // POST: /Admin/Branches/Create
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BranchFormModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return Redirect("/Admin/Branches");
            }

            try
            {
                var branch = new Branch
                {
                    Name = form.Name.Trim(),
                    Address = form.Address?.Trim() ?? string.Empty,
                    Phone = form.Phone?.Trim() ?? string.Empty,
                    IsActive = form.IsActive
                };
                
                await _branchService.CreateAsync(branch);
                TempData["Success"] = "Şube eklendi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Şube eklenirken hata oluştu: {ex.Message}";
            }
            
            return Redirect("/Admin/Branches");
        }

        // POST: /Admin/Branches/Edit
        [HttpPost]
        [Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BranchFormModel form)
        {
            if (!ModelState.IsValid || form.Id <= 0)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return Redirect("/Admin/Branches");
            }

            try
            {
                var branch = await _branchService.GetByIdAsync(form.Id);
                if (branch == null)
                {
                    TempData["Error"] = "Kayıt bulunamadı.";
                    return Redirect("/Admin/Branches");
                }

                branch.Name = form.Name.Trim();
                branch.Address = form.Address?.Trim() ?? string.Empty;
                branch.Phone = form.Phone?.Trim() ?? string.Empty;
                branch.IsActive = form.IsActive;
                
                await _branchService.UpdateAsync(branch);
                TempData["Success"] = "Şube güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Şube güncellenirken hata oluştu: {ex.Message}";
            }
            
            return Redirect("/Admin/Branches");
        }

        // POST: /Admin/Branches/Delete
        [HttpPost]
        [Route("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Geçersiz istek.";
                return Redirect("/Admin/Branches");
            }

            try
            {
                await _branchService.DeleteAsync(id);
                TempData["Success"] = "Şube silindi.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = $"Şube silinemedi: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Beklenmeyen hata: {ex.Message}";
            }

            return Redirect("/Admin/Branches");
        }

        // GET: /Admin/Branches/Create
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View(new BranchFormModel());
        }

        // GET: /Admin/Branches/Details/5
        [HttpGet]
        [Route("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var branch = await _branchService.GetByIdAsync(id);
                if (branch == null)
                {
                    return NotFound();
                }

                // Şube istatistikleri
                var appointments = await _appointmentService.GetByBranchAsync(id);
                var stylists = await _stylistService.GetByBranchAsync(id);
                
                var branchDetails = new BranchDetailsViewModel
                {
                    Branch = branch,
                    TotalAppointments = appointments.Count(),
                    TotalStylists = stylists.Count(),
                    MonthlyRevenue = appointments.Where(a => a.StartAt >= DateTime.Now.AddMonths(-1)).Sum(a => a.FinalPrice),
                    AverageRating = stylists.Any() ? (double)stylists.Average(s => s.Rating) : 0
                };

                return View(branchDetails);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // GET: /Admin/Branches/Edit/5
        [HttpGet]
        [Route("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var branch = await _branchService.GetByIdAsync(id);
                if (branch == null)
                {
                    return NotFound();
                }

                var formModel = new BranchFormModel
                {
                    Id = branch.Id,
                    Name = branch.Name,
                    Address = branch.Address ?? string.Empty,
                    Phone = branch.Phone ?? string.Empty,
                    IsActive = branch.IsActive
                };

                return View(formModel);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // POST: /Admin/Branches/ToggleActive/5
        [HttpPost]
        [Route("ToggleActive/{id:int}")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                var branch = await _branchService.GetByIdAsync(id);
                if (branch == null)
                {
                    return Json(new { success = false, message = "Şube bulunamadı" });
                }

                branch.IsActive = !branch.IsActive;
                await _branchService.UpdateAsync(branch);
                
                return Json(new { success = true, isActive = branch.IsActive });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /Admin/Branches/Statistics/{id:int}
        [HttpGet]
        [Route("Statistics/{id:int}")]
        public async Task<IActionResult> GetBranchStatistics(int id)
        {
            try
            {
                var appointments = await _appointmentService.GetByBranchAsync(id);
                var stylists = await _stylistService.GetByBranchAsync(id);
                
                var stats = new
                {
                    totalAppointments = appointments.Count(),
                    monthlyAppointments = appointments.Count(a => a.StartAt >= DateTime.Now.AddMonths(-1)),
                    totalRevenue = appointments.Sum(a => a.FinalPrice),
                    monthlyRevenue = appointments.Where(a => a.StartAt >= DateTime.Now.AddMonths(-1)).Sum(a => a.FinalPrice),
                    totalStylists = stylists.Count(),
                    activeStylists = stylists.Count(s => s.IsActive),
                    averageRating = stylists.Any() ? stylists.Average(s => s.Rating) : 0
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

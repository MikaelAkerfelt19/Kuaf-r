using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Branches;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BranchesController : Controller
    {
        private readonly IBranchService _branchService;
        private readonly object _lock = new object();
        private static List<BranchDto> _branches = new List<BranchDto>();

        public BranchesController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        // GET: /Admin/Branches
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
    }
}

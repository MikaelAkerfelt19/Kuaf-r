using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Stylists;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class StylistsController : Controller
    {
        private readonly IStylistService _stylistService;
        private readonly IBranchService _branchService;

        public StylistsController(IStylistService stylistService, IBranchService branchService)
        {
            _stylistService = stylistService;
            _branchService = branchService;
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
        public async Task<IActionResult> Create(global::Kuafor.Web.Models.Entities.Stylist stylist)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _stylistService.CreateAsync(stylist);
                    TempData["Success"] = "Kuaför başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Hata: {ex.Message}";
                }
            }

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

            return View(stylist);
        }

        // POST: /Admin/Stylists/Edit/5
        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, global::Kuafor.Web.Models.Entities.Stylist stylist)
        {
            if (id != stylist.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _stylistService.UpdateAsync(stylist);
                    TempData["Success"] = "Kuaför başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Hata: {ex.Message}";
                }
            }

            var branches = await _branchService.GetAllAsync();
            ViewBag.Branches = branches;

            return View(stylist);
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
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = $"Kuaför silinemedi: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Beklenmeyen hata: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Admin.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ServicesController : Controller
    {
        private readonly IServiceService _serviceService;
        private readonly IStylistService _stylistService;

        public ServicesController(IServiceService serviceService, IStylistService stylistService)
        {
            _serviceService = serviceService;
            _stylistService = stylistService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var services = await _serviceService.GetAllAsync();
            var stylists = (await _stylistService.GetActiveAsync()).ToList();

            // Service entity'lerini ServiceDto'ya dönüştür
            var serviceDtos = services.Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                StylistId = s.StylistId,
                StylistName = s.Stylist?.Name ?? string.Empty,
                Category = s.Category,
                DurationMin = s.DurationMin,
                Price = s.Price,
                IsActive = s.IsActive
            }).ToList();

            var viewModel = new ServicesIndexViewModel
            {
                Services = serviceDtos,
                Stylists = stylists
            };

            ViewBag.Stylists = await GetStylistSelectListAsync();

            return View(viewModel);
        }

        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Stylists = await GetStylistSelectListAsync();
            return View(new Service());
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (!await _stylistService.ExistsAsync(service.StylistId))
            {
                ModelState.AddModelError(nameof(service.StylistId), "Geçerli bir kuaför seçin.");
            }

            if (ModelState.IsValid)
            {
                service.IconClass = MapIconByCategory(service.Category);
                service.PriceFrom = null;
                service.ShowOnHomePage = false;

                await _serviceService.CreateAsync(service);
                TempData["Success"] = "Hizmet başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Stylists = await GetStylistSelectListAsync();
            return View(service);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var service = await _serviceService.GetByIdAsync(id);
            if (service == null)
                return NotFound();
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.Id)
                return NotFound();

            if (!await _stylistService.ExistsAsync(service.StylistId))
            {
                ModelState.AddModelError(nameof(service.StylistId), "Geçerli bir kuaför seçin.");
            }

            if (ModelState.IsValid)
            {
                service.IconClass = MapIconByCategory(service.Category);
                service.PriceFrom = null;
                service.ShowOnHomePage = false;

                await _serviceService.UpdateAsync(service);
                TempData["Success"] = "Hizmet başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Stylists = await GetStylistSelectListAsync();
            return View(service);
        }

        [HttpGet]
        [Route("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _serviceService.GetByIdAsync(id);
            if (service == null)
                return NotFound();
            return View(service);
        }

        [HttpPost]
        [Route("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _serviceService.DeleteAsync(id);
                TempData["Success"] = "Hizmet başarıyla silindi.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = $"Hizmet silinemedi: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Beklenmeyen hata: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [Route("BulkDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete(int[] selectedIds)
        {
            if (selectedIds == null || selectedIds.Length == 0)
            {
                TempData["Error"] = "Lütfen silmek için en az bir hizmet seçin.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                foreach (var id in selectedIds)
                {
                    await _serviceService.DeleteAsync(id);
                }

                TempData["Success"] = $"{selectedIds.Length} hizmet silindi.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = $"Bazı hizmetler silinemedi: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Beklenmeyen hata: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleHomePageVisibility(int id)
        {
            await _serviceService.ToggleHomePageVisibilityAsync(id);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                await _serviceService.ToggleActiveAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDisplayOrder(int id, int newOrder)
        {
            await _serviceService.UpdateDisplayOrderAsync(id, newOrder);
            return Json(new { success = true });
        }

        private async Task<List<SelectListItem>> GetStylistSelectListAsync()
        {
            var stylists = (await _stylistService.GetActiveAsync())
                .OrderBy(s => s.BranchId)
                .ThenBy(s => s.FirstName)
                .ThenBy(s => s.LastName)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.Name} (Şube #{s.BranchId})"
                })
                .ToList();

            return stylists;
        }

        private static string MapIconByCategory(string? category)
        {
            return category switch
            {
                "coloring" => "bi bi-palette",
                "styling" => "bi bi-star",
                "beard" => "bi bi-scissors",
                "care" => "bi bi-droplet",
                "manicure" => "bi bi-hand-index",
                "other" => "bi bi-heart",
                _ => "bi bi-scissors"
            };
        }

    }
}

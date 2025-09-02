using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Admin.Services;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ServicesController : Controller
    {
        private readonly IServiceService _serviceService;

        public ServicesController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var services = await _serviceService.GetAllAsync();
            
            // Service entity'lerini ServiceDto'ya dönüştür
            var serviceDtos = services.Select(s => new ServiceDto
            {
                Id = s.Id,
                Name = s.Name,
                Category = s.Category,
                DurationMin = s.DurationMin,
                Price = s.Price,
                IsActive = s.IsActive
            }).ToList();
            
            return View(serviceDtos);
        }

        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View(new Service());
        }

        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (ModelState.IsValid)
            {
                await _serviceService.CreateAsync(service);
                TempData["Success"] = "Hizmet başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
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

            if (ModelState.IsValid)
            {
                await _serviceService.UpdateAsync(service);
                TempData["Success"] = "Hizmet başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
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
    }
}

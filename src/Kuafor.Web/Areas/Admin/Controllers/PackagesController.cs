using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Admin.Packages;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class PackagesController : Controller
    {
        private readonly IPackageService _packageService;
        private readonly IServiceService _serviceService;
        private readonly ICustomerService _customerService;
        private readonly IBranchService _branchService;
        private readonly IStylistService _stylistService;

        public PackagesController(
            IPackageService packageService,
            IServiceService serviceService,
            ICustomerService customerService,
            IBranchService branchService,
            IStylistService stylistService)
        {
            _packageService = packageService;
            _serviceService = serviceService;
            _customerService = customerService;
            _branchService = branchService;
            _stylistService = stylistService;
        }

        // GET: /Admin/Packages
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var packages = await _packageService.GetAllAsync();
            return View(packages);
        }

        // GET: /Admin/Packages/Create
        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Services = await _serviceService.GetAllAsync();
            ViewBag.Branches = await _branchService.GetAllAsync();
            ViewBag.Stylists = await _stylistService.GetAllAsync();
            return View();
        }

        // POST: /Admin/Packages/Create
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Package package, int[] selectedServiceIds)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    package.CreatedAt = DateTime.UtcNow;
                    var createdPackage = await _packageService.CreateAsync(package);
                    
                    // Seçilen hizmetleri pakete ekle
                    if (selectedServiceIds?.Length > 0)
                    {
                        await _packageService.AddServicesToPackageAsync(createdPackage.Id, selectedServiceIds.ToList());
                    }
                    
                    TempData["Success"] = "Paket başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Paket oluşturulamadı: {ex.Message}";
            }
            
            // Hata durumunda listeleri tekrar yükle
            ViewBag.Services = await _serviceService.GetAllAsync();
            ViewBag.Branches = await _branchService.GetAllAsync();
            ViewBag.Stylists = await _stylistService.GetAllAsync();
            return View(package);
        }

        // GET: /Admin/Packages/Edit/5
        [HttpGet]
        [Route("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var package = await _packageService.GetByIdAsync(id);
                if (package == null)
                {
                    return NotFound();
                }

                ViewBag.Services = await _serviceService.GetAllAsync();
                ViewBag.Branches = await _branchService.GetAllAsync();
                ViewBag.Stylists = await _stylistService.GetAllAsync();
                ViewBag.PackageServices = await _packageService.GetPackageServicesAsync(id);
                
                return View(package);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // POST: /Admin/Packages/Edit/5
        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Package package, int[] selectedServiceIds)
        {
            if (id != package.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    package.UpdatedAt = DateTime.UtcNow;
                    await _packageService.UpdateAsync(package);
                    
                    // Paket hizmetlerini güncelle
                    if (selectedServiceIds != null)
                    {
                        await _packageService.UpdatePackageServicesAsync(package.Id, selectedServiceIds.ToList());
                    }
                    
                    TempData["Success"] = "Paket başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Paket güncellenemedi: {ex.Message}";
            }
            
            // Hata durumunda listeleri tekrar yükle
            ViewBag.Services = await _serviceService.GetAllAsync();
            ViewBag.Branches = await _branchService.GetAllAsync();
            ViewBag.Stylists = await _stylistService.GetAllAsync();
            ViewBag.PackageServices = await _packageService.GetPackageServicesAsync(id);
            return View(package);
        }

        // POST: /Admin/Packages/Delete/5
        [HttpPost]
        [Route("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _packageService.DeleteAsync(id);
                TempData["Success"] = "Paket başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Paket silinemedi: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Packages/Sales
        [HttpGet]
        [Route("Sales")]
        public async Task<IActionResult> Sales()
        {
            var sales = await _packageService.GetSalesAsync();
            return View(sales);
        }
    }
}

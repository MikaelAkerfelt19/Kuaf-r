using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Admin.Adisyon;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class AdisyonController : Controller
    {
        private readonly IAdisyonService _adisyonService;
        private readonly ICustomerService _customerService;
        private readonly IServiceService _serviceService;

        public AdisyonController(
            IAdisyonService adisyonService,
            ICustomerService customerService,
            IServiceService serviceService)
        {
            _adisyonService = adisyonService;
            _customerService = customerService;
            _serviceService = serviceService;
        }

        // GET: /Admin/Adisyon
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var adisyonlar = await _adisyonService.GetAllAsync();
            return View(adisyonlar);
        }

        // GET: /Admin/Adisyon/CreateForRegistered
        [HttpGet]
        [Route("CreateForRegistered")]
        public async Task<IActionResult> CreateForRegistered()
        {
            ViewBag.Customers = await _customerService.GetAllAsync();
            ViewBag.Services = await _serviceService.GetAllAsync();
            return View();
        }

        // GET: /Admin/Adisyon/CreateForNew
        [HttpGet]
        [Route("CreateForNew")]
        public async Task<IActionResult> CreateForNew()
        {
            ViewBag.Services = await _serviceService.GetAllAsync();
            ViewBag.Customers = await _customerService.GetAllAsync(); // Database'den customer bilgileri
            return View();
        }

        // POST: /Admin/Adisyon/CreateForRegistered
        [HttpPost]
        [Route("CreateForRegistered")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForRegistered(AdisyonFormModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Database'den customer bilgilerini al
                    var customer = await _customerService.GetByIdAsync(model.CustomerId);
                    if (customer == null)
                    {
                        TempData["Error"] = "Müşteri bulunamadı.";
                        return RedirectToAction(nameof(CreateForRegistered));
                    }

                    var adisyon = new Adisyon
                    {
                        CustomerId = customer.Id,
                        CustomerName = customer.FullName,
                        CustomerPhone = customer.Phone ?? "",
                        CustomerEmail = customer.Email ?? "",
                        TotalAmount = model.TotalAmount,
                        DiscountAmount = model.DiscountAmount,
                        FinalAmount = model.TotalAmount - model.DiscountAmount,
                        PaymentMethod = model.PaymentMethod,
                        Notes = model.Notes,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _adisyonService.CreateAsync(adisyon);
                    
                    // Adisyon detaylarını ekle
                    if (model.Services?.Any() == true)
                    {
                        var serviceModels = model.Services.Select(s => new AdisyonServiceModel
                        {
                            ServiceId = s.ServiceId,
                            ServiceName = s.ServiceName,
                            UnitPrice = s.Price,
                            Quantity = s.Quantity
                        }).ToList();
                        await _adisyonService.AddServicesToAdisyonAsync(adisyon.Id, serviceModels);
                    }

                    TempData["Success"] = $"Adisyon başarıyla oluşturuldu. Müşteri: {customer.FullName}";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Adisyon oluşturulamadı: {ex.Message}";
            }

            ViewBag.Customers = await _customerService.GetAllAsync();
            ViewBag.Services = await _serviceService.GetAllAsync();
            return View(model);
        }

        // POST: /Admin/Adisyon/CreateForNew
        [HttpPost]
        [Route("CreateForNew")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForNew(AdisyonFormModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Yeni müşteri için adisyon oluştur
                    var adisyon = new Adisyon
                    {
                        CustomerId = null, // Yeni müşteri için null
                        CustomerName = model.CustomerName,
                        CustomerPhone = model.CustomerPhone,
                        CustomerEmail = model.CustomerEmail,
                        TotalAmount = model.TotalAmount,
                        DiscountAmount = model.DiscountAmount,
                        FinalAmount = model.TotalAmount - model.DiscountAmount,
                        PaymentMethod = model.PaymentMethod,
                        Notes = model.Notes,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _adisyonService.CreateAsync(adisyon);
                    
                    // Adisyon detaylarını ekle
                    if (model.Services?.Any() == true)
                    {
                        var serviceModels = model.Services.Select(s => new AdisyonServiceModel
                        {
                            ServiceId = s.ServiceId,
                            ServiceName = s.ServiceName,
                            UnitPrice = s.Price,
                            Quantity = s.Quantity
                        }).ToList();
                        await _adisyonService.AddServicesToAdisyonAsync(adisyon.Id, serviceModels);
                    }

                    TempData["Success"] = $"Adisyon başarıyla oluşturuldu. Müşteri: {model.CustomerName}";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Adisyon oluşturulamadı: {ex.Message}";
            }

            ViewBag.Services = await _serviceService.GetAllAsync();
            ViewBag.Customers = await _customerService.GetAllAsync();
            return View(model);
        }

        // GET: /Admin/Adisyon/Details/5
        [HttpGet]
        [Route("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var adisyon = await _adisyonService.GetByIdAsync(id);
                if (adisyon == null)
                {
                    return NotFound();
                }

                // Müşteri bilgilerini database'den al
                if (adisyon.CustomerId != null && adisyon.CustomerId > 0)
                {
                    var customer = await _customerService.GetByIdAsync(adisyon.CustomerId.Value);
                    ViewBag.Customer = customer;
                }

                var adisyonDetails = await _adisyonService.GetAdisyonDetailsAsync(id);
                ViewBag.AdisyonDetails = adisyonDetails;

                return View(adisyon);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // API: Müşteri arama
        [HttpGet]
        [Route("SearchCustomers")]
        public async Task<IActionResult> SearchCustomers(string term)
        {
            try
            {
                var customers = await _customerService.GetAllAsync();
                var filteredCustomers = customers
                    .Where(c => c.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                               (c.Phone != null && c.Phone.Contains(term)) ||
                               (c.Email != null && c.Email.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    .Take(10)
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.FullName,
                        phone = c.Phone,
                        email = c.Email
                    });

                return Json(new { success = true, data = filteredCustomers });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

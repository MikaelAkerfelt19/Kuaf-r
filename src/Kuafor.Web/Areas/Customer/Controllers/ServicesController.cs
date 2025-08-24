using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Services;
using Microsoft.AspNetCore.Authorization;

namespace Kuafor.Web.Areas.Customer.Controllers;

[Area("Customer")]
[Route("Customer/[controller]")]
[AllowAnonymous] // Hizmetleri herkes görebilsin
public class ServicesController : Controller
{
    private readonly IServiceService _serviceService;
    private readonly IStylistService _stylistService;
    private readonly IBranchService _branchService;

    public ServicesController(
        IServiceService serviceService,
        IStylistService stylistService,
        IBranchService branchService)
    {
        _serviceService = serviceService;
        _stylistService = stylistService;
        _branchService = branchService;
    }

    // GET: /Customer/Services
    public async Task<IActionResult> Index()
    {
        var services = await _serviceService.GetActiveAsync();
        var branches = await _branchService.GetActiveAsync();
        
        var vm = new ServicesIndexViewModel
        {
            Services = services.ToList(),
            Branches = branches.ToList()
        };

        return View(vm);
    }

    // GET: /Customer/Services/Details/5
    [Route("Detail/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var service = await _serviceService.GetByIdAsync(id);
        if (service == null)
        {
            TempData["Error"] = "Hizmet bulunamadı";
            return RedirectToAction("Index");
        }

        // Bu hizmeti sunan kuaförleri bul
        var stylists = await _stylistService.GetByServiceAsync(id);
        
        var vm = new ServiceDetailsViewModel
        {
            Service = service,
            Stylists = stylists.ToList(),
            RelatedServices = (await _serviceService.GetRelatedAsync(id)).ToList()
        };

        return View(vm);
    }

    // GET: /Customer/Services/Search
    public async Task<IActionResult> Search(string q, int? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        var services = await _serviceService.SearchAsync(q, categoryId, minPrice, maxPrice);
        
        var vm = new ServicesSearchViewModel
        {
            Query = q,
            CategoryId = categoryId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Results = services.ToList()
        };

        return View(vm);
    }

    // GET: /Services (Global route - Area olmadan)
    [Route("/[controller]")]
    [AllowAnonymous]
    public async Task<IActionResult> GlobalIndex()
    {
        var services = await _serviceService.GetActiveAsync();
        var branches = await _branchService.GetActiveAsync();
        
        var vm = new ServicesIndexViewModel
        {
            Services = services.ToList(),
            Branches = branches.ToList()
        };

        return View("Index", vm);
    }

    // GET: /Services/Detail/{id} (Global route - Area olmadan)
    [Route("/[controller]/Detail/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GlobalDetails(int id)
    {
        var service = await _serviceService.GetByIdAsync(id);
        if (service == null)
        {
            TempData["Error"] = "Hizmet bulunamadı";
            return RedirectToAction("GlobalIndex");
        }

        var stylists = await _stylistService.GetByServiceAsync(id);
        
        var vm = new ServiceDetailsViewModel
        {
            Service = service,
            Stylists = stylists.ToList(),
            RelatedServices = (await _serviceService.GetRelatedAsync(id)).ToList()
        };

        return View("Details", vm);
    }
}

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
    private readonly IWorkingHoursService _workingHoursService;

    public ServicesController(
        IServiceService serviceService,
        IStylistService stylistService,
        IBranchService branchService,
        IWorkingHoursService workingHoursService)
    {
        _serviceService = serviceService;
        _stylistService = stylistService;
        _branchService = branchService;
        _workingHoursService = workingHoursService;
    }

    // GET: /Customer/Services
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index()
    {
        var services = await _serviceService.GetActiveAsync();
        var branches = await _branchService.GetActiveAsync();
        
        // Database'den gelen hizmetlerin kategorilerini al
        var categories = services
            .Where(s => !string.IsNullOrEmpty(s.Category))
            .Select(s => s.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
        
        var vm = new ServicesIndexViewModel
        {
            Services = services.ToList(),
            Branches = branches.ToList(),
            Categories = categories
        };

        return View(vm);
    }

    // GET: /Customer/Services/Details/5
    [HttpGet]
    [Route("Details/{id:int}")]
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
        
        // İlk şubenin çalışma saatlerini al (örnek olarak)
        var branches = await _branchService.GetActiveAsync();
        var firstBranch = branches.FirstOrDefault();
        if (firstBranch != null)
        {
            var workingHours = await _workingHoursService.GetByBranchAsync(firstBranch.Id);
            ViewBag.WorkingHours = workingHours;
        }
        
        var vm = new ServiceDetailsViewModel
        {
            Service = service,
            Stylists = stylists.ToList(),
            RelatedServices = (await _serviceService.GetRelatedAsync(id)).ToList()
        };

        return View(vm);
    }

    // GET: /Customer/Services/Search
    [HttpGet]
    [Route("Search")]
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
    [HttpGet]
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

    // GET: /Services/Details/{id} (Global route - Area olmadan)
    [HttpGet]
    [Route("/[controller]/Details/{id:int}")]
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

using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models;
using Microsoft.AspNetCore.OutputCaching;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Controllers;

public class HomeController : Controller
{
    private readonly IServiceService _serviceService;
    private readonly ITestimonialService _testimonialService;
    private readonly IBranchService _branchService;

    public HomeController(
        IServiceService serviceService,
        ITestimonialService testimonialService,
        IBranchService branchService)
    {
        _serviceService = serviceService;
        _testimonialService = testimonialService;
        _branchService = branchService;
    }

    [OutputCache(Duration = 300)] // 5 dakika cache
    public async Task<IActionResult> Index()
    {
        var vm = new HomeViewModel
        {
            IsAuthenticated = User?.Identity?.IsAuthenticated ?? false,
            Services = await _serviceService.GetForHomePageAsync(),
            Testimonials = await _testimonialService.GetApprovedForHomePageAsync(),
            Branches = await _branchService.GetForHomePageAsync()
        };
        return View(vm);
    }

    // GET: /Home/Contact
    public IActionResult Contact()
    {
        return View();
    }
}

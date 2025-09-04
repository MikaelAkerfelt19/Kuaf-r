using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Admin.Packages;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class PackagesController : Controller
    {
        private readonly IPackageService _packageService;
        private readonly IServiceService _serviceService;
        private readonly ICustomerService _customerService;

        public PackagesController(
            IPackageService packageService,
            IServiceService serviceService,
            ICustomerService customerService)
        {
            _packageService = packageService;
            _serviceService = serviceService;
            _customerService = customerService;
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
            return View();
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

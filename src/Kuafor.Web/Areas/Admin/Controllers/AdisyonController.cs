using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Admin.Adisyon;

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
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/[controller]")]
public class CouponsController : Controller
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    // GET: /Admin/Coupons
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var coupons = await _couponService.GetAllAsync();
            return View(coupons);
        }
        catch (Exception)
        {
            return View(new List<object>());
        }
    }
}
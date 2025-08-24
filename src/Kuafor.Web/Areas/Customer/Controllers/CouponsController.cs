using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Customer.Controllers;

[Area("Customer")]
[Route("Customer/[controller]")]
public class CouponsController : Controller
{
    private readonly ICouponService _couponService;

    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    // GET: /Customer/Coupons
    public async Task<IActionResult> Index()
    {
        var coupons = await _couponService.GetActiveAsync();
        return View(coupons);
    }
}

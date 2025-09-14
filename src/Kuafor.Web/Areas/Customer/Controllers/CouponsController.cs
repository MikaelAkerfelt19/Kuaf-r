using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Profile;
using Kuafor.Web.Models.Customer;
using Kuafor.Web.Models.Appointments;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Kuafor.Web.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize(Roles = "Customer")]
[Route("Customer/[controller]")]
public class CouponsController : Controller
{
    private readonly ICouponService _couponService;
    private readonly ICustomerService _customerService;

    public CouponsController(ICouponService couponService, ICustomerService customerService)
    {
        _couponService = couponService;
        _customerService = customerService;
    }

    // GET: /Customer/Coupons
    public async Task<IActionResult> Index()
    {
        var customerId = await GetCurrentCustomerId();
        if (customerId == 0)
        {
            TempData["Error"] = "Müşteri bilgisi bulunamadı";
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        var coupons = await _couponService.GetActiveForCustomerAsync(customerId);

        var vm = new Kuafor.Web.Models.Customer.CouponsViewModel
        {
            Active = coupons
                .Where(c => c.ExpiresAt == null || c.ExpiresAt > DateTime.UtcNow)
                .Select(c => new CouponVm(
                    c.Id,
                    c.Code,
                    c.Title,
                    c.DiscountType,
                    c.Amount,
                    c.MinSpend,
                    c.ExpiresAt
                ))
                .ToList(),
            Expired = coupons
                .Where(c => c.ExpiresAt.HasValue && c.ExpiresAt <= DateTime.UtcNow)
                .Select(c => new CouponVm(
                    c.Id,
                    c.Code,
                    c.Title,
                    c.DiscountType,
                    c.Amount,
                    c.MinSpend,
                    c.ExpiresAt
                ))
                .ToList()
        };

        return View(vm);
    }

    // GET: /Customer/Coupons/Validate
    [HttpGet]
    public async Task<IActionResult> Validate(string code, decimal amount)
    {
        try
        {
            var customerId = await GetCurrentCustomerId();
            var result = await _couponService.ValidateAsync(code, amount, customerId);
            
            return Json(new { 
                isValid = result.IsValid, 
                reason = result.Reason,
                discount = result.Discount 
            });
        }
        catch (Exception ex)
        {
            return Json(new { 
                isValid = false, 
                reason = "Kupon doğrulanırken hata oluştu: " + ex.Message 
            });
        }
    }

    private string GetCouponDescription(Coupon coupon)
    {
        var desc = "";
        if (coupon.MinSpend.HasValue)
            desc += $"Min. {coupon.MinSpend:0.##} ₺ harcama gerekli. ";
        if (coupon.ExpiresAt.HasValue)
            desc += $"Son kullanım: {coupon.ExpiresAt:dd.MM.yyyy}";
        return desc.Trim();
    }

    private string GetDiscountText(Coupon coupon)
    {
        if (coupon.DiscountType == "Percent")
            return $"%{coupon.Amount:0.##}";
        else
            return $"{coupon.Amount:0.##} ₺";
    }

    private async Task<int> GetCurrentCustomerId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return 0;
        
        var customer = await _customerService.GetByUserIdAsync(userId);
        return customer?.Id ?? 0;
    }
}

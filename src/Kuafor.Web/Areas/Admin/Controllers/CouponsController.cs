using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Admin.Coupons;

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
            var couponDtos = coupons.Select(c => new CouponDto
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                DiscountType = c.DiscountType == "Percent" ? Kuafor.Web.Models.Admin.Coupons.DiscountType.Percent : Kuafor.Web.Models.Admin.Coupons.DiscountType.Amount,
                Amount = c.Amount,
                MinSpend = c.MinSpend,
                ExpiresAt = c.ExpiresAt,
                IsActive = c.IsActive
            }).ToList();
            
            return View(couponDtos);
        }
        catch (Exception)
        {
            return View(new List<CouponDto>());
        }
    }

    // GET: /Admin/Coupons/Create
    [HttpGet]
    [Route("Create")]
    public IActionResult Create()
    {
        return View(new Coupon());
    }

    // POST: /Admin/Coupons/Create
    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Coupon coupon)
    {
        try
        {
            if (ModelState.IsValid)
            {
                coupon.CreatedAt = DateTime.UtcNow;
                await _couponService.CreateAsync(coupon);
                TempData["Success"] = "Kupon başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Kupon oluşturulamadı: {ex.Message}";
        }
        
        return View(coupon);
    }

    // GET: /Admin/Coupons/Edit/5
    [HttpGet]
    [Route("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var coupon = await _couponService.GetByIdAsync(id);
            if (coupon == null)
                return NotFound();
            
            return View(coupon);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    // POST: /Admin/Coupons/Edit/5
    [HttpPost]
    [Route("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Coupon coupon)
    {
        if (id != coupon.Id)
            return NotFound();

        try
        {
            if (ModelState.IsValid)
            {
                coupon.UpdatedAt = DateTime.UtcNow;
                await _couponService.UpdateAsync(coupon);
                TempData["Success"] = "Kupon başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Kupon güncellenemedi: {ex.Message}";
        }
        
        return View(coupon);
    }

    // POST: /Admin/Coupons/Delete/5
    [HttpPost]
    [Route("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _couponService.DeleteAsync(id);
            TempData["Success"] = "Kupon başarıyla silindi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Kupon silinemedi: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }

    // POST: /Admin/Coupons/ToggleActive/5
    [HttpPost]
    [Route("ToggleActive/{id:int}")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        try
        {
            await _couponService.ToggleActiveAsync(id);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
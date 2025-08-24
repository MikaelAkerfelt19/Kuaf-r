using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Coupons;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CouponsController : Controller
    {
        private readonly ICouponService _couponService;

        public CouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        // GET: /Admin/Coupons
        public async Task<IActionResult> Index()
        {
            var coupons = await _couponService.GetAllAsync();
            var list = coupons.Select(c => new CouponDto
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                DiscountType = (DiscountType)Enum.Parse(typeof(DiscountType), c.DiscountType),
                Amount = c.Amount,
                MinSpend = c.MinSpend,
                ExpiresAt = c.ExpiresAt,
                IsActive = c.IsActive
            }).OrderBy(c => c.Id).ToList();
            
            return View(list);
        }

        // POST: /Admin/Coupons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CouponFormModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return Redirect("/Admin/Coupons");
            }

            try
            {
                // Kod tekilliği kontrolü
                if (await _couponService.ExistsByCodeAsync(form.Code.Trim()))
                {
                    TempData["Error"] = "Bu kupon kodu zaten mevcut.";
                    return Redirect("/Admin/Coupons");
                }

                var coupon = new Coupon
                {
                    Code = form.Code.Trim(),
                    Title = form.Title.Trim(),
                    DiscountType = form.DiscountType.ToString(),
                    Amount = form.Amount,
                    MinSpend = form.MinSpend,
                    ExpiresAt = form.ExpiresAt,
                    IsActive = form.IsActive
                };

                await _couponService.CreateAsync(coupon);
                TempData["Success"] = "Kupon eklendi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kupon eklenirken hata oluştu: " + ex.Message;
            }

            return Redirect("/Admin/Coupons");
        }

        // POST: /Admin/Coupons/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CouponFormModel form)
        {
            if (!ModelState.IsValid || form.Id <= 0)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return Redirect("/Admin/Coupons");
            }

            try
            {
                var existingCoupon = await _couponService.GetByIdAsync(form.Id);
                if (existingCoupon == null)
                {
                    TempData["Error"] = "Kayıt bulunamadı.";
                    return Redirect("/Admin/Coupons");
                }

                // Kod tekilliği kontrolü 
                if (await _couponService.ExistsByCodeAsync(form.Code.Trim(), form.Id))
                {
                    TempData["Error"] = "Bu kupon kodu başka bir kayıtta var.";
                    return Redirect("/Admin/Coupons");
                }

                existingCoupon.Code = form.Code.Trim();
                existingCoupon.Title = form.Title.Trim();
                existingCoupon.DiscountType = form.DiscountType.ToString();
                existingCoupon.Amount = form.Amount;
                existingCoupon.MinSpend = form.MinSpend;
                existingCoupon.ExpiresAt = form.ExpiresAt;
                existingCoupon.IsActive = form.IsActive;

                await _couponService.UpdateAsync(existingCoupon);
                TempData["Success"] = "Kupon güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kupon güncellenirken hata oluştu: " + ex.Message;
            }

            return Redirect("/Admin/Coupons");
        }

        // POST: /Admin/Coupons/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Geçersiz istek.";
                return Redirect("/Admin/Coupons");
            }

            try
            {
                await _couponService.DeleteAsync(id);
                TempData["Success"] = "Kupon silindi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kupon silinirken hata oluştu: " + ex.Message;
            }

            return Redirect("/Admin/Coupons");
        }
    }
}

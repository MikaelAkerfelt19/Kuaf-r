using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Coupons;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CouponsController : Controller
    {
        private static readonly object _lock = new();

        // In-memory mock kupon listesi
        private static List<CouponDto> _coupons = new()
        {
            new CouponDto { Id = 1, Code = "YAZ10", Title = "Yaz İndirimi %10", DiscountType = DiscountType.Percent, Amount = 10,  MinSpend = 200, ExpiresAt = DateTime.Today.AddDays(30), IsActive = true },
            new CouponDto { Id = 2, Code = "HOSGELDIN50", Title = "Hoşgeldin 50 ₺", DiscountType = DiscountType.Amount, Amount = 50, MinSpend = null, ExpiresAt = null, IsActive = true },
            new CouponDto { Id = 3, Code = "PASIF20", Title = "Pasif Kupon %20", DiscountType = DiscountType.Percent, Amount = 20, MinSpend = 300, ExpiresAt = DateTime.Today.AddDays(-3), IsActive = false },
        };

        // GET: /Admin/Coupons
        public IActionResult Index()
        {
            var list = _coupons.OrderBy(c => c.Id).ToList();
            return View(list);
        }

        // POST: /Admin/Coupons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CouponFormModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return Redirect("/Admin/Coupons");
            }

            lock (_lock)
            {
                // Kod tekilliği (mock kontrol)
                if (_coupons.Any(c => c.Code.Equals(form.Code.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Error"] = "Bu kupon kodu zaten mevcut.";
                    return Redirect("/Admin/Coupons");
                }

                var nextId = _coupons.Count == 0 ? 1 : _coupons.Max(x => x.Id) + 1;
                _coupons.Add(new CouponDto
                {
                    Id = nextId,
                    Code = form.Code.Trim(),
                    Title = form.Title.Trim(),
                    DiscountType = form.DiscountType,
                    Amount = form.Amount,
                    MinSpend = form.MinSpend,
                    ExpiresAt = form.ExpiresAt,
                    IsActive = form.IsActive
                });
            }

            TempData["Success"] = "Kupon eklendi.";
            return Redirect("/Admin/Coupons");
        }

        // POST: /Admin/Coupons/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CouponFormModel form)
        {
            if (!ModelState.IsValid || form.Id <= 0)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return Redirect("/Admin/Coupons");
            }

            lock (_lock)
            {
                var entity = _coupons.FirstOrDefault(c => c.Id == form.Id);
                if (entity == null)
                {
                    TempData["Error"] = "Kayıt bulunamadı.";
                    return Redirect("/Admin/Coupons");
                }

                // Kod tekilliği (kendi dışında)
                if (_coupons.Any(c => c.Id != form.Id && c.Code.Equals(form.Code.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    TempData["Error"] = "Bu kupon kodu başka bir kayıtta var.";
                    return Redirect("/Admin/Coupons");
                }

                entity.Code = form.Code.Trim();
                entity.Title = form.Title.Trim();
                entity.DiscountType = form.DiscountType;
                entity.Amount = form.Amount;
                entity.MinSpend = form.MinSpend;
                entity.ExpiresAt = form.ExpiresAt;
                entity.IsActive = form.IsActive;
            }

            TempData["Success"] = "Kupon güncellendi.";
            return Redirect("/Admin/Coupons");
        }

        // POST: /Admin/Coupons/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Geçersiz istek.";
                return Redirect("/Admin/Coupons");
            }

            lock (_lock)
            {
                _coupons = _coupons.Where(c => c.Id != id).ToList();
            }

            TempData["Success"] = "Kupon silindi.";
            return Redirect("/Admin/Coupons");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Services;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ServicesController : Controller
    {
        // In-memory mock store (thread-safe basit kullanım)
        private static readonly object _lock = new();
        private static List<ServiceDto> _services = new()
        {
            new ServiceDto { Id = 1, Name = "Saç Kesimi", DurationMin = 30, Price = 250, IsActive = true },
            new ServiceDto { Id = 2, Name = "Sakal Traşı", DurationMin = 20, Price = 150, IsActive = true },
            new ServiceDto { Id = 3, Name = "Fön",        DurationMin = 15, Price = 120, IsActive = false },
        };

        // GET: /Admin/Services
        public IActionResult Index()
        {
            var list = _services.OrderBy(s => s.Id).ToList();
            return View(list);
        }

        // POST: /Admin/Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ServiceFormModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return Redirect("/Admin/Services");
            }

            lock (_lock)
            {
                var nextId = _services.Count == 0 ? 1 : _services.Max(x => x.Id) + 1;
                _services.Add(new ServiceDto
                {
                    Id = nextId,
                    Name = form.Name.Trim(),
                    DurationMin = form.DurationMin,
                    Price = form.Price,
                    IsActive = form.IsActive
                });
            }

            TempData["Success"] = "Hizmet eklendi.";
            return Redirect("/Admin/Services");
        }

        // POST: /Admin/Services/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ServiceFormModel form)
        {
            if (!ModelState.IsValid || form.Id <= 0)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return Redirect("/Admin/Services");
            }

            lock (_lock)
            {
                var entity = _services.FirstOrDefault(s => s.Id == form.Id);
                if (entity == null)
                {
                    TempData["Error"] = "Kayıt bulunamadı.";
                    return Redirect("/Admin/Services");
                }

                entity.Name = form.Name.Trim();
                entity.DurationMin = form.DurationMin;
                entity.Price = form.Price;
                entity.IsActive = form.IsActive;
            }

            TempData["Success"] = "Hizmet güncellendi.";
            return Redirect("/Admin/Services");
        }

        // POST: /Admin/Services/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Geçersiz istek.";
                return Redirect("/Admin/Services");
            }

            lock (_lock)
            {
                _services = _services.Where(s => s.Id != id).ToList();
            }

            TempData["Success"] = "Hizmet silindi.";
            return Redirect("/Admin/Services");
        }
    }
}

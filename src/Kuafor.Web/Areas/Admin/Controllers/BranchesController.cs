using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Branches;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BranchesController : Controller
    {
        private static readonly object _lock = new();

        // Mock şube listesi (Stylists modülündeki örneklerle uyumlu)
        private static List<BranchDto> _branches = new()
        {
            new BranchDto { Id = 1, Name = "Merkez Şube", IsActive = true,  Address = "İstiklal Cad. 10", Phone = "0212 000 00 00" },
            new BranchDto { Id = 2, Name = "Kadıköy",     IsActive = true,  Address = "Bahariye 25",       Phone = "0216 000 00 00" },
            new BranchDto { Id = 3, Name = "Beşiktaş",    IsActive = false, Address = "Barbaros 5",        Phone = "0212 111 11 11" }
        };

        // GET: /Admin/Branches
        public IActionResult Index()
        {
            var list = _branches.OrderBy(b => b.Id).ToList();
            return View(list);
        }

        // POST: /Admin/Branches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BranchFormModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return Redirect("/Admin/Branches");
            }

            lock (_lock)
            {
                var nextId = _branches.Count == 0 ? 1 : _branches.Max(x => x.Id) + 1;
                _branches.Add(new BranchDto
                {
                    Id = nextId,
                    Name = form.Name.Trim(),
                    Address = form.Address?.Trim() ?? string.Empty,
                    Phone = form.Phone?.Trim() ?? string.Empty,
                    IsActive = form.IsActive
                });
            }

            TempData["Success"] = "Şube eklendi.";
            return Redirect("/Admin/Branches");
        }

        // POST: /Admin/Branches/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BranchFormModel form)
        {
            if (!ModelState.IsValid || form.Id <= 0)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return Redirect("/Admin/Branches");
            }

            lock (_lock)
            {
                var entity = _branches.FirstOrDefault(b => b.Id == form.Id);
                if (entity == null)
                {
                    TempData["Error"] = "Kayıt bulunamadı.";
                    return Redirect("/Admin/Branches");
                }

                entity.Name = form.Name.Trim();
                entity.Address = form.Address?.Trim() ?? string.Empty;
                entity.Phone = form.Phone?.Trim() ?? string.Empty;
                entity.IsActive = form.IsActive;
            }

            TempData["Success"] = "Şube güncellendi.";
            return Redirect("/Admin/Branches");
        }

        // POST: /Admin/Branches/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Geçersiz istek.";
                return Redirect("/Admin/Branches");
            }

            lock (_lock)
            {
                _branches = _branches.Where(b => b.Id != id).ToList();
            }

            TempData["Success"] = "Şube silindi.";
            return Redirect("/Admin/Branches");
        }
    }
}

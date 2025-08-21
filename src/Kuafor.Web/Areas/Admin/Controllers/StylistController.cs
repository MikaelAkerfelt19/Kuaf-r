using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Kuafor.Web.Models.Admin.Stylists;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StylistsController : Controller
    {
        // Mock store
        private static readonly object _lock = new();

        private static List<BranchDto> _branches = new()
        {
            new BranchDto { Id = 1, Name = "Merkez Şube", IsActive = true, Address = "İstiklal Cad. 10", Phone = "0212 000 00 00" },
            new BranchDto { Id = 2, Name = "Kadıköy",     IsActive = true, Address = "Bahariye 25",       Phone = "0216 000 00 00" },
            new BranchDto { Id = 3, Name = "Beşiktaş",    IsActive = false, Address = "Barbaros 5",       Phone = "0212 111 11 11" }
        };

        private static List<StylistDto> _stylists = new()
        {
            new StylistDto { Id = 1, Name = "Ali Yılmaz",   Rating = 4.6m, Bio = "Klasik & modern kesimler", BranchId = 1, IsActive = true },
            new StylistDto { Id = 2, Name = "Ece Demir",    Rating = 4.9m, Bio = "Renklendirme uzmanı",      BranchId = 2, IsActive = true },
            new StylistDto { Id = 3, Name = "Mert Kaya",    Rating = 3.8m, Bio = "Sakal & fön",              BranchId = 3, IsActive = false }
        };

        private static List<SelectListItem> BuildBranchOptions()
        {
            return _branches
                .OrderBy(b => b.Name)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Name + (b.IsActive ? "" : " (Pasif)")
                })
                .ToList();
        }

        private static Dictionary<int, string> BuildBranchNames()
        {
            return _branches.ToDictionary(b => b.Id, b => b.Name);
        }

        // GET: /Admin/Stylists
        public IActionResult Index()
        {
            var vm = new StylistsPageViewModel
            {
                Stylists = _stylists.OrderBy(s => s.Id).ToList(),
                BranchOptions = BuildBranchOptions(),
                BranchNames = BuildBranchNames()
            };
            return View(vm);
        }

        // POST: /Admin/Stylists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StylistFormModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return Redirect("/Admin/Stylists");
            }

            lock (_lock)
            {
                var nextId = _stylists.Count == 0 ? 1 : _stylists.Max(x => x.Id) + 1;
                _stylists.Add(new StylistDto
                {
                    Id = nextId,
                    Name = form.Name.Trim(),
                    Rating = form.Rating,
                    Bio = string.IsNullOrWhiteSpace(form.Bio) ? null : form.Bio.Trim(),
                    BranchId = form.BranchId,
                    IsActive = form.IsActive
                });
            }

            TempData["Success"] = "Kuaför eklendi.";
            return Redirect("/Admin/Stylists");
        }

        // POST: /Admin/Stylists/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(StylistFormModel form)
        {
            if (!ModelState.IsValid || form.Id <= 0)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return Redirect("/Admin/Stylists");
            }

            lock (_lock)
            {
                var entity = _stylists.FirstOrDefault(s => s.Id == form.Id);
                if (entity == null)
                {
                    TempData["Error"] = "Kayıt bulunamadı.";
                    return Redirect("/Admin/Stylists");
                }

                entity.Name = form.Name.Trim();
                entity.Rating = form.Rating;
                entity.Bio = string.IsNullOrWhiteSpace(form.Bio) ? null : form.Bio.Trim();
                entity.BranchId = form.BranchId;
                entity.IsActive = form.IsActive;
            }

            TempData["Success"] = "Kuaför güncellendi.";
            return Redirect("/Admin/Stylists");
        }

        // POST: /Admin/Stylists/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Geçersiz istek.";
                return Redirect("/Admin/Stylists");
            }

            lock (_lock)
            {
                _stylists = _stylists.Where(s => s.Id != id).ToList();
            }

            TempData["Success"] = "Kuaför silindi.";
            return Redirect("/Admin/Stylists");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Stylists;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class StylistController : Controller
    {
        private readonly IStylistService _stylistService;
        private readonly IBranchService _branchService;

        public StylistController(IStylistService stylistService, IBranchService branchService)
        {
            _stylistService = stylistService;
            _branchService = branchService;
        }

        // GET: /Admin/Stylist
        public async Task<IActionResult> Index()
        {
            var stylists = await _stylistService.GetAllAsync();
            var branches = await _branchService.GetAllAsync();

            var list = stylists.Select(s => new StylistDto
            {
                Id = s.Id,
                Name = $"{s.FirstName} {s.LastName}",
                Rating = s.Rating,
                Bio = s.Bio,
                BranchId = s.BranchId,
                IsActive = s.IsActive
            }).OrderBy(s => s.Id).ToList();

            ViewBag.Branches = branches.Select(b => new BranchDto
            {
                Id = b.Id,
                Name = b.Name,
                IsActive = b.IsActive,
                Address = b.Address ?? string.Empty,
                Phone = b.Phone ?? string.Empty
            }).ToList();

            return View(list);
        }

        // Stylists action'ı kaldırıldı - route conflict nedeniyle
        // Artık /Admin/Stylists için StylistsController kullanılıyor
    }
}

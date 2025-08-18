using Microsoft.AspNetCore.Mvc;

namespace Kuafor.Web.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Profile = new
            {
                FullName = User?.Identity?.Name ?? "Misafir Kullanıcı",
                Email = "user@example.com",
                Phone = "+90 5xx xxx xx xx",
                Newsletter = true
            };
            return View();
        }
    }
}
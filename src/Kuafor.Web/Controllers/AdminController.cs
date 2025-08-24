using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Kuafor.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        // Admin ana sayfasından Admin area'daki Dashboard'a yönlendir
        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
    }
}

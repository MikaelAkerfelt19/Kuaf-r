using Microsoft.AspNetCore.Mvc;

namespace Kuafor.Web.Controllers;

public class TestController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

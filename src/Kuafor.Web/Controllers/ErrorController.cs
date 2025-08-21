using Microsoft.AspNetCore.Mvc;

namespace Kuafor.Web.Controllers
{
    [Route("Error")]
    public class ErrorController : Controller
    {
        [HttpGet("")]
        public IActionResult Error() => View();

        [HttpGet("{code:int}")]
        public IActionResult Status(int code)
        {
            if (code == 404) return View("NotFound");
            return View("Error");
        }
    }
}

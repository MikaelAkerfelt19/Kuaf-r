using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Kuafor.Web.Controllers
{
    [Route("")]
    public class LegalController : Controller
    {
        // /gizlilik-politikasi
        [HttpGet("gizlilik-politikasi")]
        public IActionResult Privacy()
        {
            ViewData["Title"] = "Gizlilik Politikası";
            ViewData["LastUpdated"] = DateTime.Today.ToString("dd MMMM yyyy", new CultureInfo("tr-TR"));
            return View();
        }

        // /kullanim-sartlari
        [HttpGet("kullanim-sartlari")]
        public IActionResult Terms()
        {
            ViewData["Title"] = "Kullanım Şartları";
            ViewData["LastUpdated"] = DateTime.Today.ToString("dd MMMM yyyy", new CultureInfo("tr-TR"));
            return View();
        }
    }
}

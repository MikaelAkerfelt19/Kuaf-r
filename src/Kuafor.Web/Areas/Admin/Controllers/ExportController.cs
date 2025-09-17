using Microsoft.AspNetCore.Mvc;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    public class ExportController : Controller
    {
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            // Export/Import ana sayfası
            return View();
        }

        [HttpGet("download-customer-template")]
        public IActionResult DownloadCustomerTemplate()
        {
            // Müşteri Excel şablonu indir
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "customer_template.xlsx");
            
            if (System.IO.File.Exists(templatePath))
            {
                return File(System.IO.File.ReadAllBytes(templatePath), 
                           "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                           "musteri_sablonu.xlsx");
            }
            
            // Şablon yoksa basit bir Excel oluştur
            return RedirectToAction("Index");
        }

        [HttpGet("download-service-template")]
        public IActionResult DownloadServiceTemplate()
        {
            // Hizmet Excel şablonu indir
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "service_template.xlsx");
            
            if (System.IO.File.Exists(templatePath))
            {
                return File(System.IO.File.ReadAllBytes(templatePath), 
                           "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                           "hizmet_sablonu.xlsx");
            }
            
            return RedirectToAction("Index");
        }
    }
}

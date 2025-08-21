using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles="Admin")] // TODO: Backend hazır olduğunda aç
    public class DashboardController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var vm = new AdminDashboardViewModel
            {
                Kpis =
                {
                    new KpiVm { Label = "Bugün Randevu", Value = "18", SubText = "Son 7g ort: 16", Badge = "+12%" },
                    new KpiVm { Label = "Haftalık Randevu", Value = "94", SubText = "Hedef: 120", Badge = "-6%" },
                    new KpiVm { Label = "Aktif Kuaför", Value = "7", SubText = "Toplam 9" },
                    new KpiVm { Label = "Bugünkü Ciro", Value = "₺8.450", SubText = "Tahmini", Badge = "+5%" }
                },
                Upcoming =
                {
                    new UpcomingApptRow(1201, DateTime.Today.AddHours(14), "Cemre Y.", "Saç Kesimi", "Ahmet Ö.", "Kadıköy"),
                    new UpcomingApptRow(1202, DateTime.Today.AddHours(15), "Efe T.",   "Bakım & Spa", "İbrahim T.", "Moda"),
                    new UpcomingApptRow(1203, DateTime.Today.AddHours(16), "Mina A.",  "Boya & Röfle", "Ahmet Ü.", "Erenköy")
                },
                TopServices =
                {
                    new TopServiceRow("Saç Kesimi", 42, "+"),
                    new TopServiceRow("Bakım & Spa", 31, "•"),
                    new TopServiceRow("Boya & Röfle", 27, "↑")
                }
            };
            return View(vm);
        }
    }
}
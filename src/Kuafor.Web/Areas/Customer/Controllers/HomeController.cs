using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models;
using System;

namespace Kuafor.Web.Areas.Customer.Controllers
{
    // Not: Bu controller sadece dashboard görünümünü göstermek içindir.
    // ViewModel, veri erişimi vb. kısımlar eklenecek.
    [Area("Customer")]
    public class HomeController : Controller
    {
        // GET: /Customer
        public IActionResult Index()
        {
            var vm = new CustomerDashboardViewModel
            {
                Upcoming = new UpcomingAppointmentVm
                {
                    HasAppointment = true, // Boş durumu görmek için false yapılabilir.
                    StartTime = DateTime.Today.AddDays(2).AddHours(14).AddMinutes(30),
                    ServiceName = "Saç Kesimi",
                    StylistName = "Ahmet Özdoğan",
                    Branch = "Merkez",
                    AppointmentId = 123
                }
            };
            // TODO: Kullanıcıya ait yaklaşan randevular vb. ViewModel ile doldurulacak.
            return View(vm);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Appointments;
using System;

namespace Kuafor.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AppointmentsController : Controller
    {
        private List<ServiceVm> MockServices => new()
        {
            new(1, "Saç Kesimi", "30 dakika", "Temel kesim, model önerisi"),
            new(2, "Boya & Röfle", "90-150 dakika", "Renk değişimi, röfle"),
            new(3, "Bakım & Spa", "45-60 dakika", "Keratin, nem desteği"),
        };

        private List<StylistVm> MockStylists => new()
        {
            new(10, "Ahmet Özdoğan", 4.5, "Kesim ve stil uzmanı"),
            new(11, "Ahmet Ülker", 4.8, "Renk ve bakım"),
            new(12, "İbrahim Taşkın", 4.2, "Erkek kesim ve sakal"),
        };

        private List<TimeSlotVm> BuildSlots()
        {
            var start = DateTime.Today.AddDays(2).AddHours(10); // 2 gün sonrası, sabah 10
            var slots = new List<TimeSlotVm>();
            for (int i = 0; i < 16; i++) // 10:00-18:00 arası her 30 dk
            {
                var t = start.AddMinutes(30 * i);
                slots.Add(new TimeSlotVm(t, IsAvailable: i % 5 != 0)); // her 5'te 1 dolu
            }
            return slots;
        }

        public IActionResult New(int? serviceId, int? stylistId, string? start)
        {
            var vm = new AppointmentWizardViewModel
            {
                Services = MockServices,
                Stylists = MockStylists,
                TimeSlots = BuildSlots()
            };

            if (serviceId.HasValue) { vm.SelectedServiceId = serviceId; vm.Step = WizardStep.Stylist; }
            if (stylistId.HasValue) { vm.SelectedStylistId = stylistId; vm.Step = WizardStep.Time; }
            if (!string.IsNullOrEmpty(start) && DateTime.TryParse(start, out var dt))
            { vm.SelectedStart = dt; vm.Step = WizardStep.Confirm; }

            return View(vm);
        }

        public IActionResult Index()
        {
            // Mock listeler
            var upcomings = new[]
            {
                new { Id = 501, When = DateTime.Today.AddDays(3).AddHours(14).AddMinutes(30), Service = "Saç Kesimi", Stylist = "Ahmet Özdoğan", Branch = "Merkez" },
                new { Id = 502, When = DateTime.Today.AddDays(7).AddHours(11), Service = "Boya & Röfle", Stylist = "Ahmet Ülker", Branch = "Merkez" },
            };
            var past = new[]
            {
                new { Id = 401, When = DateTime.Today.AddDays(-10).AddHours(16), Service = "Bakım & Spa", Stylist = "İbrahim Taşkın", Branch = "Merkez" }
            };

            ViewBag.Upcoming = upcomings;
            ViewBag.Past = past;
            return View();
        }

        public IActionResult Details(int id)
        {
            // Mock detay
            ViewBag.Model = new
            {
                Id = id,
                When = DateTime.Today.AddDays(3).AddHours(14).AddMinutes(30),
                Service = "Saç Kesimi",
                Stylist = "Ahmet Özdoğan",
                Branch = "Merkez",
                Notes = "Omuz hizası kesim, katlı."
            };
            return View();
        }

        [HttpPost]
        public IActionResult Confirm(int serviceId, int stylistId, DateTime start)
        {
            // TODO: Backend geldiğinde kaydet
            TempData["Booked"] = $"Randevu oluşturuldu: {start:dd MMM dddd, HH:mm} · {MockServices.First(s => s.Id == serviceId).Name} · {MockStylists.First(s => s.Id == stylistId).Name}";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Cancel(int id)
        {
            // TODO: Backend geldiğinde iptal et
            TempData["Booked"] = $"Randevu iptal edildi: {id}";
            return RedirectToAction("Index");
        }

        public IActionResult Reschedule(int id, string? start)
        {
            if (!string.IsNullOrWhiteSpace(start) && DateTime.TryParse(start, out var dt))
            {
                TempData["Booked"] = $"Randevu #{id} yeniden planlandı: {dt:dd MMM dddd, HH:mm}";
                return RedirectToAction("Index");
            }

            // Saat seç ekranı (step3 benzeri)
            var vm = new AppointmentWizardViewModel
            {
                Step = WizardStep.Time,
                Services = MockServices,
                Stylists = MockStylists,
                TimeSlots = BuildSlots()
            };
            ViewBag.RescheduleForId = id;
            return View("Reschedule", vm);
        }
    }
}
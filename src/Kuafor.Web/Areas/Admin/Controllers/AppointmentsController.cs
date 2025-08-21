using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Appointments;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AppointmentsController : Controller
    {
        private static readonly object _lock = new();

        // Mock referans verileri (id/ad eşlemleri)
        private static readonly Dictionary<int, string> _branches = new()
        {
            { 1, "Merkez Şube" }, { 2, "Kadıköy" }, { 3, "Beşiktaş" }
        };
        private static readonly Dictionary<int, string> _stylists = new()
        {
            { 1, "Ali Yılmaz" }, { 2, "Ece Demir" }, { 3, "Mert Kaya" }
        };

        // In-memory randevu listesi
        private static List<AppointmentDto> _appointments = new()
        {
            new AppointmentDto {
                Id = 1, StartAt = DateTime.Today.AddHours(11), DurationMin = 30,
                CustomerName = "Burak A.", BranchId = 1, StylistId = 1,
                ServiceName = "Saç Kesimi", Price = 250, Status = AppointmentStatus.Scheduled,
                Note = "Yanlar kısa, üst doğal."
            },
            new AppointmentDto {
                Id = 2, StartAt = DateTime.Today.AddHours(13), DurationMin = 45,
                CustomerName = "Dilara K.", BranchId = 2, StylistId = 2,
                ServiceName = "Renklendirme", Price = 900, Status = AppointmentStatus.Scheduled
            },
            new AppointmentDto {
                Id = 3, StartAt = DateTime.Today.AddDays(-1).AddHours(18), DurationMin = 20,
                CustomerName = "Emre S.", BranchId = 3, StylistId = 3,
                ServiceName = "Sakal Traşı", Price = 150, Status = AppointmentStatus.Completed
            },
            new AppointmentDto {
                Id = 4, StartAt = DateTime.Today.AddDays(2).AddHours(10), DurationMin = 30,
                CustomerName = "Gizem T.", BranchId = 1, StylistId = 2,
                ServiceName = "Fön", Price = 120, Status = AppointmentStatus.Rescheduled,
                Note = "Toplantı öncesi."
            },
            new AppointmentDto {
                Id = 5, StartAt = DateTime.Today.AddDays(3).AddHours(16), DurationMin = 60,
                CustomerName = "Onur N.", BranchId = 2, StylistId = 1,
                ServiceName = "Renk Açma", Price = 450, Status = AppointmentStatus.Cancelled
            }
        };

        // GET: /Admin/Appointments
        public IActionResult Index([FromQuery] AppointmentFilter filter)
        {
            IEnumerable<AppointmentDto> query = _appointments;

            if (filter.From.HasValue)
            {
                var from = filter.From.Value.Date;
                query = query.Where(a => a.StartAt.Date >= from);
            }
            if (filter.To.HasValue)
            {
                var to = filter.To.Value.Date;
                query = query.Where(a => a.StartAt.Date <= to);
            }
            if (filter.BranchId.HasValue)
                query = query.Where(a => a.BranchId == filter.BranchId.Value);

            if (filter.StylistId.HasValue)
                query = query.Where(a => a.StylistId == filter.StylistId.Value);

            if (filter.Status.HasValue)
                query = query.Where(a => a.Status == filter.Status.Value);

            var list = query.OrderBy(a => a.StartAt).ToList();

            var vm = new AppointmentsPageViewModel
            {
                Filter = filter,
                Items = list,
                BranchNames = new Dictionary<int, string>(_branches),
                StylistNames = new Dictionary<int, string>(_stylists)
            };

            return View(vm);
        }

        // POST: /Admin/Appointments/Reschedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reschedule(RescheduleForm form)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Form hatalı. Lütfen alanları kontrol edin.";
                return Redirect("/Admin/Appointments");
            }

            lock (_lock)
            {
                var entity = _appointments.FirstOrDefault(x => x.Id == form.Id);
                if (entity == null)
                {
                    TempData["Error"] = "Kayıt bulunamadı.";
                    return Redirect("/Admin/Appointments");
                }

                entity.StartAt = form.NewStartAt;
                entity.DurationMin = form.DurationMin;
                entity.Status = AppointmentStatus.Rescheduled;
            }

            TempData["Success"] = "Randevu ertelendi.";
            return Redirect("/Admin/Appointments");
        }

        // POST: /Admin/Appointments/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(CancelForm form)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Geçersiz istek.";
                return Redirect("/Admin/Appointments");
            }

            lock (_lock)
            {
                var entity = _appointments.FirstOrDefault(x => x.Id == form.Id);
                if (entity == null)
                {
                    TempData["Error"] = "Kayıt bulunamadı.";
                    return Redirect("/Admin/Appointments");
                }
                entity.Status = AppointmentStatus.Cancelled;
                // Notu formdan ekleyebiliriz (mock)
                if (!string.IsNullOrWhiteSpace(form.Reason))
                {
                    entity.Note = $"(İptal) {form.Reason}".Trim();
                }
            }

            TempData["Success"] = "Randevu iptal edildi.";
            return Redirect("/Admin/Appointments");
        }
    }
}

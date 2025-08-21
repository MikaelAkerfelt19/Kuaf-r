using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Models.Admin.Reports;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportsController : Controller
    {
        // NOT: Bu controller bağımsız mock veri kullanır.
        // İstersen tek bir "MockDb" sınıfı oluşturup diğer controller'lardaki listeleri de oraya alabiliriz.

        private static readonly Dictionary<int, string> _branches = new()
        {
            { 1, "Merkez Şube" }, { 2, "Kadıköy" }, { 3, "Beşiktaş" }
        };
        private static readonly Dictionary<int, bool> _branchActive = new()
        {
            { 1, true }, { 2, true }, { 3, false }
        };

        private static readonly Dictionary<int, string> _stylists = new()
        {
            { 1, "Ali Yılmaz" }, { 2, "Ece Demir" }, { 3, "Mert Kaya" }
        };
        private static readonly Dictionary<int, bool> _stylistActive = new()
        {
            { 1, true }, { 2, true }, { 3, false }
        };

        private static readonly List<(int Id, string Name, bool Active, decimal Price, int Duration)> _services = new()
        {
            (1, "Saç Kesimi", true, 250m, 30),
            (2, "Sakal Traşı", true, 150m, 20),
            (3, "Fön",        false,120m, 15),
            (4, "Renklendirme", true, 900m, 60)
        };

        private enum Status { Scheduled = 1, Completed = 2, Cancelled = 3, Rescheduled = 4 }

        private static readonly List<(int Id, DateTime StartAt, int Duration, int BranchId, int StylistId, int ServiceId, decimal Price, Status St)> _appointments
            = SeedAppointments();

        private static List<(int, DateTime, int, int, int, int, decimal, Status)> SeedAppointments()
        {
            var today = DateTime.Today;
            // Haftaya yayılmış küçük bir örnek set
            var list = new List<(int, DateTime, int, int, int, int, decimal, Status)>
            {
                (1, today.AddHours(10), 30, 1, 1, 1, 250m, Status.Scheduled),
                (2, today.AddHours(14), 45, 2, 2, 4, 900m, Status.Scheduled),
                (3, today.AddDays(-1).AddHours(18), 20, 3, 3, 2, 150m, Status.Completed),
                (4, today.AddDays(1).AddHours(11), 60, 1, 2, 4, 900m, Status.Rescheduled),
                (5, today.AddDays(2).AddHours(16), 30, 2, 1, 1, 250m, Status.Scheduled),
                (6, today.AddDays(5).AddHours(13), 30, 1, 1, 1, 250m, Status.Cancelled),
            };
            return list;
        }

        // GET: /Admin/Reports
        public IActionResult Index()
        {
            var now = DateTime.Now;
            var today = DateTime.Today;

            // Bu haftanın başlangıcı (Pazartesi)
            int delta = ((int)today.DayOfWeek + 6) % 7; // Pazartesi=0
            var weekStart = today.AddDays(-delta);
            var weekEnd = weekStart.AddDays(7).AddTicks(-1);

            // Kart metrikleri
            int todayCount = _appointments.Count(a => a.StartAt.Date == today);
            int weekCount = _appointments.Count(a => a.StartAt >= weekStart && a.StartAt <= weekEnd);
            // Ciro tahmini (mock): Haftada "Scheduled/Rescheduled/Completed" statülerinin toplam Price'ı
            decimal weekRevenue = _appointments
                .Where(a => a.StartAt >= weekStart && a.StartAt <= weekEnd && a.St != Status.Cancelled)
                .Sum(a => a.Price);

            int activeStylists = _stylistActive.Count(kv => kv.Value);
            int activeBranches = _branchActive.Count(kv => kv.Value);
            int activeServices = _services.Count(s => s.Active);

            // Önümüzdeki 7 gün
            var next7 = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var d = today.AddDays(i);
                    var cnt = _appointments.Count(a => a.StartAt.Date == d.Date && a.St != Status.Cancelled);
                    return new DayBucket { Day = d, Count = cnt };
                })
                .ToList();

            // Bu hafta en çok randevu alan kuaförler
            var top = _appointments
                .Where(a => a.StartAt >= weekStart && a.StartAt <= weekEnd && a.St != Status.Cancelled)
                .GroupBy(a => a.StylistId)
                .Select(g => new TopStylist
                {
                    Name = _stylists.TryGetValue(g.Key, out var n) ? n : $"#{g.Key}",
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Name)
                .Take(5)
                .ToList();

            var vm = new ReportsViewModel
            {
                TodayAppointments = todayCount,
                WeekAppointments = weekCount,
                WeekRevenueEstimate = weekRevenue,
                ActiveStylists = activeStylists,
                ActiveBranches = activeBranches,
                ActiveServices = activeServices,
                Next7Days = next7,
                TopStylistsByWeek = top
            };

            return View(vm);
        }
    }
}

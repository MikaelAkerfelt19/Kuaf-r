using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Admin.Reports
{
    public class ReportsViewModel
    {
        // Kartlar
        public int TotalAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public int WeekAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal WeeklyRevenue { get; set; }
        public decimal WeekRevenueEstimate { get; set; }

        public int ActiveStylists { get; set; }
        public int ActiveBranches { get; set; }
        public int ActiveServices { get; set; }

        // Tablolar / özetler
        public List<DayBucket> Next7Days { get; set; } = new();         // Önümüzdeki 7 gün adet
        public List<TopStylist> TopStylistsByWeek { get; set; } = new(); // Bu hafta en çok randevu
        public List<TopStylist> TopStylistsByMonth { get; set; } = new(); // Bu ay en çok randevu
        public List<BranchPerformance> BranchPerformance { get; set; } = new(); // Şube performansı
    }

    public class DayBucket
    {
        [DataType(DataType.Date)]
        public DateTime Day { get; set; }
        public int Count { get; set; }
    }

    public class TopStylist
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class BranchPerformance
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
    }
}

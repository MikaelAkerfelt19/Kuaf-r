using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Models.Admin.Branches
{
    public class BranchDetailsViewModel
    {
        public Branch Branch { get; set; } = new();
        public int TotalAppointments { get; set; }
        public int TotalStylists { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public double AverageRating { get; set; }
        public int ActiveStylists { get; set; }
        public int MonthlyAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Appointment> RecentAppointments { get; set; } = new();
        public List<Entities.Stylist> BranchStylists { get; set; } = new();
    }
}

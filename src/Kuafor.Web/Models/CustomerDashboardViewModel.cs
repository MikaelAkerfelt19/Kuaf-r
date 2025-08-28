using System;

namespace Kuafor.Web.Models
{
    public class CustomerDashboardViewModel
    {
        public List<AppointmentSummaryVm> UpcomingAppointments { get; set; } = new();
        public int TotalAppointments { get; set; }
        public List<RecentServiceVm> RecentServices { get; set; } = new();
    }

    public class AppointmentSummaryVm
    {
        public int Id { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string StylistName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class RecentServiceVm
    {
        public string Name { get; set; } = string.Empty;
        public DateTime LastUsed { get; set; }
        public int UsageCount { get; set; }
    }
}
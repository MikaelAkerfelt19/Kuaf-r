using System;

namespace Kuafor.Web.Models
{
    public class UpcomingAppointmentVm
    {
        public bool HasAppointment { get; set; }
        public DateTime? StartTime { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string StylistName { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public int? AppointmentId { get; set; }
    }
    public class CustomerDashboardViewModel
    {
        public UpcomingAppointmentVm Upcoming { get; set; } = new();
    }
}
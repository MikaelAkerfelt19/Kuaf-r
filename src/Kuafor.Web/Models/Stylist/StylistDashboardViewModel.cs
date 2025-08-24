using System;
using System.Collections.Generic;

namespace Kuafor.Web.Models.Stylist
{
    public class StylistDashboardViewModel
    {
        public List<AppointmentViewModel> TodayAppointments { get; set; } = new();
        public List<AppointmentViewModel> UpcomingAppointments { get; set; } = new();
        public int TotalCompletedAppointments { get; set; }
        public int TotalCancelledAppointments { get; set; }
    }

    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public string Status { get; set; } = string.Empty;

        public DateTime EndTime => StartTime.AddMinutes(Duration);
        public string FormattedTime => $"{StartTime:HH:mm} - {EndTime:HH:mm}";
        
        public string StatusBadgeClass => Status.ToLower() switch
        {
            "scheduled" => "bg-primary",
            "completed" => "bg-success",
            "cancelled" => "bg-danger",
            "rescheduled" => "bg-warning text-dark",
            _ => "bg-secondary"
        };
    }
}
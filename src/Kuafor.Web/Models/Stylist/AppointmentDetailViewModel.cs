using System;

namespace Kuafor.Web.Models.Stylist
{
    public class AppointmentDetailViewModel
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
        public string? ServiceName { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }

        public string FormattedTime => StartTime.ToString("HH:mm");
        public string FormattedDate => StartTime.ToString("dd MMMM yyyy, dddd");
        public DateTime EndTime => StartTime.AddMinutes(Duration);
        public string FormattedEndTime => EndTime.ToString("HH:mm");

        public string? StatusBadgeClass => Status?.ToLower() switch
        {
            "scheduled" => "bg-primary",
            "completed" => "bg-success",
            "cancelled" => "bg-danger",
            "no-show" => "bg-warning",
            _ => "bg-secondary"
        };
    }
}
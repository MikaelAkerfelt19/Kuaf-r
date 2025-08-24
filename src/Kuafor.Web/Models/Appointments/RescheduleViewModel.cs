using System.ComponentModel.DataAnnotations;
using Kuafor.Web.Models.Appointments;

namespace Kuafor.Web.Models.Appointments;

public class RescheduleViewModel
{
    public int AppointmentId { get; set; }
    
    [Required(ErrorMessage = "Yeni tarih/saat zorunludur")]
    public DateTime NewStartTime { get; set; }
    
    public DateTime CurrentStartTime { get; set; }
    public DateTime CurrentEndTime { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string StylistName { get; set; } = string.Empty;
    public List<TimeSlotVm> AvailableTimeSlots { get; set; } = new();
}

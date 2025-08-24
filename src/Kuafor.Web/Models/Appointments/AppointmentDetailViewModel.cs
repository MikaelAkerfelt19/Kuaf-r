using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Models.Appointments;

public class AppointmentDetailViewModel
{
    public Appointment Appointment { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public global::Kuafor.Web.Models.Entities.Stylist Stylist { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}

using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Models.Appointments;

public class AppointmentDetailViewModel
{
    public Appointment Appointment { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public global::Kuafor.Web.Models.Entities.Stylist Stylist { get; set; } = null!;
    public Branch Branch { get; set; } = null!;

    // Default constructor
    public AppointmentDetailViewModel()
    {
    }

    // Constructor with parameters
    public AppointmentDetailViewModel(Appointment appointment, Service service, global::Kuafor.Web.Models.Entities.Stylist stylist, Branch branch)
    {
        Appointment = appointment ?? throw new ArgumentNullException(nameof(appointment));
        Service = service ?? throw new ArgumentNullException(nameof(service));
        Stylist = stylist ?? throw new ArgumentNullException(nameof(stylist));
        Branch = branch ?? throw new ArgumentNullException(nameof(branch));
    }
}

using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Models.Services;

public class ServiceDetailsViewModel
{
    public Service Service { get; set; } = null!;
    public List<global::Kuafor.Web.Models.Entities.Stylist> Stylists { get; set; } = new();
    public List<Service> RelatedServices { get; set; } = new();
}

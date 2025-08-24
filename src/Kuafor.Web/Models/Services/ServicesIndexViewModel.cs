using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Models.Services;

public class ServicesIndexViewModel
{
    public List<Service> Services { get; set; } = new();
    public List<Branch> Branches { get; set; } = new();
}
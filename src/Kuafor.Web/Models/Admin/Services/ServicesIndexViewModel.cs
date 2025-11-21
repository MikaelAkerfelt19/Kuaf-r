using Kuafor.Web.Models.Entities;
using System.Collections.Generic;

namespace Kuafor.Web.Models.Admin.Services;

public class ServicesIndexViewModel
{
    public List<ServiceDto> Services { get; set; } = new();
    public List<Stylist> Stylists { get; set; } = new();
}

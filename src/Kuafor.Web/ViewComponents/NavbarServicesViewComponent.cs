using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.ViewComponents;

public class NavbarServicesViewComponent : ViewComponent
{
    private readonly IServiceService _serviceService;

    public NavbarServicesViewComponent(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            var services = await _serviceService.GetActiveAsync();
            return View(services.Take(5).ToList()); // Navbar için maksimum 5 hizmet
        }
        catch
        {
            return View(new List<Kuafor.Web.Models.Entities.Service>());
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.ViewComponents;

public class WorkingHoursViewComponent : ViewComponent
{
    private readonly IWorkingHoursService _workingHoursService;

    public WorkingHoursViewComponent(IWorkingHoursService workingHoursService)
    {
        _workingHoursService = workingHoursService;
    }

    public async Task<IViewComponentResult> InvokeAsync(int branchId)
    {
        var workingHours = await _workingHoursService.GetByBranchAsync(branchId);
        return View(workingHours);
    }
}

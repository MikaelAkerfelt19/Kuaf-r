using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Customer.Controllers;

[Area("Customer")]
[Route("Customer/[controller]")]
public class StylistsController : Controller
{
    private readonly IStylistService _stylistService;
    private readonly IBranchService _branchService;

    public StylistsController(IStylistService stylistService, IBranchService branchService)
    {
        _stylistService = stylistService;
        _branchService = branchService;
    }

    // GET: /Customer/Stylists
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index()
    {
        var stylists = await _stylistService.GetActiveAsync();
        var branches = await _branchService.GetActiveAsync();
        
        ViewBag.Branches = branches;
        return View(stylists);
    }

    // GET: /Customer/Stylists/Details/5
    [HttpGet]
    [Route("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var stylist = await _stylistService.GetByIdAsync(id);
        if (stylist == null)
        {
            return NotFound();
        }
        
        return View(stylist);
    }
}
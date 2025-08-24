using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Controllers.Api.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class StylistsController : ControllerBase
{
    private readonly IStylistService _stylistService;
    
    public StylistsController(IStylistService stylistService)
    {
        _stylistService = stylistService;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Stylist>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var stylists = await _stylistService.GetAllAsync();
        return Ok(stylists);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Stylist), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var stylist = await _stylistService.GetByIdAsync(id);
        if (stylist == null)
            return NotFound();
            
        return Ok(stylist);
    }
    
    [HttpGet("branch/{branchId}")]
    [ProducesResponseType(typeof(IEnumerable<Stylist>), 200)]
    public async Task<IActionResult> GetByBranch(int branchId)
    {
        var stylists = await _stylistService.GetByBranchAsync(branchId);
        return Ok(stylists);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Stylist), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Stylist stylist)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var created = await _stylistService.CreateAsync(stylist);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Stylist), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int id, [FromBody] Stylist stylist)
    {
        if (id != stylist.Id)
            return BadRequest();
            
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        try
        {
            var updated = await _stylistService.UpdateAsync(stylist);
            return Ok(updated);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        await _stylistService.DeleteAsync(id);
        return NoContent();
    }
}

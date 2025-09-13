using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Controllers.Api.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize] // JWT token gerekli
public class ServicesController : ControllerBase
{
    private readonly IServiceService _serviceService;
    
    public ServicesController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Service>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var services = await _serviceService.GetAllAsync();
        return Ok(services);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Service), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var service = await _serviceService.GetByIdAsync(id);
        if (service == null)
            return NotFound();
            
        return Ok(service);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Service), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Service service)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var created = await _serviceService.CreateAsync(service);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Service), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int id, [FromBody] Service service)
    {
        if (id != service.Id)
            return BadRequest();
            
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        try
        {
            var updated = await _serviceService.UpdateAsync(service);
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
        await _serviceService.DeleteAsync(id);
        return NoContent();
    }
}

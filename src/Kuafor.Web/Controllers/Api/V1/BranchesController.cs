using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Controllers.Api.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;
    
    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Branch>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var branches = await _branchService.GetAllAsync();
        return Ok(branches);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Branch), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var branch = await _branchService.GetByIdAsync(id);
        if (branch == null)
            return NotFound();
            
        return Ok(branch);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Branch), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Branch branch)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var created = await _branchService.CreateAsync(branch);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Branch), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int id, [FromBody] Branch branch)
    {
        if (id != branch.Id)
            return BadRequest();
            
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        try
        {
            var updated = await _branchService.UpdateAsync(branch);
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
        await _branchService.DeleteAsync(id);
        return NoContent();
    }
}

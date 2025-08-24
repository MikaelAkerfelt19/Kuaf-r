using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Controllers.Api.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CouponsController : ControllerBase
{
    private readonly ICouponService _couponService;
    
    public CouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Coupon>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var coupons = await _couponService.GetAllAsync();
        return Ok(coupons);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Coupon), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var coupon = await _couponService.GetByIdAsync(id);
        if (coupon == null)
            return NotFound();
            
        return Ok(coupon);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Coupon), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Coupon coupon)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var created = await _couponService.CreateAsync(coupon);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Coupon), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(int id, [FromBody] Coupon coupon)
    {
        if (id != coupon.Id)
            return BadRequest();
            
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        try
        {
            var updated = await _couponService.UpdateAsync(coupon);
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
        await _couponService.DeleteAsync(id);
        return NoContent();
    }
    
    [HttpPost("validate")]
    [ProducesResponseType(typeof(CouponValidationResult), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Validate([FromBody] CouponValidationRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var result = await _couponService.ValidateAsync(request.Code, request.BasketTotal, request.CustomerId);
        return Ok(result);
    }
}

public class CouponValidationRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;
    
    [Required, Range(0, 999999)]
    public decimal BasketTotal { get; set; }
    
    public int? CustomerId { get; set; }
}

using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Controllers.Api.V1;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Appointment>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? branchId,
        [FromQuery] int? stylistId)
    {
        IEnumerable<Appointment> appointments;

        if (stylistId.HasValue)
            appointments = await _appointmentService.GetByStylistAsync(stylistId.Value, from, to);
        else if (branchId.HasValue)
            appointments = await _appointmentService.GetByBranchAsync(branchId.Value, from, to);
        else
            appointments = await _appointmentService.GetAllAsync();

        return Ok(appointments);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Appointment), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null)
            return NotFound();

        return Ok(appointment);
    }

    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<Appointment>), 200)]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
        var appointments = await _appointmentService.GetByCustomerAsync(customerId);
        return Ok(appointments);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Appointment), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] Appointment appointment)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var created = await _appointmentService.CreateAsync(appointment);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/reschedule")]
    [ProducesResponseType(typeof(Appointment), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleRequest request)
    {
        try
        {
            var updated = await _appointmentService.RescheduleAsync(id, request.NewStartAt);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("çakışma"))
            {
                return Conflict(new { message = ex.Message });
            }
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(Appointment), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelRequest request)
    {
        try
        {
            var updated = await _appointmentService.CancelAsync(id, request.Reason);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

public class RescheduleRequest
{
    [Required]
    public DateTime NewStartAt { get; set; }
}

public class CancelRequest
{
    public string? Reason { get; set; }
}
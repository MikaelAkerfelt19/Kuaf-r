using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ApplicationDbContext _context;
    
    public AppointmentService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Include(a => a.Customer)
            .Include(a => a.Coupon)
            .OrderByDescending(a => a.StartAt)
            .ToListAsync();
    }
    
    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Include(a => a.Customer)
            .Include(a => a.Coupon)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
    
    public async Task<IEnumerable<Appointment>> GetByCustomerAsync(int customerId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Where(a => a.CustomerId == customerId);

        if (from.HasValue)
            query = query.Where(a => a.StartTime >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.StartTime <= to.Value);

        return await query.OrderBy(a => a.StartTime).ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByStylistAsync(int stylistId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Where(a => a.StylistId == stylistId);

        if (from.HasValue)
            query = query.Where(a => a.StartTime >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.StartTime <= to.Value);

        return await query.OrderBy(a => a.StartTime).ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByBranchAsync(int branchId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Where(a => a.BranchId == branchId);

        if (from.HasValue)
            query = query.Where(a => a.StartTime >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.StartTime <= to.Value);

        return await query.OrderBy(a => a.StartTime).ToListAsync();
    }

    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        // Zaman çakışması kontrolü 
        if (await HasTimeConflictAsync(appointment.StylistId, appointment.StartAt, appointment.EndAt))
        {
            throw new InvalidOperationException("Seçilen zaman diliminde başka bir randevu bulunmaktadır");
        }
        
        appointment.CreatedAt = DateTime.UtcNow;
        appointment.Status = "Scheduled";
        
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        
        return appointment;
    }
    
    public async Task<Appointment> RescheduleAsync(int id, DateTime newStartAt)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            throw new ArgumentException("Appointment not found");

        appointment.StartAt = newStartAt;
        appointment.EndAt = newStartAt.AddMinutes(30); // Default 30 minutes
        appointment.Status = "Rescheduled";
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task<Appointment> CancelAsync(int id, string? reason = null)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            throw new ArgumentException("Appointment not found");

        appointment.Status = "Cancelled";
        appointment.CancellationReason = reason;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return appointment;
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Appointments.AnyAsync(a => a.Id == id);
    }
    
    public async Task<bool> HasTimeConflictAsync(int stylistId, DateTime startAt, DateTime endAt, int? excludeId = null)
    {
        return !await IsTimeSlotAvailableAsync(stylistId, startAt, endAt, excludeId);
    }

    public async Task<bool> HasConflictAsync(int stylistId, DateTime startAt, DateTime endAt, int? excludeId = null)
    {
        return await HasTimeConflictAsync(stylistId, startAt, endAt, excludeId);
    }

    public async Task<List<string>> GetAvailableSlotsAsync(int stylistId, DateTime date, int durationMin)
    {
        var slots = new List<string>();
        var startHour = 9; // 9:00 AM
        var endHour = 18; // 6:00 PM

        for (int hour = startHour; hour < endHour; hour++)
        {
            for (int minute = 0; minute < 60; minute += 30)
            {
                var slotStart = date.AddHours(hour).AddMinutes(minute);
                var slotEnd = slotStart.AddMinutes(durationMin);

                if (await IsTimeSlotAvailableAsync(stylistId, slotStart, slotEnd))
                {
                    slots.Add(slotStart.ToString("HH:mm"));
                }
            }
        }

        return slots;
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Customer)
            .Include(a => a.Branch) // Branch include'ı eklendi
            .Where(a => a.StartAt >= start && a.StartAt < end)
            .OrderBy(a => a.StartAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);
        return await GetByDateRangeAsync(start, end);
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAsync(int customerId)
    {
        var now = DateTime.UtcNow;
        return await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Where(a => a.CustomerId == customerId && 
                       a.StartTime > now && 
                       a.Status != "Cancelled")
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }
    
    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        appointment.UpdatedAt = DateTime.UtcNow;
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
        
        return appointment;
    }

    // Yeni eklenen method'lar
    public async Task<int> GetCountAsync()
    {
        return await _context.Appointments.CountAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _context.Appointments
            .Where(a => a.Status == "Completed")
            .SumAsync(a => a.TotalPrice);
    }

    public async Task<decimal> GetRevenueByDateAsync(DateTime date)
    {
        return await _context.Appointments
            .Where(a => a.Status == "Completed" && 
                       a.StartTime.Date == date.Date)
            .SumAsync(a => a.TotalPrice);
    }

    public async Task<decimal> GetRevenueByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.Appointments
            .Where(a => a.Status == "Completed" && 
                       a.StartTime.Date >= start.Date && 
                       a.StartTime.Date <= end.Date)
            .SumAsync(a => a.TotalPrice);
    }

    public async Task<IEnumerable<Appointment>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Where(a => a.CustomerId == customerId)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByStylistIdAsync(int stylistId)
    {
        return await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Where(a => a.StylistId == stylistId)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByBranchIdAsync(int branchId)
    {
        return await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Where(a => a.BranchId == branchId)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsTimeSlotAvailableAsync(int stylistId, DateTime startTime, DateTime endTime, int? excludeAppointmentId = null)
    {
        var query = _context.Appointments
            .Where(a => a.StylistId == stylistId && 
                       a.Status != "Cancelled" &&
                       ((a.StartTime < endTime && a.EndTime > startTime)));

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }

        return !await query.AnyAsync();
    }
}

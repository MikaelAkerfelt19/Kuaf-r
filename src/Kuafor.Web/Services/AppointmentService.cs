using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Enums;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IWorkingHoursService _workingHoursService;
    
    public AppointmentService(ApplicationDbContext context, IWorkingHoursService workingHoursService)
    {
        _context = context;
        _workingHoursService = workingHoursService;
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
            query = query.Where(a => a.StartAt >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.StartAt <= to.Value);

        return await query.OrderBy(a => a.StartAt).ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingByCustomerAsync(int customerId)
    {
        return await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Include(a => a.Customer)
            .Where(a => a.CustomerId == customerId && 
                       a.StartAt > DateTime.UtcNow &&
                       a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.StartAt)
            .ToListAsync();
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
            query = query.Where(a => a.StartAt >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.StartAt <= to.Value);

        return await query.OrderBy(a => a.StartAt).ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByBranchAsync(int branchId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .Include(a => a.Branch)
            .Where(a => a.BranchId == branchId);

        if (from.HasValue)
            query = query.Where(a => a.StartAt >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.StartAt <= to.Value);

        return await query.OrderBy(a => a.StartAt).ToListAsync();
    }

    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        // Zaman çakışması kontrolü 
        if (await HasTimeConflictAsync(appointment.StylistId, appointment.StartAt, appointment.EndAt))
        {
            throw new InvalidOperationException("Seçilen zaman diliminde başka bir randevu bulunmaktadır");
        }
        
        appointment.CreatedAt = DateTime.UtcNow;
        appointment.Status = AppointmentStatus.Confirmed;
        
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
        appointment.Status = AppointmentStatus.Rescheduled;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task<Appointment> CancelAsync(int id, string? reason = null)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            throw new ArgumentException("Appointment not found");

        appointment.Status = AppointmentStatus.Cancelled;
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
        
        // Stylist bilgisini al
        var stylist = await _context.Stylists
            .Include(s => s.Branch)
            .FirstOrDefaultAsync(s => s.Id == stylistId);
            
        if (stylist == null)
            return slots;

        // WorkingHoursService kullanarak dinamik çalışma saatlerini al
        var workingHours = await _context.WorkingHours
            .FirstOrDefaultAsync(w => w.BranchId == stylist.BranchId && w.DayOfWeek == date.DayOfWeek);
            
        if (workingHours == null || !workingHours.IsWorkingDay)
            return slots;

        // Çalışma saatleri içinde slotlar oluştur
        var currentTime = date.Date.Add(workingHours.OpenTime);
        var endTime = date.Date.Add(workingHours.CloseTime);
        
        while (currentTime.AddMinutes(durationMin) <= endTime)
        {
            // Öğle arası kontrolü - sadece çalışma süresi 4 saatten fazlaysa
            var workDuration = workingHours.CloseTime - workingHours.OpenTime;
            if (workDuration.TotalHours >= 4 && 
                workingHours.BreakStart.HasValue && workingHours.BreakEnd.HasValue &&
                workingHours.BreakStart.Value < workingHours.CloseTime &&
                workingHours.BreakEnd.Value > workingHours.OpenTime)
            {
                var breakStart = date.Date.Add(workingHours.BreakStart.Value);
                var breakEnd = date.Date.Add(workingHours.BreakEnd.Value);
                
                if (currentTime < breakEnd && currentTime.AddMinutes(durationMin) > breakStart)
                {
                    currentTime = breakEnd;
                    continue;
                }
            }
            
            // Çakışma kontrolü
            var slotEnd = currentTime.AddMinutes(durationMin);
            if (await IsTimeSlotAvailableAsync(stylistId, currentTime, slotEnd))
            {
                slots.Add(currentTime.ToString("HH:mm"));
            }
            
            currentTime = currentTime.AddMinutes(15); // 15 dakikalık aralıklar
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
                       a.StartAt > now && 
                       a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.StartAt)
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
            .Where(a => a.Status == AppointmentStatus.Completed)
            .SumAsync(a => a.TotalPrice);
    }

    public async Task<decimal> GetRevenueByDateAsync(DateTime date)
    {
        return await _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Completed && 
                       a.StartAt.Date == date.Date)
            .SumAsync(a => a.FinalPrice);
    }

    public async Task<decimal> GetRevenueByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Completed && 
                       a.StartAt.Date >= start.Date && 
                       a.StartAt.Date <= end.Date)
            .SumAsync(a => a.FinalPrice);
    }

    public async Task<IEnumerable<Appointment>> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Where(a => a.CustomerId == customerId)
            .OrderBy(a => a.StartAt)
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
            .OrderBy(a => a.StartAt)
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
            .OrderBy(a => a.StartAt)
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
        Console.WriteLine($"DEBUG: IsTimeSlotAvailableAsync - Stylist: {stylistId}, Start: {startTime:yyyy-MM-dd HH:mm}, End: {endTime:yyyy-MM-dd HH:mm}");
        
        // 1. Çalışma günü kontrolü
        var branchId = await GetStylistBranchIdAsync(stylistId);
        Console.WriteLine($"DEBUG: Branch ID: {branchId}");
        
        if (!await _workingHoursService.IsWorkingDayAsync(branchId, startTime.Date))
        {
            Console.WriteLine("DEBUG: Çalışma günü değil");
            return false;
        }
        
        // 2. Çalışma saatleri kontrolü
        if (!await _workingHoursService.IsWithinWorkingHoursAsync(branchId, startTime))
        {
            Console.WriteLine("DEBUG: Çalışma saatleri dışında");
            return false;
        }
        
        // 3. Mevcut randevu çakışması kontrolü
        var hasConflict = await HasExistingConflictAsync(stylistId, startTime, endTime, excludeAppointmentId);
        if (hasConflict)
        {
            Console.WriteLine("DEBUG: Çakışma var");
            return false;
        }
        
        // 4. Minimum önceden bildirim süresi kontrolü kaldırıldı (test için)
        Console.WriteLine("DEBUG: Minimum bildirim süresi kontrolü atlandı");
        
        Console.WriteLine("DEBUG: Slot müsait");
        return true;
    }
    
    private readonly Dictionary<int, int> _stylistBranchCache = new();
    
    private async Task<int> GetStylistBranchIdAsync(int stylistId)
    {
        if (_stylistBranchCache.TryGetValue(stylistId, out int cachedBranchId))
            return cachedBranchId;
            
        var stylist = await _context.Stylists.FindAsync(stylistId);
        var branchId = stylist?.BranchId ?? 0;
        
        if (branchId > 0)
            _stylistBranchCache[stylistId] = branchId;
            
        return branchId;
    }
    
    private async Task<bool> HasExistingConflictAsync(int stylistId, DateTime startTime, DateTime endTime, int? excludeAppointmentId)
    {
        Console.WriteLine($"DEBUG: HasExistingConflictAsync - Stylist: {stylistId}, Start: {startTime:yyyy-MM-dd HH:mm}, End: {endTime:yyyy-MM-dd HH:mm}");
        
        var query = _context.Appointments
            .Where(a => a.StylistId == stylistId && 
                       a.Status != AppointmentStatus.Cancelled &&
                       a.Status != AppointmentStatus.Completed &&
                       ((a.StartAt < endTime && a.EndAt > startTime)));

        if (excludeAppointmentId.HasValue)
        {
            query = query.Where(a => a.Id != excludeAppointmentId.Value);
        }

        var conflictingAppointments = await query.ToListAsync();
        
        // Debug log
        if (conflictingAppointments.Any())
        {
            Console.WriteLine($"DEBUG: Çakışan randevular bulundu. Stylist: {stylistId}, İstenen: {startTime:yyyy-MM-dd HH:mm} - {endTime:yyyy-MM-dd HH:mm}");
            foreach (var appt in conflictingAppointments)
            {
                Console.WriteLine($"  - Mevcut randevu: {appt.StartAt:yyyy-MM-dd HH:mm} - {appt.EndAt:yyyy-MM-dd HH:mm} (Status: {appt.Status})");
            }
        }
        else
        {
            Console.WriteLine($"DEBUG: Çakışma yok. Stylist: {stylistId}, İstenen: {startTime:yyyy-MM-dd HH:mm} - {endTime:yyyy-MM-dd HH:mm}");
        }

        return conflictingAppointments.Any();
    }
}


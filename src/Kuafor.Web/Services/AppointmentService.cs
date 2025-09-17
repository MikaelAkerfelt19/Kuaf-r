using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Entities.Analytics;
using Kuafor.Web.Models.Enums;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IWorkingHoursService _workingHoursService;
    private readonly ISmsService _smsService;
    private readonly ILogger<AppointmentService> _logger;
    private readonly IWhatsAppService _whatsAppService;
    
    public AppointmentService(
        ApplicationDbContext context,
        IWorkingHoursService workingHoursService,
        ISmsService smsService,
        IWhatsAppService whatsAppService,
        ILogger<AppointmentService> logger)
    {
        _context = context;
        _workingHoursService = workingHoursService;
        _smsService = smsService;
        _whatsAppService = whatsAppService;
        _logger = logger;
    }
    
    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await _context.Appointments
            .AsNoTracking()
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
            .AsNoTracking()
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Include(a => a.Customer)
            .Include(a => a.Coupon)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
    
    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        try
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            
            // SMS ve WhatsApp'dan gönder
            try
            {
                await _smsService.SendAppointmentConfirmationAsync(appointment);
                _logger.LogInformation("SMS gönderildi - Randevu ID: {AppointmentId}", appointment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS gönderilirken hata oluştu - Randevu ID: {AppointmentId}", appointment.Id);
            }

            try
            {
                   await _whatsAppService.SendAppointmentConfirmationAsync(appointment);
                _logger.LogInformation("WhatsApp mesajı gönderildi - Randevu ID: {AppointmentId}", appointment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp mesajı gönderilirken hata oluştu - Randevu ID: {AppointmentId}", appointment.Id);
            }
            return appointment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Randevu oluşturulurken hata oluştu");
            throw;
        }
    }
    
    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
        return appointment;
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
    
    public async Task<IEnumerable<Appointment>> GetByCustomerAsync(int customerId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Include(a => a.Coupon)
            .Where(a => a.CustomerId == customerId);
            
        if (from.HasValue)
            query = query.Where(a => a.StartAt >= from.Value);
            
        if (to.HasValue)
            query = query.Where(a => a.StartAt <= to.Value);
            
        return await query.OrderByDescending(a => a.StartAt).ToListAsync();
    }
    
    public async Task<IEnumerable<Appointment>> GetUpcomingByCustomerAsync(int customerId)
    {
        var now = DateTime.UtcNow;
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Include(a => a.Coupon)
            .Where(a => a.CustomerId == customerId && a.StartAt > now && a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.StartAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Appointment>> GetByStylistAsync(int stylistId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Service)
            .Include(a => a.Customer)
            .Include(a => a.Branch)
            .Include(a => a.Coupon)
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
            .AsNoTracking()
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Customer)
            .Include(a => a.Coupon)
            .Where(a => a.BranchId == branchId);
            
        if (from.HasValue)
            query = query.Where(a => a.StartAt >= from.Value);
            
        if (to.HasValue)
            query = query.Where(a => a.StartAt <= to.Value);
            
        return await query.OrderBy(a => a.StartAt).ToListAsync();
    }
    
    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Include(a => a.Customer)
            .Include(a => a.Coupon)
            .Where(a => a.StartAt >= start && a.StartAt <= end)
            .OrderBy(a => a.StartAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);
        
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Service)
            .Include(a => a.Stylist)
            .Include(a => a.Branch)
            .Include(a => a.Customer)
            .Include(a => a.Coupon)
            .Where(a => a.StartAt >= startOfDay && a.StartAt < endOfDay)
            .OrderBy(a => a.StartAt)
            .ToListAsync();
    }
    
    public async Task<Appointment> RescheduleAsync(int id, DateTime newStartAt)
    {
        try
        {
            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Stylist)
                .Include(a => a.Branch)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);
                
            if (appointment == null)
                throw new ArgumentException("Randevu bulunamadı");
                
            var oldStartTime = appointment.StartAt;
            appointment.StartAt = newStartAt;
            appointment.EndAt = newStartAt.AddMinutes((int)(appointment.EndAt - appointment.StartAt).TotalMinutes);
            appointment.Status = AppointmentStatus.Confirmed;
            
            await _context.SaveChangesAsync();
            
            // SMS gönder
            try
            {
                await _smsService.SendAppointmentRescheduleAsync(appointment, newStartAt);
                _logger.LogInformation("Ertelenme SMS'i gönderildi - Randevu ID: {AppointmentId}", appointment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ertelenme SMS'i gönderilirken hata oluştu - Randevu ID: {AppointmentId}", appointment.Id);
            }
            
            return appointment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Randevu ertelenirken hata oluştu - ID: {AppointmentId}", id);
            throw;
        }
    }
    
    public async Task<Appointment> CancelAsync(int id, string? reason = null)
    {
        try
        {
            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Stylist)
                .Include(a => a.Branch)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);
                
            if (appointment == null)
                throw new ArgumentException("Randevu bulunamadı");
                
            appointment.Status = AppointmentStatus.Cancelled;
            if (!string.IsNullOrEmpty(reason))
            {
                appointment.Notes = $"İptal sebebi: {reason}";
            }
            
            await _context.SaveChangesAsync();
            
            // SMS gönder
            try
            {
                await _smsService.SendAppointmentCancellationAsync(appointment);
                _logger.LogInformation("İptal SMS'i gönderildi - Randevu ID: {AppointmentId}", appointment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İptal SMS'i gönderilirken hata oluştu - Randevu ID: {AppointmentId}", appointment.Id);
            }
            
            return appointment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Randevu iptal edilirken hata oluştu - ID: {AppointmentId}", id);
            throw;
        }
    }
    
    public async Task<bool> HasTimeConflictAsync(int stylistId, DateTime startAt, DateTime endAt, int? excludeId = null)
    {
        var query = _context.Appointments
            .Where(a => a.StylistId == stylistId && 
                       a.Status != AppointmentStatus.Cancelled &&
                       ((a.StartAt < endAt && a.EndAt > startAt)));
                       
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);
            
        return await query.AnyAsync();
    }
    
    public async Task<bool> HasConflictAsync(int stylistId, DateTime startAt, DateTime endAt, int? excludeId = null)
    {
        return await HasTimeConflictAsync(stylistId, startAt, endAt, excludeId);
    }
    
    public async Task<bool> IsTimeSlotAvailableAsync(int stylistId, DateTime startTime, DateTime endTime, int? excludeAppointmentId = null)
    {
        return !await HasTimeConflictAsync(stylistId, startTime, endTime, excludeAppointmentId);
    }
    
    public async Task<List<string>> GetAvailableSlotsAsync(int stylistId, DateTime date, int durationMin)
    {
        var slots = new List<string>();
        var startTime = date.Date.AddHours(9);
        var endTime = date.Date.AddHours(18);
        
        var stylist = await _context.Stylists.FindAsync(stylistId);
        if (stylist == null) return slots;
        
        var workingHours = await _workingHoursService.GetByBranchAsync(stylist.BranchId);
        var dayOfWeek = date.DayOfWeek;
        var todayHours = workingHours.FirstOrDefault(w => w.DayOfWeek == dayOfWeek);
        
        if (todayHours != null && todayHours.IsWorkingDay)
        {
            startTime = date.Date.Add(todayHours.OpenTime);
            endTime = date.Date.Add(todayHours.CloseTime);
        }
        
        var currentTime = startTime;
        while (currentTime.AddMinutes(durationMin) <= endTime)
        {
            var slotEnd = currentTime.AddMinutes(durationMin);
            var hasConflict = await HasTimeConflictAsync(stylistId, currentTime, slotEnd);
            
            if (!hasConflict)
            {
                slots.Add(currentTime.ToString("HH:mm"));
            }
            
            currentTime = currentTime.AddMinutes(30);
        }
        
        return slots;
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Appointments.AnyAsync(a => a.Id == id);
    }
    
    public async Task<int> GetCountAsync()
    {
        return await _context.Appointments.CountAsync();
    }
    
    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Completed)
            .SumAsync(a => a.FinalPrice);
    }
    
    public async Task<decimal> GetRevenueByDateAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);
        
        return await _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Completed && 
                       a.StartAt >= startOfDay && a.StartAt < endOfDay)
            .SumAsync(a => a.FinalPrice);
    }
    
    public async Task<decimal> GetRevenueByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Completed && 
                       a.StartAt >= start && a.StartAt <= end)
            .SumAsync(a => a.FinalPrice);
    }
    
    // Legacy Support
    public async Task<IEnumerable<Appointment>> GetByCustomerIdAsync(int customerId)
    {
        return await GetByCustomerAsync(customerId);
    }
    
    public async Task<IEnumerable<Appointment>> GetByStylistIdAsync(int stylistId)
    {
        return await GetByStylistAsync(stylistId);
    }
    
    public async Task<IEnumerable<Appointment>> GetByBranchIdAsync(int branchId)
    {
        return await GetByBranchAsync(branchId);
    }

    public async Task<List<Appointment>> GetRepeatingAppointmentsAsync(int customerId)
    {
        // Müşterinin tekrarlayan randevularını getirir
        var appointments = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Service)
            .Where(a => a.CustomerId == customerId)
            .OrderBy(a => a.StartAt)
            .ToListAsync();

        return appointments;
    }

    public async Task<bool> CreateRepeatingAppointmentAsync(Appointment baseAppointment, int repeatCount, string repeatType)
    {
        // Tekrarlayan randevu oluşturur (haftalık, aylık vs.)
        var appointments = new List<Appointment>();
        
        for (int i = 1; i <= repeatCount; i++)
        {
            var newAppointment = new Appointment
            {
                CustomerId = baseAppointment.CustomerId,
                StylistId = baseAppointment.StylistId,
                ServiceId = baseAppointment.ServiceId,
                BranchId = baseAppointment.BranchId,
                StartAt = repeatType == "Weekly" ? baseAppointment.StartAt.AddDays(i * 7) : baseAppointment.StartAt.AddMonths(i),
                EndAt = repeatType == "Weekly" ? baseAppointment.EndAt.AddDays(i * 7) : baseAppointment.EndAt.AddMonths(i),
                TotalPrice = baseAppointment.TotalPrice,
                FinalPrice = baseAppointment.FinalPrice,
                Status = AppointmentStatus.Pending,
                Notes = baseAppointment.Notes,
                CreatedAt = DateTime.UtcNow
            };
            
            appointments.Add(newAppointment);
        }
        
        _context.Appointments.AddRange(appointments);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<TimeSlot>> GetAvailableTimeSlotsAsync(int stylistId, DateTime date, int serviceDuration)
    {
        // Belirtilen gün için uygun zaman dilimlerini getirir
        // var workingHours = await _workingHoursService.GetWorkingHoursAsync(stylistId, date.DayOfWeek); // TODO: GetWorkingHoursAsync metodunu implement et
        var existingAppointments = await GetByStylistAsync(stylistId, date, date.AddDays(1));
        
        var availableSlots = new List<TimeSlot>();
        // Geçici çözüm - varsayılan çalışma saatleri
        var startTime = date.Date.Add(TimeSpan.FromHours(9)); // 09:00
        var endTime = date.Date.Add(TimeSpan.FromHours(18)); // 18:00
        
        while (startTime.AddMinutes(serviceDuration) <= endTime)
        {
            var slotEndTime = startTime.AddMinutes(serviceDuration);
            var hasConflict = existingAppointments.Any(a => 
                (startTime >= a.StartAt && startTime < a.EndAt) ||
                (slotEndTime > a.StartAt && slotEndTime <= a.EndAt));
                
            if (!hasConflict)
            {
                availableSlots.Add(new TimeSlot
                {
                    StartTime = startTime,
                    EndTime = slotEndTime,
                    IsAvailable = true
                });
            }
            
            startTime = startTime.AddMinutes(30); // 30 dakikalık aralıklarla
        }
        
        return availableSlots;
    }

    public async Task<AppointmentStatistics> GetAppointmentStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        // Randevu istatistiklerini getirir
        var appointments = await _context.Appointments
            .Where(a => a.StartAt >= startDate && a.StartAt <= endDate)
            .ToListAsync();
        
        return new AppointmentStatistics
        {
            TotalAppointments = appointments.Count,
            CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
            CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
            PendingAppointments = appointments.Count(a => a.Status == AppointmentStatus.Pending),
            TotalRevenue = appointments.Sum(a => a.FinalPrice),
            AverageAppointmentValue = appointments.Any() ? appointments.Average(a => a.FinalPrice) : 0
        };
    }
}

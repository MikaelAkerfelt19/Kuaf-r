using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class StylistWorkingHoursService : IStylistWorkingHoursService
{
    private readonly ApplicationDbContext _context;

    public StylistWorkingHoursService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StylistWorkingHours>> GetByStylistAsync(int stylistId)
    {
        return await _context.StylistWorkingHours
            .Where(wh => wh.StylistId == stylistId)
            .OrderBy(wh => wh.DayOfWeek)
            .ToListAsync();
    }

    public async Task<StylistWorkingHours?> GetByStylistAndDayAsync(int stylistId, DayOfWeek dayOfWeek)
    {
        return await _context.StylistWorkingHours
            .FirstOrDefaultAsync(wh => wh.StylistId == stylistId && wh.DayOfWeek == dayOfWeek);
    }

    public async Task<bool> IsWorkingDayAsync(int stylistId, DateTime date)
    {
        var workingHours = await GetByStylistAndDayAsync(stylistId, date.DayOfWeek);
        return workingHours?.IsWorkingDay ?? false;
    }

    public async Task<bool> IsWithinWorkingHoursAsync(int stylistId, DateTime dateTime)
    {
        var workingHours = await GetByStylistAndDayAsync(stylistId, dateTime.DayOfWeek);
        if (workingHours == null || !workingHours.IsWorkingDay)
            return false;

        var timeOfDay = dateTime.TimeOfDay;
        if (timeOfDay < workingHours.OpenTime || timeOfDay >= workingHours.CloseTime)
            return false;

        // Öğle arası kontrolü
        if (workingHours.BreakStart.HasValue && workingHours.BreakEnd.HasValue)
        {
            if (timeOfDay >= workingHours.BreakStart.Value && timeOfDay < workingHours.BreakEnd.Value)
                return false;
        }

        return true;
    }

    public async Task<IEnumerable<TimeSpan>> GetAvailableTimeSlotsAsync(int stylistId, DateTime date, int durationMinutes)
    {
        var workingHours = await GetByStylistAndDayAsync(stylistId, date.DayOfWeek);
        if (workingHours == null || !workingHours.IsWorkingDay)
            return Enumerable.Empty<TimeSpan>();

        var slots = new List<TimeSpan>();
        var currentTime = workingHours.OpenTime;
        var endTime = workingHours.CloseTime;

        while (currentTime.Add(TimeSpan.FromMinutes(durationMinutes)) <= endTime)
        {
            // Öğle arası kontrolü
            if (workingHours.BreakStart.HasValue && workingHours.BreakEnd.HasValue)
            {
                var slotEnd = currentTime.Add(TimeSpan.FromMinutes(durationMinutes));
                if (currentTime < workingHours.BreakEnd.Value && slotEnd > workingHours.BreakStart.Value)
                {
                    currentTime = workingHours.BreakEnd.Value;
                    continue;
                }
            }

            slots.Add(currentTime);
            currentTime = currentTime.Add(TimeSpan.FromMinutes(15)); // 15 dakika aralıklarla
        }

        return slots;
    }

    public async Task<StylistWorkingHours?> GetByIdAsync(int id)
    {
        return await _context.StylistWorkingHours.FindAsync(id);
    }

    public async Task<StylistWorkingHours> CreateAsync(StylistWorkingHours workingHours)
    {
        _context.StylistWorkingHours.Add(workingHours);
        await _context.SaveChangesAsync();
        return workingHours;
    }

    public async Task<StylistWorkingHours> UpdateAsync(StylistWorkingHours workingHours)
    {
        workingHours.UpdatedAt = DateTime.UtcNow;
        _context.StylistWorkingHours.Update(workingHours);
        await _context.SaveChangesAsync();
        return workingHours;
    }

    public async Task DeleteAsync(int id)
    {
        var workingHours = await _context.StylistWorkingHours.FindAsync(id);
        if (workingHours != null)
        {
            _context.StylistWorkingHours.Remove(workingHours);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SetStylistWorkingHoursAsync(int stylistId, List<StylistWorkingHours> workingHoursList)
    {
        // Mevcut çalışma saatlerini sil
        var existingHours = await _context.StylistWorkingHours
            .Where(wh => wh.StylistId == stylistId)
            .ToListAsync();
        
        _context.StylistWorkingHours.RemoveRange(existingHours);

        // Yeni çalışma saatlerini ekle
        foreach (var workingHour in workingHoursList)
        {
            workingHour.StylistId = stylistId;
            workingHour.CreatedAt = DateTime.UtcNow;
        }

        _context.StylistWorkingHours.AddRange(workingHoursList);
        await _context.SaveChangesAsync();
    }

    public async Task InitializeDefaultWorkingHoursAsync(int stylistId)
    {
        var defaultHours = new List<StylistWorkingHours>();
        
        for (int i = 0; i < 7; i++)
        {
            var dayOfWeek = (DayOfWeek)i;
            var isWorkingDay = dayOfWeek != DayOfWeek.Sunday; // Pazar günü kapalı
            
            defaultHours.Add(new StylistWorkingHours
            {
                StylistId = stylistId,
                DayOfWeek = dayOfWeek,
                IsWorkingDay = isWorkingDay,
                OpenTime = isWorkingDay ? TimeSpan.FromHours(9) : TimeSpan.Zero,
                CloseTime = isWorkingDay ? TimeSpan.FromHours(18) : TimeSpan.Zero,
                BreakStart = isWorkingDay ? TimeSpan.FromHours(12) : null,
                BreakEnd = isWorkingDay ? TimeSpan.FromHours(13) : null,
                CreatedAt = DateTime.UtcNow
            });
        }

        await SetStylistWorkingHoursAsync(stylistId, defaultHours);
    }
}

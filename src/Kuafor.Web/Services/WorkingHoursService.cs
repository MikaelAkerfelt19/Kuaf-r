using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class WorkingHoursService : IWorkingHoursService
{
    private readonly ApplicationDbContext _context;
    
    public WorkingHoursService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<WorkingHours>> GetByBranchAsync(int branchId)
    {
        return await _context.WorkingHours
            .Where(w => w.BranchId == branchId)
            .OrderBy(w => w.DayOfWeek)
            .ToListAsync();
    }
    
    public async Task<WorkingHours?> GetByBranchAndDayAsync(int branchId, DayOfWeek dayOfWeek)
    {
        return await _context.WorkingHours
            .FirstOrDefaultAsync(w => w.BranchId == branchId && w.DayOfWeek == dayOfWeek);
    }
    
    public async Task<bool> IsWorkingDayAsync(int branchId, DateTime date)
    {
        var dayOfWeek = date.DayOfWeek;
        var workingHours = await GetByBranchAndDayAsync(branchId, dayOfWeek);
        
        return workingHours?.IsWorkingDay ?? false;
    }
    
    public async Task<bool> IsWithinWorkingHoursAsync(int branchId, DateTime dateTime)
    {
        Console.WriteLine($"DEBUG: IsWithinWorkingHoursAsync - Branch: {branchId}, DateTime: {dateTime:yyyy-MM-dd HH:mm}");
        
        var workingHours = await GetByBranchAndDayAsync(branchId, dateTime.DayOfWeek);
        if (workingHours == null || !workingHours.IsWorkingDay)
        {
            Console.WriteLine("DEBUG: Çalışma günü değil veya working hours bulunamadı");
            return false;
        }
        
        var timeOfDay = dateTime.TimeOfDay;
        Console.WriteLine($"DEBUG: TimeOfDay: {timeOfDay}, OpenTime: {workingHours.OpenTime}, CloseTime: {workingHours.CloseTime}");
        
        // Çalışma saatleri kontrolü
        if (timeOfDay < workingHours.OpenTime || timeOfDay > workingHours.CloseTime)
        {
            Console.WriteLine("DEBUG: Çalışma saatleri dışında");
            return false;
        }
        
        // Öğle arası kontrolü
        if (workingHours.BreakStart.HasValue && workingHours.BreakEnd.HasValue)
        {
            Console.WriteLine($"DEBUG: BreakStart: {workingHours.BreakStart.Value}, BreakEnd: {workingHours.BreakEnd.Value}");
            if (timeOfDay >= workingHours.BreakStart.Value && timeOfDay < workingHours.BreakEnd.Value)
            {
                Console.WriteLine("DEBUG: Öğle arası saatleri içinde");
                return false;
            }
        }
        
        Console.WriteLine("DEBUG: Çalışma saatleri içinde");
        return true;
    }
    
    public async Task<IEnumerable<TimeSpan>> GetAvailableTimeSlotsAsync(int branchId, DateTime date, int durationMinutes)
    {
        var workingHours = await GetByBranchAndDayAsync(branchId, date.DayOfWeek);
        if (workingHours == null || !workingHours.IsWorkingDay)
            return Enumerable.Empty<TimeSpan>();
        
        var slots = new List<TimeSpan>();
        var currentTime = workingHours.OpenTime;
        var endTime = workingHours.CloseTime;
        
        while (currentTime.Add(TimeSpan.FromMinutes(durationMinutes)) <= endTime)
        {
            // Öğle arası kontrolü - sadece çalışma süresi 4 saatten fazlaysa
            var workDuration = workingHours.CloseTime - workingHours.OpenTime;
            if (workDuration.TotalHours >= 4 && 
                workingHours.BreakStart.HasValue && workingHours.BreakEnd.HasValue &&
                workingHours.BreakStart.Value < workingHours.CloseTime &&
                workingHours.BreakEnd.Value > workingHours.OpenTime)
            {
                var slotEnd = currentTime.Add(TimeSpan.FromMinutes(durationMinutes));
                if (currentTime < workingHours.BreakEnd.Value && slotEnd > workingHours.BreakStart.Value)
                {
                    currentTime = workingHours.BreakEnd.Value;
                    continue;
                }
            }
            
            slots.Add(currentTime);
            var slotIntervalMinutes = 15; // Daha detaylı slot aralığı
            currentTime = currentTime.Add(TimeSpan.FromMinutes(slotIntervalMinutes));
        }
        
        return slots;
    }

    public async Task<Stylist?> GetStylistBranchAsync(int stylistId)
    {
        return await _context.Stylists
            .Include(s => s.Branch)
            .FirstOrDefaultAsync(s => s.Id == stylistId);
    }

    // CRUD Operations
    public async Task<WorkingHours?> GetByIdAsync(int id)
    {
        return await _context.WorkingHours.FindAsync(id);
    }

    public async Task<WorkingHours> CreateAsync(WorkingHours workingHours)
    {
        workingHours.CreatedAt = DateTime.UtcNow;
        _context.WorkingHours.Add(workingHours);
        await _context.SaveChangesAsync();
        return workingHours;
    }

    public async Task<WorkingHours> UpdateAsync(WorkingHours workingHours)
    {
        workingHours.UpdatedAt = DateTime.UtcNow;
        _context.WorkingHours.Update(workingHours);
        await _context.SaveChangesAsync();
        return workingHours;
    }

    public async Task DeleteAsync(int id)
    {
        var workingHours = await _context.WorkingHours.FindAsync(id);
        if (workingHours != null)
        {
            _context.WorkingHours.Remove(workingHours);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SetBranchWorkingHoursAsync(int branchId, List<WorkingHours> workingHoursList)
    {
        // Mevcut çalışma saatlerini sil
        var existingHours = await _context.WorkingHours
            .Where(w => w.BranchId == branchId)
            .ToListAsync();
        
        _context.WorkingHours.RemoveRange(existingHours);
        
        // Yeni çalışma saatlerini ekle
        foreach (var workingHours in workingHoursList)
        {
            workingHours.BranchId = branchId;
            workingHours.CreatedAt = DateTime.UtcNow;
            _context.WorkingHours.Add(workingHours);
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task InitializeDefaultWorkingHoursAsync(int branchId)
    {
        var defaultHours = new List<WorkingHours>();
        
        // Pazartesi - Cumartesi: 09:00 - 18:00, Öğle arası: 12:00 - 13:00
        for (int i = 1; i <= 6; i++)
        {
            defaultHours.Add(new WorkingHours
            {
                BranchId = branchId,
                DayOfWeek = (DayOfWeek)i,
                OpenTime = TimeSpan.FromHours(9),
                CloseTime = TimeSpan.FromHours(18),
                BreakStart = TimeSpan.FromHours(12),
                BreakEnd = TimeSpan.FromHours(13),
                IsWorkingDay = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        
        // Pazar: Kapalı
        defaultHours.Add(new WorkingHours
        {
            BranchId = branchId,
            DayOfWeek = DayOfWeek.Sunday,
            OpenTime = TimeSpan.FromHours(9),
            CloseTime = TimeSpan.FromHours(18),
            IsWorkingDay = false,
            CreatedAt = DateTime.UtcNow
        });
        
        await SetBranchWorkingHoursAsync(branchId, defaultHours);
    }
}

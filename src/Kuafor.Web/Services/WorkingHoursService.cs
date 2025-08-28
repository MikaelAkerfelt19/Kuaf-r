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
        var workingHours = await GetByBranchAndDayAsync(branchId, dateTime.DayOfWeek);
        if (workingHours == null || !workingHours.IsWorkingDay)
            return false;
        
        var timeOfDay = dateTime.TimeOfDay;
        
        // Çalışma saatleri kontrolü
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
            currentTime = currentTime.Add(TimeSpan.FromMinutes(30)); // 30 dakikalık aralıklar
        }
        
        return slots;
    }
}

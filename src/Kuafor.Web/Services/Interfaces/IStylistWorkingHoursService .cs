using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IStylistWorkingHoursService
{
    Task<IEnumerable<StylistWorkingHours>> GetByStylistAsync(int stylistId);
    Task<StylistWorkingHours?> GetByStylistAndDayAsync(int stylistId, DayOfWeek dayOfWeek);
    Task<bool> IsWorkingDayAsync(int stylistId, DateTime date);
    Task<bool> IsWithinWorkingHoursAsync(int stylistId, DateTime dateTime);
    Task<IEnumerable<TimeSpan>> GetAvailableTimeSlotsAsync(int stylistId, DateTime date, int durationMinutes);
    
    // CRUD Operations
    Task<StylistWorkingHours> CreateAsync(StylistWorkingHours workingHours);
    Task<StylistWorkingHours> UpdateAsync(StylistWorkingHours workingHours);
    Task DeleteAsync(int id);
    Task<StylistWorkingHours?> GetByIdAsync(int id);
    
    // Batch Operations
    Task SetStylistWorkingHoursAsync(int stylistId, List<StylistWorkingHours> workingHoursList);
    Task InitializeDefaultWorkingHoursAsync(int stylistId);
}
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IWorkingHoursService
{
    // Query Operations
    Task<IEnumerable<WorkingHours>> GetByBranchAsync(int branchId);
    Task<WorkingHours?> GetByBranchAndDayAsync(int branchId, DayOfWeek dayOfWeek);
    Task<bool> IsWorkingDayAsync(int branchId, DateTime date);
    Task<bool> IsWithinWorkingHoursAsync(int branchId, DateTime dateTime);
    Task<IEnumerable<TimeSpan>> GetAvailableTimeSlotsAsync(int branchId, DateTime date, int durationMinutes);
    Task<Stylist?> GetStylistBranchAsync(int stylistId);
    
    // CRUD Operations
    Task<WorkingHours> CreateAsync(WorkingHours workingHours);
    Task<WorkingHours> UpdateAsync(WorkingHours workingHours);
    Task DeleteAsync(int id);
    Task<WorkingHours?> GetByIdAsync(int id);
    
    // Batch Operations for Branch
    Task SetBranchWorkingHoursAsync(int branchId, List<WorkingHours> workingHoursList);
    Task InitializeDefaultWorkingHoursAsync(int branchId);
}
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IWorkingHoursService
{
    Task<IEnumerable<WorkingHours>> GetByBranchAsync(int branchId);
    Task<WorkingHours?> GetByBranchAndDayAsync(int branchId, DayOfWeek dayOfWeek);
    Task<bool> IsWorkingDayAsync(int branchId, DateTime date);
    Task<bool> IsWithinWorkingHoursAsync(int branchId, DateTime dateTime);
    Task<IEnumerable<TimeSpan>> GetAvailableTimeSlotsAsync(int branchId, DateTime date, int durationMinutes);
}
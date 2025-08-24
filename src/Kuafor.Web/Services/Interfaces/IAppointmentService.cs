using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<Appointment?> GetByIdAsync(int id);
    Task<IEnumerable<Appointment>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<Appointment>> GetByStylistIdAsync(int stylistId);
    Task<IEnumerable<Appointment>> GetByBranchIdAsync(int branchId);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date);
    Task<Appointment> CreateAsync(Appointment appointment);
    Task<Appointment> UpdateAsync(Appointment appointment);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> IsTimeSlotAvailableAsync(int stylistId, DateTime startTime, DateTime endTime, int? excludeAppointmentId = null);
    
    // Yeni eklenen method'lar
    Task<int> GetCountAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetRevenueByDateAsync(DateTime date);
    Task<decimal> GetRevenueByDateRangeAsync(DateTime start, DateTime end);
    
    // Eksik method'lar
    Task<IEnumerable<Appointment>> GetByCustomerAsync(int customerId, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<Appointment>> GetByStylistAsync(int stylistId, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<Appointment>> GetByBranchAsync(int branchId, DateTime? from = null, DateTime? to = null);
    Task<Appointment> RescheduleAsync(int id, DateTime newStartAt);
    Task<Appointment> CancelAsync(int id, string? reason = null);
    Task<bool> HasTimeConflictAsync(int stylistId, DateTime startAt, DateTime endAt, int? excludeId = null);
    Task<bool> HasConflictAsync(int stylistId, DateTime startAt, DateTime endAt, int? excludeId = null);
    Task<List<string>> GetAvailableSlotsAsync(int stylistId, DateTime date, int durationMin);
    Task<IEnumerable<Appointment>> GetUpcomingAsync(int customerId);
}

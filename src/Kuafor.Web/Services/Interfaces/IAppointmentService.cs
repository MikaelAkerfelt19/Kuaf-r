using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Entities.Analytics;
using Kuafor.Web.Models.Enums;

namespace Kuafor.Web.Services.Interfaces;

public interface IAppointmentService
{
    // CRUD Operations
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<Appointment?> GetByIdAsync(int id);
    Task<Appointment> CreateAsync(Appointment appointment);
    Task<Appointment> UpdateAsync(Appointment appointment);
    Task DeleteAsync(int id);
    
    // Query Operations
    Task<IEnumerable<Appointment>> GetByCustomerAsync(int customerId, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<Appointment>> GetUpcomingByCustomerAsync(int customerId);
    Task<IEnumerable<Appointment>> GetByStylistAsync(int stylistId, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<Appointment>> GetByBranchAsync(int branchId, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date);
    
    // Business Logic
    Task<Appointment> RescheduleAsync(int id, DateTime newStartAt);
    Task<Appointment> CancelAsync(int id, string? reason = null);
    Task<bool> HasTimeConflictAsync(int stylistId, DateTime startAt, DateTime endAt, int? excludeId = null);
    Task<bool> HasConflictAsync(int stylistId, DateTime startAt, DateTime endAt, int? excludeId = null);
    Task<bool> IsTimeSlotAvailableAsync(int stylistId, DateTime startTime, DateTime endTime, int? excludeAppointmentId = null);
    Task<List<string>> GetAvailableSlotsAsync(int stylistId, DateTime date, int durationMin);
    
    // Utility Methods
    Task<bool> ExistsAsync(int id);
    Task<int> GetCountAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetRevenueByDateAsync(DateTime date);
    Task<decimal> GetRevenueByDateRangeAsync(DateTime start, DateTime end);
    
    // Legacy Support (for backward compatibility)
    Task<IEnumerable<Appointment>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<Appointment>> GetByStylistIdAsync(int stylistId);
    Task<IEnumerable<Appointment>> GetByBranchIdAsync(int branchId);
    
    
    // Tekrarlayan randevular
    Task<List<Appointment>> GetRepeatingAppointmentsAsync(int customerId);
    Task<bool> CreateRepeatingAppointmentAsync(Appointment baseAppointment, int repeatCount, string repeatType);
    
    // Zaman dilimi yönetimi
    Task<List<TimeSlot>> GetAvailableTimeSlotsAsync(int stylistId, DateTime date, int serviceDuration);
    
    // İstatistikler ve analiz
    Task<AppointmentStatistics> GetAppointmentStatisticsAsync(DateTime startDate, DateTime endDate);
}

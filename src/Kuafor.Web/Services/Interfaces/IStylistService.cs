using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IStylistService
{
    // CRUD Operations
    Task<IEnumerable<Stylist>> GetAllAsync();
    Task<Stylist?> GetByIdAsync(int id);
    Task<Stylist> CreateAsync(Stylist stylist);
    Task<Stylist> UpdateAsync(Stylist stylist);
    Task DeleteAsync(int id);
    
    // Query Operations
    Task<IEnumerable<Stylist>> GetActiveAsync();
    Task<IEnumerable<Stylist>> GetByBranchAsync(int branchId);
    Task<IEnumerable<Stylist>> GetByServiceAsync(int serviceId);
    Task<Stylist?> GetByUserIdAsync(string userId);
    Task<IEnumerable<Stylist>> GetTopRatedAsync(int count = 5);
    Task<IEnumerable<Stylist>> GetTopStylistsByWeekAsync(int count = 5);
    Task<IEnumerable<Stylist>> GetTopStylistsByMonthAsync(int count = 5);
    
    // Business Logic
    Task<bool> IsActiveAsync(int id);
    Task<bool> IsAvailableAsync(int stylistId, DateTime dateTime, int durationMinutes);
    Task<int> GetAppointmentCountAsync(int stylistId, DateTime? from = null, DateTime? to = null);
    Task<decimal> GetRevenueAsync(int stylistId, DateTime? from = null, DateTime? to = null);
    Task<double> GetAverageRatingAsync(int stylistId);
    
    // Utility Methods
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByUserIdAsync(string userId);
    Task<int> GetCountAsync();
    Task<int> GetCountByBranchAsync(int branchId);
}

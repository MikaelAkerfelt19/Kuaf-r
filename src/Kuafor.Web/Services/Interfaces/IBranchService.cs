using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IBranchService
{
    // CRUD Operations
    Task<IEnumerable<Branch>> GetAllAsync();
    Task<Branch?> GetByIdAsync(int id);
    Task<Branch> CreateAsync(Branch branch);
    Task<Branch> UpdateAsync(Branch branch);
    Task DeleteAsync(int id);
    
    // Query Operations
    Task<IEnumerable<Branch>> GetActiveAsync();
    Task<IEnumerable<Branch>> GetForHomePageAsync();
    Task<Branch?> GetByNameAsync(string name);
    Task<IEnumerable<Branch>> GetByCityAsync(string city);
    
    // Business Logic
    Task<bool> IsActiveAsync(int id);
    Task<int> GetStylistCountAsync(int branchId);
    Task<int> GetAppointmentCountAsync(int branchId, DateTime? date = null);
    Task<decimal> GetRevenueAsync(int branchId, DateTime? from = null, DateTime? to = null);
    Task<object> GetPerformanceAsync(int branchId, DateTime? from = null, DateTime? to = null);
    
    // Utility Methods
    Task<bool> ExistsAsync(int id);
    Task<int> GetCountAsync();
    Task<IEnumerable<string>> GetCitiesAsync();
}

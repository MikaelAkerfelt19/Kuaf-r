using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface ICustomerService
{
    // CRUD Operations
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(int id);
    Task<Customer> CreateAsync(Customer customer);
    Task<Customer> UpdateAsync(Customer customer);
    Task DeleteAsync(int id);
    
    // Query Operations
    Task<Customer?> GetByUserIdAsync(string userId);
    Task<Customer?> GetByEmailAsync(string email);
    Task<Customer?> GetByPhoneAsync(string phone);
    Task<IEnumerable<Customer>> GetActiveAsync();
    Task<IEnumerable<Customer>> GetByBranchAsync(int branchId);
    
    // Business Logic
    Task<bool> IsActiveAsync(int id);
    Task<int> GetAppointmentCountAsync(int customerId);
    Task<decimal> GetTotalSpentAsync(int customerId);
    Task<DateTime?> GetLastVisitAsync(int customerId);
    Task<bool> HasUpcomingAppointmentsAsync(int customerId);
    
    // Utility Methods
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByUserIdAsync(string userId);
    Task<bool> ExistsByEmailAsync(string email);
    Task<int> GetCountAsync();
}

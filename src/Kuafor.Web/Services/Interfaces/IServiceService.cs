using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IServiceService
{
    // CRUD Operations
    Task<IEnumerable<Service>> GetAllAsync();
    Task<Service?> GetByIdAsync(int id);
    Task<Service> CreateAsync(Service service);
    Task<Service> UpdateAsync(Service service);
    Task DeleteAsync(int id);
    
    // Query Operations
    Task<IEnumerable<Service>> GetActiveAsync();
    Task<IEnumerable<Service>> GetForHomePageAsync();
    Task<IEnumerable<Service>> GetByCategoryAsync(string category);
    Task<IEnumerable<Service>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<IEnumerable<Service>> SearchAsync(string query);
    Task<IEnumerable<Service>> SearchAsync(string query, int? categoryId, decimal? minPrice, decimal? maxPrice);
    Task<IEnumerable<Service>> GetRelatedAsync(int serviceId);
    
    // Business Logic
    Task ToggleHomePageVisibilityAsync(int serviceId);
    Task ToggleActiveAsync(int serviceId);
    Task UpdateDisplayOrderAsync(int serviceId, int displayOrder);
    Task<bool> IsActiveAsync(int id);
    Task<int> GetAppointmentCountAsync(int serviceId, DateTime? from = null, DateTime? to = null);
    Task<decimal> GetRevenueAsync(int serviceId, DateTime? from = null, DateTime? to = null);
    Task<double> GetAverageRatingAsync(int serviceId);
    
    // Utility Methods
    Task<bool> ExistsAsync(int id);
    Task<int> GetCountAsync();
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<decimal> GetMinPriceAsync();
    Task<decimal> GetMaxPriceAsync();
}

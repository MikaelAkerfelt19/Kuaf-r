using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IServiceService
{
    Task<IEnumerable<Service>> GetAllAsync();
    Task<IEnumerable<Service>> GetActiveAsync(); 
    Task<IEnumerable<Service>> GetForHomePageAsync();
    Task<Service?> GetByIdAsync(int id);
    Task<Service> CreateAsync(Service service);
    Task<Service> UpdateAsync(Service service);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task UpdateDisplayOrderAsync(int id, int newOrder);
    Task ToggleHomePageVisibilityAsync(int id);
    Task<IEnumerable<Service>> GetRelatedAsync(int serviceId);
    Task<IEnumerable<Service>> SearchAsync(string? query, int? categoryId, decimal? minPrice, decimal? maxPrice);
}

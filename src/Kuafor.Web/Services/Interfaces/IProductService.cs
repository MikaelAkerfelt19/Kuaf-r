using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Product>> GetActiveAsync();
    Task<bool> UpdateStockAsync(int productId, int quantity);
    Task<decimal> CalculateCommissionAsync(int productId, decimal amount);
}

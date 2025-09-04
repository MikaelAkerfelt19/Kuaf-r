using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IPackageService
{
    Task<IEnumerable<Package>> GetAllAsync();
    Task<Package?> GetByIdAsync(int id);
    Task<Package> CreateAsync(Package package);
    Task<Package> UpdateAsync(Package package);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<PackageSale>> GetSalesAsync();
    Task<PackageSale> CreateSaleAsync(int packageId, int customerId);
    Task<bool> UseSessionAsync(int packageSaleId);
    Task<decimal> CalculateTotalPrice(Package package, List<PackageService> services);
}

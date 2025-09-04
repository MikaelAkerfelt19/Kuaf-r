using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class PackageManagementService : IPackageService
{
    private readonly ApplicationDbContext _context;

    public PackageManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Package>> GetAllAsync()
    {
        return await _context.Packages
            .Include(p => p.PackageServices)
            .ThenInclude(ps => ps.Service)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Package?> GetByIdAsync(int id)
    {
        return await _context.Packages
            .Include(p => p.PackageServices)
            .ThenInclude(ps => ps.Service)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Package> CreateAsync(Package package)
    {
        package.CreatedAt = DateTime.UtcNow;
        _context.Packages.Add(package);
        await _context.SaveChangesAsync();
        return package;
    }

    public async Task<Package> UpdateAsync(Package package)
    {
        _context.Packages.Update(package);
        await _context.SaveChangesAsync();
        return package;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var package = await _context.Packages.FindAsync(id);
        if (package == null) return false;

        _context.Packages.Remove(package);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PackageSale>> GetSalesAsync()
    {
        return await _context.PackageSales
            .Include(ps => ps.Package)
            .Include(ps => ps.Customer)
            .OrderByDescending(ps => ps.PurchaseDate)
            .ToListAsync();
    }

    public async Task<PackageSale> CreateSaleAsync(int packageId, int customerId)
    {
        var package = await _context.Packages.FindAsync(packageId);
        if (package == null) throw new ArgumentException("Package not found");

        var sale = new PackageSale
        {
            PackageId = packageId,
            CustomerId = customerId,
            RemainingSessions = package.SessionCount,
            PurchaseDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddMonths(6) // 6 ay geçerlilik
        };

        _context.PackageSales.Add(sale);
        await _context.SaveChangesAsync();
        return sale;
    }

    public async Task<bool> UseSessionAsync(int packageSaleId)
    {
        var sale = await _context.PackageSales.FindAsync(packageSaleId);
        if (sale == null || sale.RemainingSessions <= 0) return false;

        sale.RemainingSessions--;
        await _context.SaveChangesAsync();
        return true;
    }

    public decimal CalculateTotalPrice(Package package, List<PackageService> services)
    {
        return services.Sum(ps => ps.SessionCount * package.SessionUnitPrice);
    }

    Task<decimal> IPackageService.CalculateTotalPrice(Package package, List<PackageService> services)
    {
        throw new NotImplementedException();
    }
}

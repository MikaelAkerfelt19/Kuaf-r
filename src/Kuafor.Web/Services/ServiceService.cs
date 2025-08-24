using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class ServiceService : IServiceService
{
    private readonly ApplicationDbContext _context;
    
    public ServiceService(ApplicationDbContext context)
    {
        _context = context;
    }
    //Hizmet listesini gösterme için yapılan metod
    public async Task<IEnumerable<Service>> GetAllAsync()
    {
        return await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
    //hizmet detayını gösterme için yapılan metod
    public async Task<Service?> GetByIdAsync(int id)
    {
        return await _context.Services
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
    }
    //hizmet oluşturma için yapılan metod
    public async Task<Service> CreateAsync(Service service)
    {
        service.CreatedAt = DateTime.UtcNow;
        service.IsActive = true;
        
        _context.Services.Add(service);
        await _context.SaveChangesAsync();
        
        return service;
    }
    //hizmet güncelleme için yapılan metod
    public async Task<Service> UpdateAsync(Service service)
    {
        var existing = await _context.Services.FindAsync(service.Id);
        if (existing == null)
            throw new InvalidOperationException("Service not found");
            
        existing.Name = service.Name;
        existing.Description = service.Description;
        existing.IconClass = service.IconClass;
        existing.DurationMin = service.DurationMin;
        existing.Price = service.Price;
        existing.PriceFrom = service.PriceFrom;
        existing.DisplayOrder = service.DisplayOrder;
        existing.ShowOnHomePage = service.ShowOnHomePage;
        existing.IsActive = service.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return existing;
    }
    //hizmet silme için yapılan metod
    public async Task DeleteAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service != null)
        {
            service.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
    //hizmet varlığını kontrol etme için yapılan metod
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Services.AnyAsync(s => s.Id == id && s.IsActive);
    }

    public async Task<IEnumerable<Service>> GetForHomePageAsync()
    {
        return await _context.Services
            .Where(s => s.IsActive && s.ShowOnHomePage)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task UpdateDisplayOrderAsync(int id, int newOrder)
    {
        var service = await _context.Services.FindAsync(id);
        if (service != null)
        {
            service.DisplayOrder = newOrder;
            service.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ToggleHomePageVisibilityAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service != null)
        {
            service.ShowOnHomePage = !service.ShowOnHomePage;
            service.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Service>> GetActiveAsync()
    {
        return await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Service>> GetRelatedAsync(int serviceId)
    {
        var currentService = await GetByIdAsync(serviceId);
        if (currentService == null) return Enumerable.Empty<Service>();
        
        return await _context.Services
            .Where(s => s.Id != serviceId && s.IsActive && s.Category == currentService.Category)
            .Take(3)
            .ToListAsync();
    }

    public async Task<IEnumerable<Service>> SearchAsync(string? query, int? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        var queryable = _context.Services.Where(s => s.IsActive);
        
        if (!string.IsNullOrEmpty(query))
            queryable = queryable.Where(s => s.Name.Contains(query) || s.Description.Contains(query));
        
        if (categoryId.HasValue)
            queryable = queryable.Where(s => s.Category == categoryId.ToString()); 
        
        if (minPrice.HasValue)
            queryable = queryable.Where(s => s.Price >= minPrice);
        
        if (maxPrice.HasValue)
            queryable = queryable.Where(s => s.Price <= maxPrice);
        
        return await queryable.OrderBy(s => s.DisplayOrder).ThenBy(s => s.Name).ToListAsync();
    }
}

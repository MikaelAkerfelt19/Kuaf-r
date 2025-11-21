using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Enums; 
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class ServiceService : IServiceService
{
    private readonly ApplicationDbContext _context;
    
    public ServiceService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Service>> GetAllAsync()
    {
        // Context'i temizle ve fresh data getir
        _context.ChangeTracker.Clear();
        
        return await _context.Services
            .Include(s => s.Stylist)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }
    
    public async Task<Service?> GetByIdAsync(int id)
    {
        return await _context.Services
            .Include(s => s.Appointments)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
    
    public async Task<Service> CreateAsync(Service service)
    {
        service.CreatedAt = DateTime.UtcNow;
        _context.Services.Add(service);
        await _context.SaveChangesAsync();
        return service;
    }
    
    public async Task<Service> UpdateAsync(Service service)
    {
        service.UpdatedAt = DateTime.UtcNow;
        _context.Services.Update(service);
        await _context.SaveChangesAsync();
        return service;
    }
    
    public async Task DeleteAsync(int id)
    {
        var service = await _context.Services
            .Include(s => s.Appointments)
            .FirstOrDefaultAsync(s => s.Id == id);
            
        if (service != null)
        {
            // Eğer bu servise ait randevular varsa silmeye izin verme
            if (service.Appointments.Any())
            {
                throw new InvalidOperationException("Bu hizmete ait randevular bulunduğu için silinemez. Önce randevuları iptal edin.");
            }
            
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            
            // Değişikliği doğrula
            var deletedService = await _context.Services.FindAsync(id);
            if (deletedService != null)
            {
                throw new InvalidOperationException("Hizmet silinemedi. Lütfen tekrar deneyin.");
            }
        }
    }
    
    public async Task<IEnumerable<Service>> GetActiveAsync()
    {
        // Context'i temizle ve fresh data getir
        _context.ChangeTracker.Clear();
        
        return await _context.Services
            .Include(s => s.Stylist)
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Service>> GetForHomePageAsync()
    {
        return await _context.Services
            .Where(s => s.IsActive && s.ShowOnHomePage)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Service>> GetByCategoryAsync(string category)
    {
        return await _context.Services
            .Where(s => s.IsActive && s.Category == category)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Service>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        return await _context.Services
            .Where(s => s.IsActive && s.Price >= minPrice && s.Price <= maxPrice)
            .OrderBy(s => s.Price)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Service>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetActiveAsync();
        
        return await _context.Services
            .Where(s => s.IsActive && 
                       (s.Name.Contains(query) || 
                        (s.Description != null && s.Description.Contains(query)) || 
                        (s.Category != null && s.Category.Contains(query))))
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Service>> SearchAsync(string query, int? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        var queryable = _context.Services.Where(s => s.IsActive);
        
        if (!string.IsNullOrWhiteSpace(query))
        {
            queryable = queryable.Where(s => s.Name.Contains(query) || 
                                           (s.Description != null && s.Description.Contains(query)) || 
                                           (s.Category != null && s.Category.Contains(query)));
        }
        
        if (categoryId.HasValue)
        {
            // CategoryId yerine Category string kullanılıyor, bu durumda categoryId'yi string'e çevirmek gerekir
            // Ancak bu mantıklı değil, bu kısmı kaldırıyoruz
            // queryable = queryable.Where(s => s.CategoryId == categoryId.Value);
        }
        
        if (minPrice.HasValue)
        {
            queryable = queryable.Where(s => s.Price >= minPrice.Value);
        }
        
        if (maxPrice.HasValue)
        {
            queryable = queryable.Where(s => s.Price <= maxPrice.Value);
        }
        
        return await queryable
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Service>> GetRelatedAsync(int serviceId)
    {
        var service = await _context.Services.FindAsync(serviceId);
        if (service == null)
            return new List<Service>();
        
        return await _context.Services
            .Where(s => s.IsActive && s.Id != serviceId && s.Category != null && s.Category == service.Category)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .Take(4)
            .ToListAsync();
    }
    
        public async Task ToggleHomePageVisibilityAsync(int serviceId)
    {
        var service = await _context.Services.FindAsync(serviceId);
        if (service != null)
        {
            service.ShowOnHomePage = !service.ShowOnHomePage;
            service.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ToggleActiveAsync(int serviceId)
    {
        var service = await _context.Services.FindAsync(serviceId);
        if (service != null)
        {
            service.IsActive = !service.IsActive;
            service.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateDisplayOrderAsync(int serviceId, int displayOrder)
    {
        var service = await _context.Services.FindAsync(serviceId);
        if (service != null)
        {
            service.DisplayOrder = displayOrder;
            service.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> IsActiveAsync(int id)
    {
        var service = await _context.Services.FindAsync(id);
        return service?.IsActive ?? false;
    }
    
    public async Task<int> GetAppointmentCountAsync(int serviceId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Appointments.Where(a => a.ServiceId == serviceId);
        
        if (from.HasValue)
            query = query.Where(a => a.StartAt >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.StartAt <= to.Value);
        
        return await query.CountAsync();
    }
    
    public async Task<decimal> GetRevenueAsync(int serviceId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Appointments
            .Where(a => a.ServiceId == serviceId && a.Status == AppointmentStatus.Completed);
        
        if (from.HasValue)
            query = query.Where(a => a.StartAt >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.StartAt <= to.Value);
        
        return await query.SumAsync(a => a.FinalPrice);
    }
    
    public async Task<double> GetAverageRatingAsync(int serviceId)
    {
        var appointments = await _context.Appointments
            .Where(a => a.ServiceId == serviceId && a.CustomerRating != null)
            .Select(a => a.CustomerRating ?? 0)
            .ToListAsync();
        
        return appointments.Any() ? appointments.Average() : 0.0;
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Services.AnyAsync(s => s.Id == id);
    }
    
    public async Task<int> GetCountAsync()
    {
        return await _context.Services.CountAsync();
    }
    
    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        var services = await _context.Services
            .Where(s => s.IsActive && s.Category != null && s.Category.Length > 0)
            .ToListAsync();
            
        var categories = new List<string>();
        foreach (var service in services)
        {
            if (service.Category != null)
            {
                categories.Add(service.Category);
            }
        }
            
        return categories
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }
    
    public async Task<decimal> GetMinPriceAsync()
    {
        var minPrice = await _context.Services
            .Where(s => s.IsActive)
            .MinAsync(s => s.Price);
        return minPrice;
    }
    
    public async Task<decimal> GetMaxPriceAsync()
    {
        var maxPrice = await _context.Services
            .Where(s => s.IsActive)
            .MaxAsync(s => s.Price);
        return maxPrice;
    }
}

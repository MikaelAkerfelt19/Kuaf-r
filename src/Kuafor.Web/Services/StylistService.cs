using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class StylistService : IStylistService
{
    private readonly ApplicationDbContext _context;
    
    public StylistService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Stylist>> GetAllAsync()
    {
        return await _context.Stylists
            .Include(s => s.Branch)
            .Where(s => s.IsActive)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();
    }
    
    public async Task<Stylist?> GetByIdAsync(int id)
    {
        return await _context.Stylists
            .Include(s => s.Branch)
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
    }
    
    public async Task<Stylist> CreateAsync(Stylist stylist)
    {
        stylist.CreatedAt = DateTime.UtcNow;
        stylist.IsActive = true;
        
        _context.Stylists.Add(stylist);
        await _context.SaveChangesAsync();
        
        return stylist;
    }
    
    public async Task<Stylist> UpdateAsync(Stylist stylist)
    {
        var existing = await _context.Stylists.FindAsync(stylist.Id);
        if (existing == null)
            throw new InvalidOperationException("Stylist not found");
            
        existing.FirstName = stylist.FirstName;
        existing.LastName = stylist.LastName;
        existing.Phone = stylist.Phone;
        existing.Email = stylist.Email;
        existing.Bio = stylist.Bio;
        existing.Rating = stylist.Rating;
        existing.BranchId = stylist.BranchId;
        existing.IsActive = stylist.IsActive;
        
        await _context.SaveChangesAsync();
        
        return existing;
    }
    
    public async Task DeleteAsync(int id)
    {
        var stylist = await _context.Stylists.FindAsync(id);
        if (stylist != null)
        {
            stylist.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Stylists.AnyAsync(s => s.Id == id && s.IsActive);
    }
    
    public async Task<Stylist?> GetByUserIdAsync(string userId)
    {
        return await _context.Stylists
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }
    
    public async Task<IEnumerable<Stylist>> GetByBranchAsync(int branchId)
    {
        return await GetByBranchIdAsync(branchId);
    }

    public async Task<IEnumerable<Stylist>> GetActiveAsync()
    {
        return await _context.Stylists
            .Include(s => s.Branch)
            .Where(s => s.IsActive)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Stylist>> GetByServiceAsync(int serviceId)
    {
        // Bu hizmeti sunan kuaförleri bul
        // Şimdilik tüm aktif kuaförleri döndür, daha sonra hizmet-kuaför ilişkisi eklenebilir
        return await _context.Stylists
            .Include(s => s.Branch)
            .Where(s => s.IsActive)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Stylist>> GetByBranchIdAsync(int branchId)
    {
        return await _context.Stylists
            .Where(s => s.BranchId == branchId && s.IsActive)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();
    }

    // Yeni eklenen method'lar
    public async Task<List<TopStylist>> GetTopStylistsByWeekAsync(DateTime weekStart, DateTime weekEnd)
    {
        var topStylists = await _context.Appointments
            .Where(a => a.StartTime >= weekStart && a.StartTime <= weekEnd)
            .GroupBy(a => new { a.StylistId, a.Stylist.FirstName, a.Stylist.LastName })
            .Select(g => new TopStylist
            {
                Id = g.Key.StylistId,
                Name = $"{g.Key.FirstName} {g.Key.LastName}",
                AppointmentCount = g.Count(),
                TotalRevenue = g.Sum(a => a.TotalPrice),
                AverageRating = (double)g.Average(a => a.Stylist.Rating)
            })
            .OrderByDescending(s => s.AppointmentCount)
            .Take(10)
            .ToListAsync();

        return topStylists;
    }

    public async Task<List<TopStylist>> GetTopStylistsByMonthAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var topStylists = await _context.Appointments
            .Where(a => a.StartTime >= startDate && a.StartTime <= endDate)
            .GroupBy(a => new { a.StylistId, a.Stylist.FirstName, a.Stylist.LastName })
            .Select(g => new TopStylist
            {
                Id = g.Key.StylistId,
                Name = $"{g.Key.FirstName} {g.Key.LastName}",
                AppointmentCount = g.Count(),
                TotalRevenue = g.Sum(a => a.TotalPrice),
                AverageRating = (double)g.Average(a => a.Stylist.Rating)
            })
            .OrderByDescending(s => s.AppointmentCount)
            .Take(10)
            .ToListAsync();

        return topStylists;
    }
}

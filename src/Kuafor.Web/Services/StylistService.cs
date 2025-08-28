using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Enums; 

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
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();
    }
    
    public async Task<Stylist?> GetByIdAsync(int id)
    {
        return await _context.Stylists
            .Include(s => s.Branch)
            .Include(s => s.Appointments)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
    
    public async Task<Stylist> CreateAsync(Stylist stylist)
    {
        stylist.CreatedAt = DateTime.UtcNow;
        _context.Stylists.Add(stylist);
        await _context.SaveChangesAsync();
        return stylist;
    }
    
    public async Task<Stylist> UpdateAsync(Stylist stylist)
    {
        stylist.UpdatedAt = DateTime.UtcNow;
        _context.Stylists.Update(stylist);
        await _context.SaveChangesAsync();
        return stylist;
    }
    
    public async Task DeleteAsync(int id)
    {
        var stylist = await _context.Stylists
            .Include(s => s.Appointments)
            .FirstOrDefaultAsync(s => s.Id == id);
            
        if (stylist != null)
        {
            // Eğer bu kuaföre ait randevular varsa silmeye izin verme
            if (stylist.Appointments.Any())
            {
                throw new InvalidOperationException("Bu kuaföre ait randevular bulunduğu için silinemez. Önce randevuları iptal edin.");
            }
            
            _context.Stylists.Remove(stylist);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<IEnumerable<Stylist>> GetActiveAsync()
    {
        return await _context.Stylists
            .Where(s => s.IsActive)
            .Include(s => s.Branch)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Stylist>> GetByBranchAsync(int branchId)
    {
        return await _context.Stylists
            .Where(s => s.BranchId == branchId && s.IsActive)
            .OrderBy(s => s.FirstName)
            .ThenBy(s => s.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Stylist>> GetByServiceAsync(int serviceId)
    {
        // Bu hizmeti sunan kuaförleri bul
        var stylistIds = await _context.Appointments
            .Where(a => a.ServiceId == serviceId && a.Status != AppointmentStatus.Cancelled)
            .Select(a => a.StylistId)
            .Distinct()
            .ToListAsync();
            
        if (!stylistIds.Any())
            return new List<Stylist>();
            
        return await _context.Stylists
            .Where(s => stylistIds.Contains(s.Id) && s.IsActive)
            .ToListAsync();
    }

    public async Task<Stylist?> GetByUserIdAsync(string userId)
    {
        return await _context.Stylists
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }
    
    public async Task<IEnumerable<Stylist>> GetTopRatedAsync(int count = 5)
    {
        return await _context.Stylists
            .Where(s => s.IsActive && s.Rating > 0)
            .OrderByDescending(s => s.Rating)
            .ThenBy(s => s.FirstName)
            .Take(count)
            .Include(s => s.Branch)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Stylist>> GetTopStylistsByWeekAsync(int count = 5)
    {
        var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var weekEnd = weekStart.AddDays(7);
        
        return await _context.Stylists
            .Where(s => s.IsActive)
            .Select(s => new { 
                Stylist = s,
                WeeklyRevenue = s.Appointments
                    .Where(a => a.StartAt >= weekStart && a.StartAt < weekEnd && a.Status == AppointmentStatus.Completed)
                    .Sum(a => a.FinalPrice)
            })
            .OrderByDescending(x => x.WeeklyRevenue)
            .Take(count)
            .Select(x => x.Stylist)
            .Include(s => s.Branch)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Stylist>> GetTopStylistsByMonthAsync(int count = 5)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var monthEnd = monthStart.AddMonths(1);
        
        return await _context.Stylists
            .Where(s => s.IsActive)
            .Select(s => new { 
                Stylist = s,
                MonthlyRevenue = s.Appointments
                    .Where(a => a.StartAt >= monthStart && a.StartAt < monthEnd && a.Status == AppointmentStatus.Completed)
                    .Sum(a => a.FinalPrice)
            })
            .OrderByDescending(x => x.MonthlyRevenue)
            .Take(count)
            .Select(x => x.Stylist)
            .Include(s => s.Branch)
            .ToListAsync();
    }
    
    public async Task<bool> IsActiveAsync(int id)
    {
        var stylist = await _context.Stylists.FindAsync(id);
        return stylist?.IsActive ?? false;
    }
    
    public async Task<bool> IsAvailableAsync(int stylistId, DateTime dateTime, int durationMinutes)
    {
        var stylist = await _context.Stylists
            .Include(s => s.Branch)
            .FirstOrDefaultAsync(s => s.Id == stylistId);
        
        if (stylist == null || !stylist.IsActive)
            return false;
        
        // Çalışma saatleri kontrolü (basit kontrol)
        var hour = dateTime.Hour;
        if (hour < 9 || hour >= 18)
            return false;
        
        // Randevu çakışması kontrolü
        var endTime = dateTime.AddMinutes(durationMinutes);
        var hasConflict = await _context.Appointments
            .AnyAsync(a => a.StylistId == stylistId && 
                          a.Status != AppointmentStatus.Cancelled &&
                          ((a.StartAt < endTime && a.EndAt > dateTime)));
        
        return !hasConflict;
    }
    
    public async Task<int> GetAppointmentCountAsync(int stylistId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Appointments.Where(a => a.StylistId == stylistId);
        
        if (from.HasValue)
            query = query.Where(a => a.StartAt >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.StartAt <= to.Value);
        
        return await query.CountAsync();
    }
    
    public async Task<decimal> GetRevenueAsync(int stylistId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Appointments
            .Where(a => a.StylistId == stylistId && a.Status == AppointmentStatus.Completed);
        
        if (from.HasValue)
            query = query.Where(a => a.StartAt >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.StartAt <= to.Value);
        
        return await query.SumAsync(a => a.FinalPrice);
    }
    
    public async Task<double> GetAverageRatingAsync(int stylistId)
    {
        var stylist = await _context.Stylists.FindAsync(stylistId);
        return (double)(stylist?.Rating ?? 0m);
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Stylists.AnyAsync(s => s.Id == id);
    }
    
    public async Task<bool> ExistsByUserIdAsync(string userId)
    {
        return await _context.Stylists.AnyAsync(s => s.UserId == userId);
    }
    
    public async Task<int> GetCountAsync()
    {
        return await _context.Stylists.CountAsync();
    }
    
    public async Task<int> GetCountByBranchAsync(int branchId)
    {
        return await _context.Stylists
            .Where(s => s.BranchId == branchId && s.IsActive)
            .CountAsync();
    }
}

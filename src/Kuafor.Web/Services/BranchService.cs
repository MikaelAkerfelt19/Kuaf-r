using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Enums; 
using Kuafor.Web.Services.Interfaces;
namespace Kuafor.Web.Services;

public class BranchService : IBranchService
{
    private readonly ApplicationDbContext _context;
    
    public BranchService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Branch>> GetAllAsync()
    {
        return await _context.Branches
            .Include(b => b.Stylists)
            .OrderBy(b => b.DisplayOrder)
            .ThenBy(b => b.Name)
            .ToListAsync();
    }
    
    public async Task<Branch?> GetByIdAsync(int id)
    {
        return await _context.Branches
            .Include(b => b.Stylists)
            .Include(b => b.Appointments)
            .FirstOrDefaultAsync(b => b.Id == id);
    }
    
    public async Task<Branch> CreateAsync(Branch branch)
    {
        branch.CreatedAt = DateTime.UtcNow;
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();
        return branch;
    }
    
    public async Task<Branch> UpdateAsync(Branch branch)
    {
        branch.UpdatedAt = DateTime.UtcNow;
        _context.Branches.Update(branch);
        await _context.SaveChangesAsync();
        return branch;
    }
    
    public async Task DeleteAsync(int id)
    {
        var branch = await _context.Branches
            .Include(b => b.Stylists)
            .Include(b => b.Appointments)
            .FirstOrDefaultAsync(b => b.Id == id);
            
        if (branch != null)
        {
            // Eğer bu şubeye ait kuaförler varsa silmeye izin verme
            if (branch.Stylists.Any())
            {
                throw new InvalidOperationException("Bu şubeye ait kuaförler bulunduğu için silinemez. Önce kuaförleri başka şubeye taşıyın veya silin.");
            }
            
            // Eğer bu şubeye ait randevular varsa silmeye izin verme
            if (branch.Appointments.Any())
            {
                throw new InvalidOperationException("Bu şubeye ait randevular bulunduğu için silinemez. Önce randevuları iptal edin.");
            }
            
            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<IEnumerable<Branch>> GetActiveAsync()
    {
        return await _context.Branches
            .Where(b => b.IsActive)
            .OrderBy(b => b.DisplayOrder)
            .ThenBy(b => b.Name)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Branch>> GetForHomePageAsync()
    {
        return await _context.Branches
            .Where(b => b.IsActive && b.ShowOnHomePage)
            .OrderBy(b => b.DisplayOrder)
            .ThenBy(b => b.Name)
            .ToListAsync();
    }
    
    public async Task<Branch?> GetByNameAsync(string name)
    {
        return await _context.Branches
            .FirstOrDefaultAsync(b => b.Name == name);
    }
    
    public async Task<IEnumerable<Branch>> GetByCityAsync(string city)
    {
        return await _context.Branches
            .Where(b => b.IsActive && !string.IsNullOrEmpty(b.Address) && b.Address.Contains(city))
            .OrderBy(b => b.Name)
            .ToListAsync();
    }
    
    public async Task<bool> IsActiveAsync(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        return branch?.IsActive ?? false;
    }
    
    public async Task<int> GetStylistCountAsync(int branchId)
    {
        return await _context.Stylists
            .Where(s => s.BranchId == branchId && s.IsActive)
            .CountAsync();
    }
    
    public async Task<int> GetAppointmentCountAsync(int branchId, DateTime? date = null)
    {
        var query = _context.Appointments.Where(a => a.BranchId == branchId);
        
        if (date.HasValue)
        {
            query = query.Where(a => a.StartAt.Date == date.Value.Date);
        }
        
        return await query.CountAsync();
    }
    
    public async Task<decimal> GetRevenueAsync(int branchId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Appointments
            .Where(a => a.BranchId == branchId && a.Status == AppointmentStatus.Completed);
        
        if (from.HasValue)
            query = query.Where(a => a.StartAt >= from.Value);
        if (to.HasValue)
            query = query.Where(a => a.StartAt <= to.Value);
        
        return await query.SumAsync(a => a.FinalPrice);
    }
    
    public async Task<object> GetPerformanceAsync(int branchId, DateTime? from = null, DateTime? to = null)
    {
        var revenue = await GetRevenueAsync(branchId, from, to);
        var appointmentCount = await GetAppointmentCountAsync(branchId);
        var stylistCount = await GetStylistCountAsync(branchId);
        
        return new
        {
            BranchId = branchId,
            Revenue = revenue,
            AppointmentCount = appointmentCount,
            StylistCount = stylistCount,
            AverageRevenuePerAppointment = appointmentCount > 0 ? revenue / appointmentCount : 0,
            Period = new { From = from, To = to }
        };
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Branches.AnyAsync(b => b.Id == id);
    }
    
    public async Task<int> GetCountAsync()
    {
        return await _context.Branches.CountAsync();
    }
    
    public async Task<IEnumerable<string>> GetCitiesAsync()
    {
        var branches = await _context.Branches
            .Where(b => b.IsActive && !string.IsNullOrEmpty(b.Address))
            .Select(b => b.Address)
            .ToListAsync();
            
        return branches
            .Where(address => !string.IsNullOrEmpty(address))
            .Select(address => address!.Split(',')[0].Trim())
            .Where(city => !string.IsNullOrEmpty(city))
            .Distinct()
            .OrderBy(city => city)
            .ToList();
    }
}

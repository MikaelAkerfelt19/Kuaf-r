using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
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
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }
    
    public async Task<Branch?> GetByIdAsync(int id)
    {
        return await _context.Branches
            .Include(b => b.Stylists)
            .FirstOrDefaultAsync(b => b.Id == id && b.IsActive);
    }
    
    public async Task<Branch> CreateAsync(Branch branch)
    {
        branch.CreatedAt = DateTime.UtcNow;
        branch.IsActive = true;
        
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();
        
        return branch;
    }
    
    public async Task<Branch> UpdateAsync(Branch branch)
    {
        var existing = await _context.Branches.FindAsync(branch.Id);
        if (existing == null)
            throw new InvalidOperationException("Branch not found");
            
        existing.Name = branch.Name;
        existing.Address = branch.Address;
        existing.Phone = branch.Phone;
        existing.Email = branch.Email; // Yeni
        existing.Description = branch.Description; // Yeni
        existing.WorkingHours = branch.WorkingHours; // Yeni
        existing.ShowOnHomePage = branch.ShowOnHomePage; // Yeni
        existing.DisplayOrder = branch.DisplayOrder; // Yeni
        existing.IsActive = branch.IsActive;
        existing.UpdatedAt = DateTime.UtcNow; // Yeni
        
        await _context.SaveChangesAsync();
        
        return existing;
    }
    
    public async Task DeleteAsync(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch != null)
        {
            branch.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Branches.AnyAsync(b => b.Id == id && b.IsActive);
    }

    public async Task<IEnumerable<Branch>> GetForHomePageAsync()
    {
        return await _context.Branches
            .Where(b => b.IsActive && b.ShowOnHomePage)
            .OrderBy(b => b.DisplayOrder)
            .ThenBy(b => b.Name)
            .ToListAsync();
    }

    public async Task UpdateDisplayOrderAsync(int id, int newOrder)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch != null)
        {
            branch.DisplayOrder = newOrder;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ToggleHomePageVisibilityAsync(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch != null)
        {
            branch.ShowOnHomePage = !branch.ShowOnHomePage;
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

    // Yeni eklenen method
    public async Task<List<BranchPerformance>> GetPerformanceAsync(DateTime start, DateTime end)
    {
        var branchPerformance = await _context.Appointments
            .Where(a => a.StartTime >= start && a.StartTime <= end)
            .GroupBy(a => new { a.BranchId, a.Branch.Name })
            .Select(g => new BranchPerformance
            {
                Id = g.Key.BranchId,
                Name = g.Key.Name,
                AppointmentCount = g.Count(),
                TotalRevenue = g.Sum(a => a.TotalPrice),
                AverageRating = 0.0 // Branch entity'de Rating property yok, default değer
            })
            .OrderByDescending(b => b.TotalRevenue)
            .ToListAsync();

        return branchPerformance;
    }
}

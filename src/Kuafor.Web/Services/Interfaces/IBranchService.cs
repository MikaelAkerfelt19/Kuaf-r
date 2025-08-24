using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IBranchService
{
    Task<IEnumerable<Branch>> GetAllAsync();
    Task<Branch?> GetByIdAsync(int id);
    Task<IEnumerable<Branch>> GetActiveAsync();
    Task<IEnumerable<Branch>> GetForHomePageAsync();
    Task<Branch> CreateAsync(Branch branch);
    Task<Branch> UpdateAsync(Branch branch);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    
    // Yeni eklenen method
    Task<List<BranchPerformance>> GetPerformanceAsync(DateTime start, DateTime end);
    
    // Eksik method'lar
    Task UpdateDisplayOrderAsync(int id, int newOrder);
    Task ToggleHomePageVisibilityAsync(int id);
}

// BranchPerformance DTO'su
public class BranchPerformance
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AppointmentCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AverageRating { get; set; }
}

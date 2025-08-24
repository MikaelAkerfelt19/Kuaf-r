using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IStylistService
{
    Task<IEnumerable<Stylist>> GetAllAsync();
    Task<Stylist?> GetByIdAsync(int id);
    Task<IEnumerable<Stylist>> GetActiveAsync();
    Task<IEnumerable<Stylist>> GetByBranchIdAsync(int branchId);
    Task<IEnumerable<Stylist>> GetByServiceAsync(int serviceId);
    Task<Stylist> CreateAsync(Stylist stylist);
    Task<Stylist> UpdateAsync(Stylist stylist);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    
    // Yeni eklenen method'lar
    Task<List<TopStylist>> GetTopStylistsByWeekAsync(DateTime weekStart, DateTime weekEnd);
    Task<List<TopStylist>> GetTopStylistsByMonthAsync(int year, int month);
    
    // Eksik method'lar
    Task<Stylist?> GetByUserIdAsync(string userId);
    Task<IEnumerable<Stylist>> GetByBranchAsync(int branchId);
}

// TopStylist DTO'su
public class TopStylist
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AppointmentCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AverageRating { get; set; }
}

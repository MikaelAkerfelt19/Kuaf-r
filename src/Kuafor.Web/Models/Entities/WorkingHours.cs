using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class WorkingHours
{
    public int Id { get; set; }
    
    [Required]
    public int BranchId { get; set; }
    
    [Required]
    public DayOfWeek DayOfWeek { get; set; }
    
    [Required]
    public TimeSpan OpenTime { get; set; }
    
    [Required]
    public TimeSpan CloseTime { get; set; }
    
    public bool IsWorkingDay { get; set; } = true;
    
    public TimeSpan? BreakStart { get; set; }
    
    public TimeSpan? BreakEnd { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public virtual Branch Branch { get; set; } = null!;
}
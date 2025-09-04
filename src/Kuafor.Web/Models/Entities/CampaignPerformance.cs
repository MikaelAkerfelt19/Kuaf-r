using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class CampaignPerformance
{
    public int Id { get; set; }
    
    [Required]
    public int CampaignId { get; set; }
    
    [Required]
    public int SentCount { get; set; }
    
    [Required]
    public int DeliveredCount { get; set; }
    
    [Required]
    public int OpenedCount { get; set; }
    
    [Required]
    public int ClickedCount { get; set; }
    
    [Required]
    public int ConvertedCount { get; set; }
    
    public double DeliveryRate { get; set; }
    public double OpenRate { get; set; }
    public double ClickRate { get; set; }
    public double ConversionRate { get; set; }
    
    [StringLength(100)]
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Campaign Campaign { get; set; } = null!;
}

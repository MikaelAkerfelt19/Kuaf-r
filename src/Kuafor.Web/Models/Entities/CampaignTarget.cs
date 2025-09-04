using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class CampaignTarget
{
    public int Id { get; set; }
    
    [Required]
    public int CampaignId { get; set; }
    
    [Required, StringLength(50)]
    public string TargetType { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? TargetValue { get; set; }
    
    [StringLength(100)]
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Campaign Campaign { get; set; } = null!;
}

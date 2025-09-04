using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class Campaign
{
    public int Id { get; set; }
    
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required, StringLength(50)]
    public string CampaignType { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required, StringLength(50)]
    public string Status { get; set; } = "Draft";
    
    [Required, StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;
    
    public int? CreatedById { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Stylist? CreatedByUser { get; set; }
    
    // Navigation properties
    public virtual ICollection<CampaignTarget> Targets { get; set; } = new List<CampaignTarget>();
    public virtual ICollection<CampaignMessage> Messages { get; set; } = new List<CampaignMessage>();
    public virtual ICollection<CampaignPerformance> Performance { get; set; } = new List<CampaignPerformance>();
}

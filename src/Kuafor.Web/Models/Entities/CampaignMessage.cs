using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class CampaignMessage
{
    public int Id { get; set; }
    
    [Required]
    public int CampaignId { get; set; }
    
    [Required]
    public int Sequence { get; set; }
    
    [Required, StringLength(50)]
    public string MessageType { get; set; } = string.Empty;
    
    [Required, StringLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    public string Body { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Campaign Campaign { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class MessageRecipient
{
    public int Id { get; set; }
    
    [Required]
    public int MessageCampaignId { get; set; }
    
    [Required]
    public int CustomerId { get; set; }
    
    [Required, StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Sent, Failed, Delivered
    
    public DateTime? SentAt { get; set; }
    
    [StringLength(500)]
    public string? ErrorMessage { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual MessageCampaign MessageCampaign { get; set; } = null!;
    public virtual Customer Customer { get; set; } = null!;
}

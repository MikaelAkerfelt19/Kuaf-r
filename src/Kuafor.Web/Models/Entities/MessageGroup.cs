using System.ComponentModel.DataAnnotations;
namespace Kuafor.Web.Models.Entities;

public class MessageGroup
{
    public int Id { get; set; }
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<MessageGroupMember> Members { get; set; } = new List<MessageGroupMember>();
}

public class MessageGroupMember
{
    public int Id { get; set; }
    public int MessageGroupId { get; set; }
    public int CustomerId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    public virtual MessageGroup MessageGroup { get; set; } = null!;
    public virtual Customer Customer { get; set; } = null!;
}

public class MessageCampaign
{
    public int Id { get; set; }
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required, StringLength(1000)]
    public string Content { get; set; } = string.Empty;
    [Required, StringLength(20)]
    public string Type { get; set; } = "SMS"; // SMS, WhatsApp, Email
    public int? MessageGroupId { get; set; }
    [Required, StringLength(20)]
    public string Status { get; set; } = "Draft"; // Draft, Sent, Failed
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual MessageGroup? MessageGroup { get; set; }
    public virtual ICollection<MessageRecipient> Recipients { get; set; } = new List<MessageRecipient>();
}

// Mesaj kara listesi -
public class MessageBlacklist
{
    public int Id { get; set; }
    
    [Required, StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? CustomerName { get; set; }
    
    [Required, StringLength(50)]
    public string Reason { get; set; } = string.Empty; // "OptOut", "InvalidNumber", "Complaint"
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

// Mesaj raporları 
public class MessageReport
{
    public int Id { get; set; }
    
    [Required]
    public int MessageCampaignId { get; set; }
    
    [Required, StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required, StringLength(20)]
    public string MessageType { get; set; } = string.Empty; // SMS, WhatsApp
    
    [Required, StringLength(20)]
    public string DeliveryStatus { get; set; } = string.Empty; // Sent, Delivered, Failed, Read
    
    [StringLength(1000)]
    public string MessageContent { get; set; } = string.Empty;
    
    public decimal? Cost { get; set; } // Mesaj maliyeti
    
    [StringLength(500)]
    public string? ErrorMessage { get; set; }
    
    public DateTime SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    
    public virtual MessageCampaign MessageCampaign { get; set; } = null!;
}

// Kredi yönetimi 
public class MessageCredit
{
    public int Id { get; set; }
    
    [Required, StringLength(20)]
    public string Type { get; set; } = string.Empty; // SMS, WhatsApp
    
    public int CreditAmount { get; set; } = 0;
    
    public decimal? CostPerCredit { get; set; }
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Mesaj şablonları 
public class MessageTemplate
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required, StringLength(20)]
    public string Type { get; set; } = string.Empty; // SMS, WhatsApp
    
    [Required, StringLength(1000)]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
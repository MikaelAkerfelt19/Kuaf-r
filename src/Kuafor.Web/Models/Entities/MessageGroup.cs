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
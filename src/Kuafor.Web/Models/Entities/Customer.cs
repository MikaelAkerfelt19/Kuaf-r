using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class Customer
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [StringLength(100)]
    public string? Email { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [StringLength(10)]
    public string? Gender { get; set; }
    
    [StringLength(50)]
    public string? Segment { get; set; }
    
    public DateTime? LastVisitDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    // Computed properties
    public string Name => $"{FirstName} {LastName}";
    public string FullName => $"{FirstName} {LastName}";
    
    [StringLength(20)]
    public string Status { get; set; } = "Active"; // Active, Inactive, Suspended
    
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    // Müşteri mesajları için navigation property
    public virtual ICollection<MessageRecipient> MessageRecipients { get; set; } = new List<MessageRecipient>();
}

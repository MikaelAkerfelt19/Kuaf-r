using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class Stylist
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(100)]
    public string? Email { get; set; }
    
    [Range(0, 5)]
    public decimal Rating { get; set; } = 0;
    
    [StringLength(500)]
    public string? Bio { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign keys
    [Required]
    public int BranchId { get; set; }
    public string? UserId { get; set; } // Identity User ID
    
    // Computed property for full name
    public string Name => $"{FirstName} {LastName}";
    
    // Navigation properties
    public virtual Branch Branch { get; set; } = null!;
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();

    public virtual ICollection<StylistWorkingHours> WorkingHours { get; set; } = new List<StylistWorkingHours>();
}

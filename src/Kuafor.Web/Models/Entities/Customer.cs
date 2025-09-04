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
    
    [StringLength(100)]
    public string? Email { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    public DateTime? BirthDate { get; set; } // Alias for DateOfBirth
    
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
    
    // Computed property for full name
    public string Name => $"{FirstName} {LastName}";
    
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

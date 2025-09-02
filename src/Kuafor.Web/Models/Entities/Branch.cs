using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class Branch
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Address { get; set; }
    
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(100)]
    public string? Email { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    

    
    public bool IsActive { get; set; } = true;
    
    public bool ShowOnHomePage { get; set; } = true;
    
    public int DisplayOrder { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<Stylist> Stylists { get; set; } = new List<Stylist>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

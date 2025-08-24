using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class Testimonial
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required, StringLength(500)]
    public string Message { get; set; } = string.Empty;
    
    [Range(1, 5)]
    public int Rating { get; set; } = 5;
    
    public bool IsActive { get; set; } = true;
    
    public bool IsApproved { get; set; } = false; // Admin onayı için
    
    public bool ShowOnHomePage { get; set; } = true; // Ana sayfada gösterilsin mi?
    
    public int DisplayOrder { get; set; } = 0; // Ana sayfada sıralama için
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Admin tarafından düzenlenebilir
    public string? AdminNotes { get; set; }
}

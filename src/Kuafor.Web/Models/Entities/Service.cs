using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class Service
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int StylistId { get; set; }

    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? DetailedDescription { get; set; } // Detaylı açıklama
    
    [StringLength(50)]
    public string IconClass { get; set; } = "bi bi-scissors"; // Bootstrap Icons class
    
    [Range(5, 480)]
    public int DurationMin { get; set; } = 30;
    
    [Range(0, 100000)]
    public decimal Price { get; set; } = 0;

    [Range(0, 100000)]
    public decimal? PriceFrom { get; set; } // "Başlangıç" fiyatı
    
    [StringLength(50)]
    public string? Category { get; set; } // Hizmet kategorisi (saç kesimi, renklendirme, vb.)
    
    public int DisplayOrder { get; set; } = 0; // Ana sayfada sıralama için
    
    public bool IsActive { get; set; } = true;

    public bool ShowOnHomePage { get; set; } = true; // Ana sayfada gösterilsin mi?

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Stylist Stylist { get; set; } = null!;
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}

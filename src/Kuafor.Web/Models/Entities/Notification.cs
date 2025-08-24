using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class Notification
{
    public int Id { get; set; }
    
    // Hangi kullanıcı için
    [Required]
    public string UserId { get; set; } = string.Empty; // Identity User ID
    
    // Bildirim başlığı
    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;
    
    // Bildirim mesajı
    [Required, StringLength(500)]
    public string Message { get; set; } = string.Empty;
    
    // Bildirim tipi
    [Required, StringLength(20)]
    public string Type { get; set; } = "Info"; // Info, Success, Warning, Error
    
    // Okundu mu?
    public bool IsRead { get; set; } = false;
    
    // Bildirim zamanı
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Okunma zamanı
    public DateTime? ReadAt { get; set; }
    
    // Bildirim linki (opsiyonel)
    [StringLength(200)]
    public string? ActionUrl { get; set; }
    
    // Bildirim ikonu (opsiyonel)
    [StringLength(50)]
    public string? IconClass { get; set; } // Bootstrap icon class
}

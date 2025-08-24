using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class Report
{
    public int Id { get; set; }
    
    // Rapor adı
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    // Rapor tipi
    [Required, StringLength(50)]
    public string Type { get; set; } = string.Empty; // Daily, Weekly, Monthly, Custom
    
    // Rapor parametreleri (JSON)
    [StringLength(1000)]
    public string? Parameters { get; set; } // JSON formatında filtreler
    
    // Rapor verisi (JSON)
    [Required]
    public string Data { get; set; } = string.Empty; // JSON formatında rapor verisi
    
    // Rapor formatı
    [StringLength(20)]
    public string Format { get; set; } = "JSON"; // JSON, CSV, PDF, Excel
    
    // Rapor boyutu (bytes)
    public long FileSize { get; set; } = 0;
    
    // Oluşturan kullanıcı
    [Required]
    public string CreatedBy { get; set; } = string.Empty; // User ID
    
    // Oluşturulma zamanı
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Geçerlilik süresi (gün)
    public int ExpiryDays { get; set; } = 30;
    
    // Rapor açıklaması
    [StringLength(500)]
    public string? Description { get; set; }
}

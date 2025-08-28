using System.ComponentModel.DataAnnotations;
using Kuafor.Web.Models.Enums;

namespace Kuafor.Web.Models.Entities;

public class Appointment
{
    public int Id { get; set; }
    
    [Required]
    public DateTime StartAt { get; set; } // UTC olarak saklanacak
    
    [Required]
    public DateTime EndAt { get; set; } // UTC olarak saklanacak
    
    // Alias properties for compatibility
    public DateTime StartTime => StartAt;
    public DateTime EndTime => EndAt;
    
    [Required, StringLength(20)]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    // Fiyat bilgileri
    [Range(0, 100000)]
    public decimal TotalPrice { get; set; } // Toplam fiyat
    
    [Range(0, 100000)]
    public decimal DiscountAmount { get; set; } = 0; // İndirim tutarı
    
    [Range(0, 100000)]
    public decimal FinalPrice { get; set; } // İndirim sonrası fiyat
    
    // Zaman takibi
    public DateTime? CheckInTime { get; set; } // Müşteri geliş zamanı
    public DateTime? ServiceStartTime { get; set; } // Hizmet başlama zamanı
    public DateTime? ServiceEndTime { get; set; } // Hizmet bitiş zamanı
    
    // İptal ve erteleme
    [StringLength(200)]
    public string? CancellationReason { get; set; } // İptal nedeni
    
    public DateTime? CancelledAt { get; set; } // İptal zamanı
    
    public string? CancelledBy { get; set; } // İptal eden (Customer, Stylist, Admin)
    
    // Müşteri memnuniyeti
    [Range(1, 5)]
    public int? CustomerRating { get; set; } // Müşteri puanı (1-5)
    
    [StringLength(500)]
    public string? CustomerFeedback { get; set; } // Müşteri yorumu
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign keys
    public int ServiceId { get; set; }
    public int StylistId { get; set; }
    public int BranchId { get; set; }
    public int CustomerId { get; set; }
    public int? CouponId { get; set; }
    
    // Navigation properties
    public virtual Service Service { get; set; } = null!;
    public virtual Stylist Stylist { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
    public virtual Customer Customer { get; set; } = null!;
    public virtual Coupon? Coupon { get; set; }
    
    // Ödeme ilişkisi
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

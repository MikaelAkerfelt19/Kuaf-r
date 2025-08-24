using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class Payment
{
    public int Id { get; set; }
    
    // Hangi randevu için
    public int AppointmentId { get; set; }
    
    // Ödeme tutarı
    [Required, Range(0, 100000)]
    public decimal Amount { get; set; }
    
    // Ödeme yöntemi
    [Required, StringLength(20)]
    public string Method { get; set; } = "Cash"; // Cash, CreditCard, Online
    
    // Ödeme durumu
    [Required, StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
    
    // Ödeme tarihi
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    
    // İşlem numarası (online ödemeler için)
    [StringLength(100)]
    public string? TransactionId { get; set; }
    
    // Kart bilgileri (maskelenmiş)
    [StringLength(20)]
    public string? MaskedCardNumber { get; set; } // ****1234
    
    // Ödeme notları
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Appointment Appointment { get; set; } = null!;
}

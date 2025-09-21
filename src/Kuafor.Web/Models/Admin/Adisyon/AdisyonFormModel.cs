using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Admin.Adisyon
{
    public class AdisyonFormModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Müşteri seçimi zorunludur")]
        public int CustomerId { get; set; }
        
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;
        
        [EmailAddress]
        [StringLength(100)]
        public string CustomerEmail { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Toplam tutar zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Toplam tutar 0'dan büyük olmalıdır")]
        public decimal TotalAmount { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "İndirim tutarı negatif olamaz")]
        public decimal DiscountAmount { get; set; } = 0;
        
        public decimal FinalAmount => TotalAmount - DiscountAmount;
        
        [Required(ErrorMessage = "Ödeme yöntemi seçimi zorunludur")]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Nakit";
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public List<AdisyonServiceItem> Services { get; set; } = new();
    }
    
    public class AdisyonServiceItem
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal Total => Price * Quantity;
    }
    
    public class AdisyonServiceModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}

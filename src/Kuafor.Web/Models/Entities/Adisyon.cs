using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities
{
    public class Adisyon
    {
        public int Id { get; set; }
        
        public int? CustomerId { get; set; } // Null olabilir (yeni müşteri için)
        
        [Required, StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string CustomerEmail { get; set; } = string.Empty;
        
        [Range(0, 999999)]
        public decimal TotalAmount { get; set; }
        
        [Range(0, 999999)]
        public decimal DiscountAmount { get; set; } = 0;
        
        [Range(0, 999999)]
        public decimal FinalAmount { get; set; }
        
        [Required, StringLength(50)]
        public string PaymentMethod { get; set; } = "Nakit";
        
        [StringLength(500)]
        public string? Notes { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<AdisyonDetail> AdisyonDetails { get; set; } = new List<AdisyonDetail>();
    }
    
    public class AdisyonDetail
    {
        public int Id { get; set; }
        public int AdisyonId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        
        public virtual Adisyon Adisyon { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
    }
}

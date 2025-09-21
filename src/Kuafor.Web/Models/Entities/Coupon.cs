using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class Coupon
{
    public int Id { get; set; }
    
    [Required, StringLength(40)]
    public string Code { get; set; } = string.Empty;
    
    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;
    
    public string DiscountType { get; set; } = "Percent"; // Percent, Amount
    
    [Range(0, 999999)]
    public decimal Amount { get; set; } = 0;
    
    [Range(0, 999999)]
    public decimal? MinSpend { get; set; }
    
    public DateTime? ExpiresAt { get; set; }
    
    public int? MaxUsageCount { get; set; } // Maksimum kullanım sayısı
    public int CurrentUsageCount { get; set; } = 0; // Mevcut kullanım sayısı
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

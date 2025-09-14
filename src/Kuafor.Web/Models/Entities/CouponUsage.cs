using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class CouponUsage
{
    public int Id { get; set; }
    
    public int CouponId { get; set; }
    public Coupon Coupon { get; set; } = null!;
    
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    
    public decimal DiscountAmount { get; set; }
    
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    
    public string? Notes { get; set; }
}

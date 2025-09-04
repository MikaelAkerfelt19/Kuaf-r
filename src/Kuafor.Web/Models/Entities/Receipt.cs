using System.ComponentModel.DataAnnotations;
namespace Kuafor.Web.Models.Entities;
public class Receipt
{
    public int Id { get; set; }
    [Required, StringLength(20)]
    public string ReceiptNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    [Range(0, 100000)]
    public decimal TotalAmount { get; set; }
    [Range(0, 100000)]
    public decimal PaidAmount { get; set; }
    [Range(0, 100000)]
    public decimal RemainingAmount { get; set; }
    [Required, StringLength(20)]
    public string Status { get; set; } = "Open"; // Open, Closed, Cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    
    public virtual Customer Customer { get; set; } = null!;
    public virtual ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class ReceiptItem
{
    public int Id { get; set; }
    public int ReceiptId { get; set; }
    public int? ServiceId { get; set; }
    public int? ProductId { get; set; }
    [Required, StringLength(100)]
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    [Range(0, 100000)]
    public decimal UnitPrice { get; set; }
    [Range(0, 100000)]
    public decimal TotalPrice { get; set; }
    
    public virtual Receipt Receipt { get; set; } = null!;
    public virtual Service? Service { get; set; }
    public virtual Product? Product { get; set; }
}
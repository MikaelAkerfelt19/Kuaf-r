using System.ComponentModel.DataAnnotations;
namespace Kuafor.Web.Models.Entities;
public class FinancialTransaction
{
    public int Id { get; set; }
    [Required, StringLength(50)]
    public string Type { get; set; } = string.Empty; // Income, Expense, Debt, Receivable
    [Required, StringLength(100)]
    public string Description { get; set; } = string.Empty;
    [Range(0, 100000)]
    public decimal Amount { get; set; }
    [Required, StringLength(20)]
    public string PaymentMethod { get; set; } = "Cash"; // Cash, CreditCard, Transfer
    public int? CustomerId { get; set; }
    public int? StylistId { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public virtual Customer? Customer { get; set; }
    public virtual Stylist? Stylist { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class Product
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Range(0, 100000)]
    public decimal Price { get; set; }
    
    public int StockQuantity { get; set; } = 0;
    
    public int Stock { get; set; } = 0; // Mevcut stok
    public int MinimumStock { get; set; } = 5; // Minimum stok seviyesi
    public int MaximumStock { get; set; } = 100; // Maksimum stok seviyesi
    
    [Range(0, 100)]
    public decimal CommissionPercentage { get; set; } = 0;
    
    [StringLength(50)]
    public string? Category { get; set; }
    
    [StringLength(100)]
    public string? Brand { get; set; }
    
    [StringLength(100)]
    public string? Barcode { get; set; }
    
    // Tedarikçi bilgileri
    [StringLength(100)]
    public string? Supplier { get; set; }
    
    [StringLength(50)]
    public string? SupplierCode { get; set; }
    
    public decimal? CostPrice { get; set; } // Maliyet fiyatı
    
    public DateTime? LastRestocked { get; set; } // Son stok güncelleme tarihi
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();
    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}

// Stok hareketleri için yeni entity
public class StockTransaction
{
    public int Id { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required, StringLength(20)]
    public string TransactionType { get; set; } = string.Empty; // IN, OUT, ADJUSTMENT, RETURN
    
    [Required]
    public int Quantity { get; set; }
    
    [StringLength(200)]
    public string? Reason { get; set; }
    
    [StringLength(100)]
    public string? Reference { get; set; } // Fatura no, sipariş no vb.
    
    public decimal? UnitCost { get; set; }
    
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string? CreatedBy { get; set; }
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
}
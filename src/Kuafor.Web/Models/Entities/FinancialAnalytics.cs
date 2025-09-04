using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;


// Maliyet analizi
public class CostAnalysis
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty; // Hizmet/Ürün adı
    
    [Required, StringLength(50)]
    public string Type { get; set; } = string.Empty; // Service, Product, Overhead
    
    public int? ServiceId { get; set; }
    public int? ProductId { get; set; }
    
    // Maliyet bileşenleri
    public decimal LaborCost { get; set; } = 0; // İşçilik maliyeti
    public decimal MaterialCost { get; set; } = 0; // Malzeme maliyeti
    public decimal OverheadCost { get; set; } = 0; // Genel gider maliyeti
    public decimal OtherCosts { get; set; } = 0; // Diğer maliyetler
    public decimal ServiceCost { get; set; } = 0; // Hizmet maliyeti
    public decimal ProductCost { get; set; } = 0; // Ürün maliyeti
    public decimal OtherCost { get; set; } = 0; // Diğer maliyet
    
    public decimal TotalCost { get; set; } = 0; // Toplam maliyet
    public decimal SellingPrice { get; set; } = 0; // Satış fiyatı
    public decimal ProfitMargin { get; set; } = 0; // Kar marjı
    public double ProfitPercentage { get; set; } = 0; // Kar yüzdesi
    
    // Analiz dönemi
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    
    [StringLength(100)]
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Service? Service { get; set; }
    public virtual Product? Product { get; set; }
}

// Bütçe planlama
public class Budget
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required, StringLength(20)]
    public string Type { get; set; } = string.Empty; // Monthly, Quarterly, Annual
    
    public int Year { get; set; }
    public int? Month { get; set; }
    public int? Quarter { get; set; }
    
    // Bütçe kategorileri
    public decimal RevenueBudget { get; set; } = 0; // Gelir bütçesi
    public decimal ExpenseBudget { get; set; } = 0; // Gider bütçesi
    public decimal ProfitBudget { get; set; } = 0; // Kar bütçesi
    
    // Gerçekleşen değerler
    public decimal ActualRevenue { get; set; } = 0; // Gerçekleşen gelir
    public decimal ActualExpense { get; set; } = 0; // Gerçekleşen gider
    public decimal ActualProfit { get; set; } = 0; // Gerçekleşen kar
    
    // Varyans analizi
    public decimal RevenueVariance { get; set; } = 0; // Gelir varyansı
    public decimal ExpenseVariance { get; set; } = 0; // Gider varyansı
    public decimal ProfitVariance { get; set; } = 0; // Kar varyansı
    
    public bool IsActive { get; set; } = true;
    
    [StringLength(100)]
    public string? CreatedBy { get; set; }
    
    public int? CreatedById { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<BudgetItem> BudgetItems { get; set; } = new List<BudgetItem>();
    public virtual Stylist? CreatedByUser { get; set; }
}

// Bütçe kalemleri
public class BudgetItem
{
    public int Id { get; set; }
    
    [Required]
    public int BudgetId { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required, StringLength(20)]
    public string Type { get; set; } = string.Empty; // Income, Expense
    
    public int? CategoryId { get; set; }
    
    public decimal BudgetedAmount { get; set; } = 0; // Bütçelenen tutar
    public decimal ActualAmount { get; set; } = 0; // Gerçekleşen tutar
    public decimal Variance { get; set; } = 0; // Varyans
    
    public double VariancePercentage { get; set; } = 0; // Varyans yüzdesi
    
    [StringLength(100)]
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Budget Budget { get; set; } = null!;
    public virtual FinancialCategory? Category { get; set; }
}

// Nakit akış takibi
public class CashFlow
{
    public int Id { get; set; }
    
    public DateTime Date { get; set; }
    
    // Girişler
    public decimal CashIn { get; set; } = 0; // Nakit girişi
    public decimal CardIn { get; set; } = 0; // Kart girişi
    public decimal TransferIn { get; set; } = 0; // Havale girişi
    public decimal OtherIn { get; set; } = 0; // Diğer girişler
    
    // Çıkışlar
    public decimal CashOut { get; set; } = 0; // Nakit çıkışı
    public decimal CardOut { get; set; } = 0; // Kart çıkışı
    public decimal TransferOut { get; set; } = 0; // Havale çıkışı
    public decimal OtherOut { get; set; } = 0; // Diğer çıkışlar
    
    // Net nakit akışı
    public decimal NetCashFlow { get; set; } = 0; // Net nakit akışı
    public decimal CashBalance { get; set; } = 0; // Nakit bakiyesi
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [StringLength(100)]
    public string? CreatedBy { get; set; }
    
    public int? CreatedById { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Stylist? CreatedByUser { get; set; }
}

// Finansal raporlar
public class FinancialReport
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required, StringLength(20)]
    public string ReportType { get; set; } = string.Empty; // P&L, Balance Sheet, Cash Flow, Budget
    
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    
    [Required]
    public string Data { get; set; } = string.Empty; // JSON formatında rapor verisi
    
    [StringLength(20)]
    public string Format { get; set; } = "JSON"; // JSON, PDF, Excel
    
    public long FileSize { get; set; } = 0;
    public string? FilePath { get; set; } // Dosya yolu
    
    [Required]
    public string CreatedBy { get; set; } = string.Empty;
    
    public int? CreatedById { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Stylist? CreatedByUser { get; set; }
}

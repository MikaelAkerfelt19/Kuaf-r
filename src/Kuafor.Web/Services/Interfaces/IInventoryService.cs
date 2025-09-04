using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IInventoryService
{
    // Stok işlemleri
    Task<bool> AddStockAsync(int productId, int quantity, string reason, string? reference = null);
    Task<bool> RemoveStockAsync(int productId, int quantity, string reason, string? reference = null);
    Task<bool> AdjustStockAsync(int productId, int newQuantity, string reason);
    Task<bool> ReturnStockAsync(int productId, int quantity, string reason);
    
    // Stok sorguları
    Task<int> GetCurrentStockAsync(int productId);
    Task<List<Product>> GetLowStockProductsAsync();
    Task<List<Product>> GetOutOfStockProductsAsync();
    Task<List<StockTransaction>> GetStockHistoryAsync(int productId, DateTime? from = null, DateTime? to = null);
    
    // Stok raporları
    Task<List<InventoryReport>> GetInventoryReportAsync();
    Task<List<StockMovement>> GetStockMovementsAsync(DateTime? from = null, DateTime? to = null);
    Task<decimal> GetInventoryValueAsync();
    
    // Otomatik stok güncelleme
    Task<bool> ProcessSaleAsync(int productId, int quantity, string reference);
    Task<bool> ProcessPurchaseAsync(int productId, int quantity, decimal unitCost, string reference);
}

public class InventoryReport
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalValue { get; set; }
    public string StockStatus { get; set; } = string.Empty; // Normal, Low, Out
    public DateTime? LastRestocked { get; set; }
}

public class StockMovement
{
    public DateTime Date { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public decimal? UnitCost { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

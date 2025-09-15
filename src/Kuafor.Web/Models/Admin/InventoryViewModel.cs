using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Models.Admin;

public class InventoryViewModel
{
    public List<object> Products { get; set; } = new();
    public int TotalProducts { get; set; }
    public List<object> LowStockProducts { get; set; } = new();
    public List<object> OutOfStockProducts { get; set; } = new();
    public decimal TotalInventoryValue { get; set; }
    public List<Product> RecentProducts { get; set; } = new();
    public List<StockAlert> StockAlerts { get; set; } = new();
}

public class StockAlert
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinStockLevel { get; set; }
    public string AlertType { get; set; } = string.Empty; // "Low" or "Out"
}

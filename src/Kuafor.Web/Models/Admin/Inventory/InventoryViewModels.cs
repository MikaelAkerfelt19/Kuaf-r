using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Models.Admin.Inventory
{
    public class InventoryIndexViewModel
    {
        public List<InventoryItemViewModel> Items { get; set; } = new();
        public int TotalProducts { get; set; }
        public decimal TotalValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public List<string> Categories { get; set; } = new();
        public Dictionary<int, string> Branches { get; set; } = new();
    }

    public class InventoryItemViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public int MaximumStock { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalValue { get; set; }
        public string Supplier { get; set; } = string.Empty;
        public bool IsLowStock { get; set; }
        public bool IsOutOfStock { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class InventoryReport
    {
        public int TotalProducts { get; set; }
        public decimal TotalValue { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public List<InventoryItemViewModel> LowStockItems { get; set; } = new();
        public List<InventoryItemViewModel> OutOfStockItems { get; set; } = new();
    }
}

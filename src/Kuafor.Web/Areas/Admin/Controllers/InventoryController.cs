using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/[controller]")]
public class InventoryController : Controller
{
    private readonly IInventoryService _inventoryService;
    private readonly IProductService _productService;
    private readonly IBranchService _branchService;

    public InventoryController(
        IInventoryService inventoryService,
        IProductService productService,
        IBranchService branchService)
    {
        _inventoryService = inventoryService;
        _productService = productService;
        _branchService = branchService;
    }

    // GET: /Admin/Inventory
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            // Products tablosundan inventory bilgileri al
            var products = await _productService.GetAllAsync();
            var branches = await _branchService.GetAllAsync();

            var inventoryItems = products.Select(p => new InventoryItemViewModel
            {
                Id = p.Id,
                ProductName = p.Name,
                Category = p.Category ?? "Belirtilmemiş",
                CurrentStock = p.Stock,
                MinimumStock = p.MinimumStock,
                MaximumStock = p.MaximumStock,
                UnitPrice = p.Price,
                TotalValue = p.Stock * p.Price,
                Supplier = p.Supplier ?? "Belirtilmemiş",
                IsLowStock = p.Stock <= p.MinimumStock,
                IsOutOfStock = p.Stock <= 0,
                LastUpdated = p.UpdatedAt ?? p.CreatedAt
            }).ToList();

            var viewModel = new InventoryIndexViewModel
            {
                Items = inventoryItems,
                TotalProducts = inventoryItems.Count,
                TotalValue = inventoryItems.Sum(i => i.TotalValue),
                LowStockCount = inventoryItems.Count(i => i.IsLowStock),
                OutOfStockCount = inventoryItems.Count(i => i.IsOutOfStock),
                Categories = inventoryItems.GroupBy(i => i.Category).Select(g => g.Key).ToList(),
                Branches = branches.ToDictionary(b => b.Id, b => b.Name)
            };

            return View(viewModel);
        }
        catch (Exception)
        {
            return View(new InventoryIndexViewModel());
        }
    }

    // GET: /Admin/Inventory/StockMovements
    [HttpGet]
    [Route("StockMovements")]
    public async Task<IActionResult> StockMovements()
    {
        try
        {
            var movements = await _inventoryService.GetStockMovementsAsync();
            return View(movements);
        }
        catch (Exception)
        {
            return View(new List<StockTransaction>());
        }
    }

    // POST: /Admin/Inventory/UpdateStock
    [HttpPost]
    [Route("UpdateStock")]
    public async Task<IActionResult> UpdateStock(int productId, int quantity, string movementType, string? reason = null)
    {
        try
        {
            await _inventoryService.UpdateStockAsync(productId, quantity, movementType, reason);
            return Json(new { success = true, message = "Stok başarıyla güncellendi" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // GET: /Admin/Inventory/LowStock
    [HttpGet]
    [Route("LowStock")]
    public async Task<IActionResult> LowStock()
    {
        try
        {
            var products = await _productService.GetAllAsync();
            var lowStockProducts = products
                .Where(p => p.Stock <= p.MinimumStock)
                .Select(p => new InventoryItemViewModel
                {
                    Id = p.Id,
                    ProductName = p.Name,
                    Category = p.Category ?? "Belirtilmemiş",
                    CurrentStock = p.Stock,
                    MinimumStock = p.MinimumStock,
                    UnitPrice = p.Price,
                    IsLowStock = true,
                    IsOutOfStock = p.Stock <= 0
                }).ToList();

            return View(lowStockProducts);
        }
        catch (Exception)
        {
            return View(new List<InventoryItemViewModel>());
        }
    }

    // GET: /Admin/Inventory/Reports
    [HttpGet]
    [Route("Reports")]
    public async Task<IActionResult> Reports()
    {
        try
        {
            var report = await _inventoryService.GetInventoryReportAsync();
            return View(report);
        }
        catch (Exception)
        {
            return View(new InventoryReport());
        }
    }
}

// ViewModels
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
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public List<CategorySummary> CategorySummaries { get; set; } = new();
}

public class CategorySummary
{
    public string Category { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public decimal TotalValue { get; set; }
    public int LowStockCount { get; set; }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IProductService _productService;

        public InventoryController(IInventoryService inventoryService, IProductService productService)
        {
            _inventoryService = inventoryService;
            _productService = productService;
        }

        // GET: /Admin/Inventory
        public async Task<IActionResult> Index()
        {
            var lowStockProducts = await _inventoryService.GetLowStockProductsAsync();
            var outOfStockProducts = await _inventoryService.GetOutOfStockProductsAsync();
            var inventoryReport = await _inventoryService.GetInventoryReportAsync();
            var inventoryValue = await _inventoryService.GetInventoryValueAsync();

            ViewBag.LowStockProducts = lowStockProducts;
            ViewBag.OutOfStockProducts = outOfStockProducts;
            ViewBag.InventoryReport = inventoryReport;
            ViewBag.InventoryValue = inventoryValue;

            return View();
        }

        // GET: /Admin/Inventory/StockMovements
        public async Task<IActionResult> StockMovements(DateTime? from, DateTime? to)
        {
            var movements = await _inventoryService.GetStockMovementsAsync(from, to);
            return View(movements);
        }

        // GET: /Admin/Inventory/AdjustStock/5
        public async Task<IActionResult> AdjustStock(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            ViewBag.Product = product;
            return View();
        }

        // POST: /Admin/Inventory/AdjustStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustStock(int productId, int newQuantity, string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                ModelState.AddModelError("reason", "Stok ayarlama nedeni gereklidir.");
                var product = await _productService.GetByIdAsync(productId);
                ViewBag.Product = product;
                return View();
            }

            var success = await _inventoryService.AdjustStockAsync(productId, newQuantity, reason);
            if (success)
            {
                TempData["Success"] = "Stok başarıyla güncellendi.";
            }
            else
            {
                TempData["Error"] = "Stok güncellenirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Inventory/AddStock/5
        public async Task<IActionResult> AddStock(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            ViewBag.Product = product;
            return View();
        }

        // POST: /Admin/Inventory/AddStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStock(int productId, int quantity, string reason, string? reference)
        {
            if (string.IsNullOrEmpty(reason))
            {
                ModelState.AddModelError("reason", "Stok ekleme nedeni gereklidir.");
                var product = await _productService.GetByIdAsync(productId);
                ViewBag.Product = product;
                return View();
            }

            var success = await _inventoryService.AddStockAsync(productId, quantity, reason, reference);
            if (success)
            {
                TempData["Success"] = "Stok başarıyla eklendi.";
            }
            else
            {
                TempData["Error"] = "Stok eklenirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Inventory/StockHistory/5
        public async Task<IActionResult> StockHistory(int id, DateTime? from, DateTime? to)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            var history = await _inventoryService.GetStockHistoryAsync(id, from, to);
            
            ViewBag.Product = product;
            return View(history);
        }

        // API: Stok uyarıları
        [HttpGet]
        public async Task<IActionResult> GetStockAlerts()
        {
            var lowStock = await _inventoryService.GetLowStockProductsAsync();
            var outOfStock = await _inventoryService.GetOutOfStockProductsAsync();

            return Json(new
            {
                lowStock = lowStock.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    currentStock = p.Stock,
                    minimumStock = p.MinimumStock,
                    alertLevel = "warning"
                }),
                outOfStock = outOfStock.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    currentStock = p.Stock,
                    minimumStock = p.MinimumStock,
                    alertLevel = "danger"
                })
            });
        }
    }
}



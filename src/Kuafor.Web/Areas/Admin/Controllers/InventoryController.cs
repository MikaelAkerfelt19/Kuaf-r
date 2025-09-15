using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Admin;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/[controller]")]
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
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var products = await _productService.GetAllAsync();
            var inventoryViewModel = new InventoryViewModel
            {
                Products = products.Cast<object>().ToList(),
                TotalProducts = products.Count(),
                LowStockProducts = products.Where(p => p.StockQuantity <= p.MinimumStock).Cast<object>().ToList(),
                OutOfStockProducts = products.Where(p => p.StockQuantity == 0).Cast<object>().ToList()
            };

            return View(inventoryViewModel);
        }
        catch (Exception)
        {
            var emptyInventory = new InventoryViewModel
            {
                Products = new List<object>(),
                TotalProducts = 0,
                LowStockProducts = new List<object>(),
                OutOfStockProducts = new List<object>()
            };
            return View(emptyInventory);
        }
    }

    // GET: /Admin/Inventory/Details/5
    [HttpGet]
    [Route("Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    // GET: /Admin/Inventory/Create
    [HttpGet]
    [Route("Create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Admin/Inventory/Create
    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        try
        {
            if (ModelState.IsValid)
            {
                await _productService.CreateAsync(product);
                TempData["Success"] = "Ürün başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        catch (Exception)
        {
            TempData["Error"] = "Ürün eklenirken bir hata oluştu.";
            return View(product);
        }
    }

    // GET: /Admin/Inventory/Edit/5
    [HttpGet]
    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    // POST: /Admin/Inventory/Edit/5
    [HttpPost]
    [Route("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        try
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _productService.UpdateAsync(product);
                TempData["Success"] = "Ürün başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        catch (Exception)
        {
            TempData["Error"] = "Ürün güncellenirken bir hata oluştu.";
            return View(product);
        }
    }

    // GET: /Admin/Inventory/Delete/5
    [HttpGet]
    [Route("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    // POST: /Admin/Inventory/Delete/5
    [HttpPost]
    [Route("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _productService.DeleteAsync(id);
            TempData["Success"] = "Ürün başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            TempData["Error"] = "Ürün silinirken bir hata oluştu.";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: /Admin/Inventory/UpdateStock
    [HttpPost]
    [Route("UpdateStock")]
    public async Task<IActionResult> UpdateStock(int productId, int newQuantity)
    {
        try
        {
            // UpdateStockAsync method'u IInventoryService'de yok, bu yüzden ProductService kullanıyoruz
            var product = await _productService.GetByIdAsync(productId);
            if (product != null)
            {
                product.StockQuantity = newQuantity;
                await _productService.UpdateAsync(product);
                return Json(new { success = true, message = "Stok güncellendi." });
            }
            return Json(new { success = false, message = "Ürün bulunamadı." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
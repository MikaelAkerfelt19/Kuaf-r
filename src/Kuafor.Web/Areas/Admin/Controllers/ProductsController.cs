using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly IBranchService _branchService;

        public ProductsController(IProductService productService, IBranchService branchService)
        {
            _productService = productService;
            _branchService = branchService;
        }

        // GET: /Admin/Products
        [HttpGet]
        [Route("Product")]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
        }

        // GET: /Admin/Products/Create
        [HttpGet]
        [Route("Create")]
        public async Task<IActionResult> Create()
        {
            var branches = await _branchService.GetAllAsync();
            ViewBag.Branches = branches;
            return View(new Product());
        }

        // POST: /Admin/Products/Create
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.UtcNow;
                await _productService.CreateAsync(product);
                TempData["Success"] = "Ürün başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            
            // Hata durumunda branch listesini tekrar yükle
            var branches = await _branchService.GetAllAsync();
            ViewBag.Branches = branches;
            return View(product);
        }

        // GET: /Admin/Products/Edit/5
        [HttpGet]
        [Route("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: /Admin/Products/Edit/5
        [HttpPost]
        [Route("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _productService.UpdateAsync(product);
                    TempData["Success"] = "Ürün başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    TempData["Error"] = "Ürün güncellenirken bir hata oluştu.";
                }
            }
            return View(product);
        }

        // GET: /Admin/Products/Delete/5
        [HttpGet]
        [Route("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: /Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [Route("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (result)
            {
                TempData["Success"] = "Ürün başarıyla silindi.";
            }
            else
            {
                TempData["Error"] = "Ürün silinirken bir hata oluştu.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

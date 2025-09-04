using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class InventoryService : IInventoryService
{
    private readonly ApplicationDbContext _context;

    public InventoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddStockAsync(int productId, int quantity, string reason, string? reference = null)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.Stock += quantity;
            product.LastRestocked = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            var transaction = new StockTransaction
            {
                ProductId = productId,
                TransactionType = "IN",
                Quantity = quantity,
                Reason = reason,
                Reference = reference,
                TransactionDate = DateTime.UtcNow
            };

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveStockAsync(int productId, int quantity, string reason, string? reference = null)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.Stock < quantity) return false;

            product.Stock -= quantity;
            product.UpdatedAt = DateTime.UtcNow;

            var transaction = new StockTransaction
            {
                ProductId = productId,
                TransactionType = "OUT",
                Quantity = -quantity,
                Reason = reason,
                Reference = reference,
                TransactionDate = DateTime.UtcNow
            };

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> AdjustStockAsync(int productId, int newQuantity, string reason)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            var difference = newQuantity - product.Stock;
            product.Stock = newQuantity;
            product.UpdatedAt = DateTime.UtcNow;

            var transaction = new StockTransaction
            {
                ProductId = productId,
                TransactionType = "ADJUSTMENT",
                Quantity = difference,
                Reason = reason,
                TransactionDate = DateTime.UtcNow
            };

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ReturnStockAsync(int productId, int quantity, string reason)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.Stock += quantity;
            product.UpdatedAt = DateTime.UtcNow;

            var transaction = new StockTransaction
            {
                ProductId = productId,
                TransactionType = "RETURN",
                Quantity = quantity,
                Reason = reason,
                TransactionDate = DateTime.UtcNow
            };

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<int> GetCurrentStockAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        return product?.Stock ?? 0;
    }

    public async Task<List<Product>> GetLowStockProductsAsync()
    {
        return await _context.Products
            .Where(p => p.IsActive && p.Stock <= p.MinimumStock && p.Stock > 0)
            .OrderBy(p => p.Stock)
            .ToListAsync();
    }

    public async Task<List<Product>> GetOutOfStockProductsAsync()
    {
        return await _context.Products
            .Where(p => p.IsActive && p.Stock == 0)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<List<StockTransaction>> GetStockHistoryAsync(int productId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.StockTransactions
            .Where(st => st.ProductId == productId);

        if (from.HasValue)
            query = query.Where(st => st.TransactionDate >= from.Value);

        if (to.HasValue)
            query = query.Where(st => st.TransactionDate <= to.Value);

        return await query
            .OrderByDescending(st => st.TransactionDate)
            .ToListAsync();
    }

    public async Task<List<InventoryReport>> GetInventoryReportAsync()
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .ToListAsync();

        return products.Select(p => new InventoryReport
        {
            ProductId = p.Id,
            ProductName = p.Name,
            Category = p.Category ?? "Genel",
            CurrentStock = p.Stock,
            MinimumStock = p.MinimumStock,
            MaximumStock = p.MaximumStock,
            UnitCost = p.CostPrice ?? 0,
            TotalValue = p.Stock * (p.CostPrice ?? 0),
            StockStatus = p.Stock == 0 ? "Out" : p.Stock <= p.MinimumStock ? "Low" : "Normal",
            LastRestocked = p.LastRestocked
        }).ToList();
    }

    public async Task<List<StockMovement>> GetStockMovementsAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.StockTransactions
            .Include(st => st.Product)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(st => st.TransactionDate >= from.Value);

        if (to.HasValue)
            query = query.Where(st => st.TransactionDate <= to.Value);

        var transactions = await query
            .OrderByDescending(st => st.TransactionDate)
            .ToListAsync();

        return transactions.Select(t => new StockMovement
        {
            Date = t.TransactionDate,
            ProductName = t.Product.Name,
            TransactionType = t.TransactionType,
            Quantity = t.Quantity,
            Reason = t.Reason ?? "",
            Reference = t.Reference ?? "",
            UnitCost = t.UnitCost,
            CreatedBy = t.CreatedBy ?? ""
        }).ToList();
    }

    public async Task<decimal> GetInventoryValueAsync()
    {
        var products = await _context.Products
            .Where(p => p.IsActive)
            .ToListAsync();

        return products.Sum(p => p.Stock * (p.CostPrice ?? 0));
    }

    public async Task<bool> ProcessSaleAsync(int productId, int quantity, string reference)
    {
        return await RemoveStockAsync(productId, quantity, "Satış", reference);
    }

    public async Task<bool> ProcessPurchaseAsync(int productId, int quantity, decimal unitCost, string reference)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.Stock += quantity;
            product.CostPrice = unitCost;
            product.LastRestocked = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            var transaction = new StockTransaction
            {
                ProductId = productId,
                TransactionType = "IN",
                Quantity = quantity,
                Reason = "Satın alma",
                Reference = reference,
                UnitCost = unitCost,
                TransactionDate = DateTime.UtcNow
            };

            _context.StockTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}



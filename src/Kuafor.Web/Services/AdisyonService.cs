using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class AdisyonService : IAdisyonService
{
    private readonly ApplicationDbContext _context;

    public AdisyonService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Receipt>> GetAllAsync()
    {
        return await _context.Receipts
            .Include(r => r.Customer)
            .Include(r => r.ReceiptItems)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Receipt?> GetByIdAsync(int id)
    {
        return await _context.Receipts
            .Include(r => r.Customer)
            .Include(r => r.ReceiptItems)
            .ThenInclude(ri => ri.Service)
            .Include(r => r.ReceiptItems)
            .ThenInclude(ri => ri.Product)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Receipt> CreateAsync(Receipt receipt)
    {
        receipt.ReceiptNumber = GenerateReceiptNumber();
        receipt.CreatedAt = DateTime.UtcNow;
        receipt.Status = "Open";
        
        _context.Receipts.Add(receipt);
        await _context.SaveChangesAsync();
        return receipt;
    }

    public async Task<Receipt> UpdateAsync(Receipt receipt)
    {
        _context.Receipts.Update(receipt);
        await _context.SaveChangesAsync();
        return receipt;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var receipt = await _context.Receipts.FindAsync(id);
        if (receipt == null) return false;

        _context.Receipts.Remove(receipt);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Receipt> CreateForRegisteredCustomerAsync(int customerId, List<ReceiptItem> items)
    {
        var totalAmount = await CalculateTotalAsync(items);
        
        var receipt = new Receipt
        {
            CustomerId = customerId,
            TotalAmount = totalAmount,
            PaidAmount = 0,
            RemainingAmount = totalAmount,
            Status = "Open",
            ReceiptItems = items
        };

        return await CreateAsync(receipt);
    }

    public async Task<Receipt> CreateForNewCustomerAsync(Customer customer, List<ReceiptItem> items)
    {
        // Önce müşteriyi kaydet
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var totalAmount = await CalculateTotalAsync(items);
        
        var receipt = new Receipt
        {
            CustomerId = customer.Id,
            TotalAmount = totalAmount,
            PaidAmount = 0,
            RemainingAmount = totalAmount,
            Status = "Open",
            ReceiptItems = items
        };

        return await CreateAsync(receipt);
    }

    public async Task<bool> CloseReceiptAsync(int receiptId)
    {
        var receipt = await _context.Receipts.FindAsync(receiptId);
        if (receipt == null) return false;

        receipt.Status = "Closed";
        receipt.ClosedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<decimal> CalculateTotalAsync(List<ReceiptItem> items)
    {
        return Task.FromResult(items.Sum(item => item.TotalPrice));
    }

    private string GenerateReceiptNumber()
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        return $"AD{date}{random}";
    }
}

using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Admin.Adisyon;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class AdisyonService : IAdisyonService
{
    private readonly ApplicationDbContext _context;

    public AdisyonService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Adisyon metotları
    public async Task<IEnumerable<Adisyon>> GetAllAsync()
    {
        return await _context.Adisyons
            .Include(a => a.Customer)
            .Include(a => a.AdisyonDetails)
            .ThenInclude(ad => ad.Service)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Adisyon?> GetByIdAsync(int id)
    {
        return await _context.Adisyons
            .Include(a => a.Customer)
            .Include(a => a.AdisyonDetails)
            .ThenInclude(ad => ad.Service)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Adisyon> CreateAsync(Adisyon adisyon)
    {
        adisyon.CreatedAt = DateTime.UtcNow;
        adisyon.IsActive = true;
        
        _context.Adisyons.Add(adisyon);
        await _context.SaveChangesAsync();
        return adisyon;
    }

    public async Task<Adisyon> UpdateAsync(Adisyon adisyon)
    {
        adisyon.UpdatedAt = DateTime.UtcNow;
        _context.Adisyons.Update(adisyon);
        await _context.SaveChangesAsync();
        return adisyon;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var adisyon = await _context.Adisyons.FindAsync(id);
        if (adisyon == null) return false;

        adisyon.IsActive = false;
        adisyon.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddServicesToAdisyonAsync(int adisyonId, List<AdisyonServiceModel> services)
    {
        try
        {
            var adisyonDetails = services.Select(s => new AdisyonDetail
            {
                AdisyonId = adisyonId,
                ServiceId = s.ServiceId,
                Quantity = s.Quantity,
                UnitPrice = s.UnitPrice,
                TotalPrice = s.TotalPrice
            }).ToList();

            _context.AdisyonDetails.AddRange(adisyonDetails);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<AdisyonDetail>> GetAdisyonDetailsAsync(int adisyonId)
    {
        return await _context.AdisyonDetails
            .Include(ad => ad.Service)
            .Where(ad => ad.AdisyonId == adisyonId)
            .ToListAsync();
    }

    public Task<decimal> CalculateTotalAsync(List<AdisyonServiceModel> services)
    {
        return Task.FromResult(services.Sum(s => s.TotalPrice));
    }

    // Receipt metotları (eski implementasyon)
    public async Task<IEnumerable<Receipt>> GetAllReceiptsAsync()
    {
        return await _context.Receipts
            .Include(r => r.Customer)
            .Include(r => r.ReceiptItems)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Receipt?> GetReceiptByIdAsync(int id)
    {
        return await _context.Receipts
            .Include(r => r.Customer)
            .Include(r => r.ReceiptItems)
            .ThenInclude(ri => ri.Service)
            .Include(r => r.ReceiptItems)
            .ThenInclude(ri => ri.Product)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Receipt> CreateReceiptAsync(Receipt receipt)
    {
        receipt.ReceiptNumber = GenerateReceiptNumber();
        receipt.CreatedAt = DateTime.UtcNow;
        receipt.Status = "Open";
        
        _context.Receipts.Add(receipt);
        await _context.SaveChangesAsync();
        return receipt;
    }

    public async Task<Receipt> UpdateReceiptAsync(Receipt receipt)
    {
        _context.Receipts.Update(receipt);
        await _context.SaveChangesAsync();
        return receipt;
    }

    public async Task<bool> DeleteReceiptAsync(int id)
    {
        var receipt = await _context.Receipts.FindAsync(id);
        if (receipt == null) return false;

        _context.Receipts.Remove(receipt);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Receipt> CreateForRegisteredCustomerAsync(int customerId, List<ReceiptItem> items)
    {
        var totalAmount = await CalculateReceiptTotalAsync(items);
        
        var receipt = new Receipt
        {
            CustomerId = customerId,
            TotalAmount = totalAmount,
            PaidAmount = 0,
            RemainingAmount = totalAmount,
            Status = "Open",
            ReceiptItems = items
        };

        return await CreateReceiptAsync(receipt);
    }

    public async Task<Receipt> CreateForNewCustomerAsync(Customer customer, List<ReceiptItem> items)
    {
        // Önce müşteriyi kaydet
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var totalAmount = await CalculateReceiptTotalAsync(items);
        
        var receipt = new Receipt
        {
            CustomerId = customer.Id,
            TotalAmount = totalAmount,
            PaidAmount = 0,
            RemainingAmount = totalAmount,
            Status = "Open",
            ReceiptItems = items
        };

        return await CreateReceiptAsync(receipt);
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

    public Task<decimal> CalculateReceiptTotalAsync(List<ReceiptItem> items)
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

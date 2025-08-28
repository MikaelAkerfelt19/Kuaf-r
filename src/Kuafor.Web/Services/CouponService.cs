using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class CouponService : ICouponService
{
    private readonly ApplicationDbContext _context;
    
    public CouponService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Coupon>> GetAllAsync()
    {
        return await _context.Coupons
            .Where(c => c.IsActive)
            .OrderBy(c => c.Title)
            .ToListAsync();
    }

    public async Task<IEnumerable<Coupon>> GetActiveForCustomerAsync(int customerId)
    {
        return await _context.Coupons
            .Where(c => c.IsActive && 
                       (c.ExpiresAt == null || c.ExpiresAt > DateTime.UtcNow) &&
                       (c.MaxUsageCount == null || c.CurrentUsageCount < c.MaxUsageCount))
            .OrderBy(c => c.ExpiresAt)
            .ToListAsync();
    }
    
    public async Task<Coupon?> GetByIdAsync(int id)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
    }
    
    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);
    }
    
    public async Task<Coupon> CreateAsync(Coupon coupon)
    {
        coupon.CreatedAt = DateTime.UtcNow;
        coupon.IsActive = true;
        coupon.CurrentUsageCount = 0;
        
        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();
        
        return coupon;
    }
    
    public async Task<Coupon> UpdateAsync(Coupon coupon)
    {
        var existing = await _context.Coupons.FindAsync(coupon.Id);
        if (existing == null)
            throw new InvalidOperationException("Coupon not found");
            
        existing.Title = coupon.Title;
        existing.DiscountType = coupon.DiscountType;
        existing.Amount = coupon.Amount;
        existing.MinSpend = coupon.MinSpend;
        existing.ExpiresAt = coupon.ExpiresAt;
        existing.MaxUsageCount = coupon.MaxUsageCount;
        existing.IsActive = coupon.IsActive;
        
        await _context.SaveChangesAsync();
        
        return existing;
    }
    
    public async Task DeleteAsync(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon != null)
        {
            coupon.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Coupons.AnyAsync(c => c.Id == id && c.IsActive);
    }
    
    public async Task<CouponValidationResult> ValidateAsync(string code, decimal basketTotal, int? customerId = null)
    {
        var coupon = await GetByCodeAsync(code);
        if (coupon == null)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                Reason = "Kupon bulunamadı"
            };
        }
        
        // Kupon aktif mi?
        if (!coupon.IsActive)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                Reason = "Kupon aktif değil"
            };
        }
        
        // Kupon süresi dolmuş mu?
        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt.Value < DateTime.UtcNow)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                Reason = "Kupon süresi dolmuş"
            };
        }
        
        // Minimum harcama tutarı kontrolü
        if (coupon.MinSpend.HasValue && basketTotal < coupon.MinSpend.Value)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                Reason = $"Minimum {coupon.MinSpend:C} harcama gerekli"
            };
        }
        
        // Maksimum kullanım sayısı kontrolü
        if (coupon.MaxUsageCount.HasValue && coupon.CurrentUsageCount >= coupon.MaxUsageCount.Value)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                Reason = "Kupon kullanım limiti dolmuş"
            };
        }
        
        // İndirim hesaplama
        decimal discountAmount = 0;
        if (coupon.DiscountType == "Percent")
        {
            discountAmount = basketTotal * (coupon.Amount / 100);
        }
        else
        {
            discountAmount = coupon.Amount;
        }
        
        return new CouponValidationResult
        {
            IsValid = true,
            Discount = new CouponDiscount
            {
                Type = coupon.DiscountType,
                Amount = discountAmount
            }
        };
    }

    public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
    {
        if (excludeId.HasValue)
        {
            return await _context.Coupons.AnyAsync(c => c.Code == code && c.Id != excludeId);
        }
        return await _context.Coupons.AnyAsync(c => c.Code == code);
    }



    public async Task<IEnumerable<Coupon>> GetActiveAsync()
    {
        return await _context.Coupons
            .Where(c => c.IsActive && (c.ExpiresAt == null || c.ExpiresAt > DateTime.UtcNow))
            .OrderBy(c => c.Code)
            .ToListAsync();
    }
}
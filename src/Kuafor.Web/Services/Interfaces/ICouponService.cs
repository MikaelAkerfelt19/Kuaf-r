using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface ICouponService
{
    Task<IEnumerable<Coupon>> GetAllAsync();
    Task<IEnumerable<Coupon>> GetActiveForCustomerAsync(int customerId);
    Task<Coupon?> GetByIdAsync(int id);
    Task<Coupon> CreateAsync(Coupon coupon);
    Task<Coupon> UpdateAsync(Coupon coupon);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
    Task<Coupon?> GetByCodeAsync(string code);
    Task<IEnumerable<Coupon>> GetActiveAsync();
    Task<CouponValidationResult> ValidateAsync(string code, decimal basketTotal, int? customerId = null);
}

public class CouponValidationResult
{
    public bool IsValid { get; set; }
    public string? Reason { get; set; }
    public CouponDiscount? Discount { get; set; }
}

public class CouponDiscount
{
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Controllers.Api.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CouponApiController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponApiController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateCoupon([FromBody] CouponValidationApiRequest request)
        {
            try
            {
                var result = await _couponService.ValidateAsync(
                    request.Code, 
                    request.BasketTotal, 
                    request.CustomerId
                );

                return Ok(new
                {
                    success = result.IsValid,
                    message = result.IsValid ? "Kupon geçerli" : result.Reason,
                    discount = result.Discount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Kupon doğrulama sırasında hata oluştu: " + ex.Message
                });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCoupons([FromQuery] int? customerId = null)
        {
            try
            {
                var coupons = customerId.HasValue 
                    ? await _couponService.GetActiveForCustomerAsync(customerId.Value)
                    : await _couponService.GetActiveAsync();

                var result = coupons.Select(c => new
                {
                    id = c.Id,
                    code = c.Code,
                    title = c.Title,
                    discountType = c.DiscountType,
                    amount = c.Amount,
                    minSpend = c.MinSpend,
                    expiresAt = c.ExpiresAt,
                    isActive = c.IsActive
                });

                return Ok(new
                {
                    success = true,
                    coupons = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Kuponlar getirilirken hata oluştu: " + ex.Message
                });
            }
        }

        [HttpPost("apply")]
        public async Task<IActionResult> ApplyCoupon([FromBody] CouponApplyApiRequest request)
        {
            try
            {
                // Önce kuponu doğrula
                var validation = await _couponService.ValidateAsync(
                    request.Code, 
                    request.BasketTotal, 
                    request.CustomerId
                );

                if (!validation.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = validation.Reason
                    });
                }

                // Kuponu uygula
                var coupon = await _couponService.GetByCodeAsync(request.Code);
                if (coupon == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Kupon bulunamadı"
                    });
                }

                var usage = await _couponService.ApplyCouponAsync(
                    coupon.Id,
                    request.CustomerId,
                    request.AppointmentId,
                    validation.Discount!.Amount,
                    request.Notes
                );

                return Ok(new
                {
                    success = true,
                    message = "Kupon başarıyla uygulandı",
                    discount = validation.Discount,
                    usageId = usage.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Kupon uygulanırken hata oluştu: " + ex.Message
                });
            }
        }

        [HttpGet("customer/{customerId}/usage")]
        public async Task<IActionResult> GetCustomerCouponUsages(int customerId)
        {
            try
            {
                var usages = await _couponService.GetCustomerCouponUsagesAsync(customerId);

                var result = usages.Select(u => new
                {
                    id = u.Id,
                    couponCode = u.Coupon.Code,
                    couponTitle = u.Coupon.Title,
                    discountAmount = u.DiscountAmount,
                    usedAt = u.UsedAt,
                    appointmentId = u.AppointmentId,
                    notes = u.Notes
                });

                return Ok(new
                {
                    success = true,
                    usages = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Kupon kullanımları getirilirken hata oluştu: " + ex.Message
                });
            }
        }
    }

    public class CouponValidationApiRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal BasketTotal { get; set; }
        public int? CustomerId { get; set; }
    }

    public class CouponApplyApiRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal BasketTotal { get; set; }
        public int CustomerId { get; set; }
        public int AppointmentId { get; set; }
        public string? Notes { get; set; }
    }
}

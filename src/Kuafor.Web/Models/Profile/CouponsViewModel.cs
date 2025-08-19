using System;
using System.Collections.Generic;

namespace Kuafor.Web.Models.Profile
{
    public record CouponVm(
        string Code,
        string Title,
        string? Description,
        DateTime? ExpiresAt,
        decimal? MinSpend,
        string DiscountText, // "%10", "₺50", vb.
        bool IsUsed          // Kullanılmış ise true
    );

    public class CouponsViewModel
    {
        public List<CouponVm> Active { get; set; } = new();
        public List<CouponVm> Expired { get; set; } = new();
    }
}
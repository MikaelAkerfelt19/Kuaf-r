using System.Collections.Generic;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Models.Profile
{
    public class CouponsViewModel
    {
        public IEnumerable<CouponUsage> UsedCoupons { get; set; } = new List<CouponUsage>();
        public IEnumerable<Coupon> AvailableCoupons { get; set; } = new List<Coupon>();
        public decimal TotalSavings { get; set; } = 0;
        public int CouponsUsedThisMonth { get; set; } = 0;
    }
}
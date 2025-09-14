using System.Collections.Generic;
using Kuafor.Web.Models.Appointments;

namespace Kuafor.Web.Models.Customer
{
    public class CouponsViewModel
    {
        public List<CouponVm> Active { get; set; } = new();
        public List<CouponVm> Expired { get; set; } = new();
    }
}

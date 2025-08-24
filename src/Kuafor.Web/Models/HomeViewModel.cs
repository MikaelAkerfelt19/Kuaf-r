using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Models
{
    public class HomeViewModel
    {
        public bool IsAuthenticated { get; set; }
        public IEnumerable<Service> Services { get; set; } = new List<Service>();
        public IEnumerable<Testimonial> Testimonials { get; set; } = new List<Testimonial>();
        public IEnumerable<Branch> Branches { get; set; } = new List<Branch>();
    }
}
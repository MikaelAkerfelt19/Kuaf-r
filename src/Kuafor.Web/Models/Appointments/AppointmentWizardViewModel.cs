using System;
using System.Collections.Generic;
using Kuafor.Web.Models.Profile; 

namespace Kuafor.Web.Models.Appointments
{
    public enum WizardStep { Service = 1, Stylist = 2, Time = 3, Confirm = 4 }

    public record ServiceVm(int Id, string Name, string DurationText, string Description);
    public record StylistVm(int Id, string Name, double Rating, string Bio, int BranchId);
    public record TimeSlotVm(DateTime Start, bool IsAvailable)
    {
        public DateTime StartUtc => Start.ToUniversalTime();
        public DateTime StartLocal => Start;
    }

    public class AppointmentWizardViewModel
    {
        public WizardStep Step { get; set; } = WizardStep.Service;
        public int? SelectedServiceId { get; set; }
        public int? SelectedStylistId { get; set; }
        public DateTime? SelectedStart { get; set; }

        public List<ServiceVm> Services { get; set; } = new();
        public List<StylistVm> Stylists { get; set; } = new();
        public List<TimeSlotVm> TimeSlots { get; set; } = new();

        // Özet için kolaylık
        public ServiceVm? SelectedService => Services.Find(s => s.Id == SelectedServiceId);
        public StylistVm? SelectedStylist => Stylists.Find(s => s.Id == SelectedStylistId);

        // Kupon özellikleri
        public string? SelectedCouponCode { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public List<CouponVm> AvailableCoupons { get; set; } = new();
    }
}
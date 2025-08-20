using System;
using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Profile
{
    public class ProfilePageViewModel
    {
        public IdentityViewModel Identity { get; set; } = new();
        public AddressesViewModel Addresses { get; set; } = new();
        public NotificationsViewModel Notifications { get; set; } = new();
        public PreferencesViewModel Preferences { get; set; } = new();
        public LoyaltyViewModel Loyalty { get; set; } = new();
        public CouponsViewModel Coupons { get; set; } = new();
        public SecurityViewModel Security { get; set; } = new();
        public PaymentsViewModel Payments { get; set; } = new();
        public PrivacyViewModel Privacy { get; set; } = new();
    }
}
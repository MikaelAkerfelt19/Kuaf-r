using System;
using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Profile
{
    public class ProfilePageViewModel
    {
        public IdentityViewModel      Identity      { get; set; } = new();
        public AddressesViewModel     Addresses     { get; set; } = new();
        public NotificationsViewModel Notifications { get; set; } = new();
        public PreferencesViewModel   Preferences   { get; set; } = new();
    }
}
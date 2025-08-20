using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Profile
{
    public class ChangePasswordModel
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;
        [Required, MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
        [Required, Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public record ActiveSessionVm(string Id, string Device, string Browser, DateTime LastAccess, bool IsCurrent);

    public class TwoFactorSetupModel
    {
        public bool IsEnabled { get; set; }
        public string? SecretKey { get; set; } // MOCK: "JBSWY3DPEHPK3PXP"
        public string? OtpauthUri { get; set; } // MOCK: otpauth://totp/xxx-hairdresser:...
        public List<string> RecoveryCodes { get; set; } = new(); // MOCK 8-10 kod
    }

    public class SecurityViewModel
    {
        public ChangePasswordModel ChangePassword { get; set; } = new();
        public TwoFactorSetupModel TwoFactor { get; set; } = new();
        public List<ActiveSessionVm> ActiveSessions { get; set; } = new();
    }
}
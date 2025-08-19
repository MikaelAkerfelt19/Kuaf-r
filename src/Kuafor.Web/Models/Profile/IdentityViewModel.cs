using System;
using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Profile
{
    public class IdentityViewModel
    {
        [Required, StringLength(80)] public string FullName { get; set; } = string.Empty;
        [StringLength(40)] public string? DisplayName { get; set; }
        public string? Gender { get; set; }
        [DataType(DataType.Date)] public DateTime? BirthDate { get; set; }

        [EmailAddress] public string Email { get; set; } = string.Empty;
        [Phone] public string? Phone { get; set; }
        public bool WhatsappOptIn { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
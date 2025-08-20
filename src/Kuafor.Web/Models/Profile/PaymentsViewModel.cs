using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Profile
{
    public record SavedCardVm(
        int Id,
        string Mask, // 1234-****-****-5678
        string HolderName, // John Doe
        int ExpMonth, // 1..12
        int ExpYear, // 2025
        bool IsDefault
    );

    public class BillingInfoViewModel
    {
        // Bireysel / Şirket
        [Required] public string InvoiceType { get; set; } = "Bireysel";

        // Bireysel: Ad Soyad, Şirket: Ünvan
        [Required, StringLength(120)] public string FullNameOrCompany { get; set; } = string.Empty;

        // Bireysel: TC Kimlik No, Şirket: VKN
        [StringLength(11)] public string? TaxNumber { get; set; }

        [Required, StringLength(160)] public string AddressLine1 { get; set; } = string.Empty;
        [Required, StringLength(60)] public string City { get; set; } = string.Empty;
        [Required, StringLength(60)] public string District { get; set; } = string.Empty;
        [StringLength(10)] public string? Zip { get; set; }

        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        public bool EInvoiceOptIn { get; set; } = false;
    };

    public class PaymentsViewModel
    {
        public List<SavedCardVm> Cards { get; set; } = new();
        public BillingInfoViewModel Billing { get; set; } = new();
    }
}
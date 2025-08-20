using System;

namespace Kuafor.Web.Models.Profile
{
    public class PrivacyViewModel
    {
        // Pazarlama izinleri (Açık rıza)
        public bool ConsentEmail { get; set; } = true;
        public bool ConsentSms { get; set; } = false;
        public bool ConsentPush { get; set; } = false;
        public bool ConsentWhatsApp { get; set; } = false;

        // Bilgilendirme
        public DateTime? ConsentedAt { get; set; }
        public string? ConsentedIp { get; set; }

        // İndirme durumu (mock)
        public bool ExportReady { get; set; } = true;
        public string? LastExportFileName { get; set; }
    }
}
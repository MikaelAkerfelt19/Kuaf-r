using System;

namespace Kuafor.Web.Models.Profile
{
    public class NotificationsViewModel
    {
        // Kanallar
        public bool Email { get; set; } = true;
        public bool Sms { get; set; } = true;
        public bool Push { get; set; } = true;
        public bool WhatsApp { get; set; } = true;

        // Kategoriler
        public bool Reminders { get; set; } = true; // Randevu hatırlatma
        public bool Campaigns { get; set; } = true; // Kampanya bildirimleri
        public bool Critical { get; set; } = true; // Önemli duyurular

        // Sessiz saat aralığı
        public TimeSpan QuietFrom { get; set; } = new(22, 0, 0);
        public TimeSpan QuietTo { get; set; } = new(8, 0, 0);
    }
}
using System;

namespace Kuafor.Web.Models.Account
{
    public class ResetSession
    {
        public string Sid { get; set; } = Guid.NewGuid().ToString("N");
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool Verified { get; set; } = false;
        public DateTime ExpiresAtUtc { get; set; }

        // 60 sn yeniden gönderim limiti için GEREKLİ
        public DateTime NextSendAllowedUtc { get; set; } = DateTime.MinValue;
    }
}

using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Kuafor.Web.Services
{
    public class TwilioSmsSender : ISmsService
    {
        private readonly IConfiguration _config;

        public TwilioSmsSender(IConfiguration config)
        {
            _config = config;
            TwilioClient.Init(_config["Twilio:AccountSid"], _config["Twilio:AuthToken"]);
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(message))
                    return false;

                var from = _config["Twilio:From"];
                await MessageResource.CreateAsync(
                    body: message,
                    from: new Twilio.Types.PhoneNumber(from),
                    to: new Twilio.Types.PhoneNumber(phoneNumber)
                );
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> SendAppointmentConfirmationAsync(Appointment appointment)
        {
            var name = appointment.Customer?.Name ?? "müşterimiz";
            var when = appointment.StartAt.ToString("dd.MM.yyyy HH:mm");
            var phone = appointment.Customer?.PhoneNumber ?? "";
            var msg = $"Merhaba {name}, {when} tarihli randevunuz onaylanmıştır.";
            return await SendSmsAsync(phone, msg);
        }

        public async Task<bool> SendAppointmentReminderAsync(Appointment appointment)
        {
            var name = appointment.Customer?.Name ?? "müşterimiz";
            var when = appointment.StartAt.ToString("dd.MM.yyyy HH:mm");
            var phone = appointment.Customer?.PhoneNumber ?? "";
            var msg = $"Hatırlatma: {when} tarihli randevunuz yaklaşıyor. Görüşmek üzere, {name}!";
            return await SendSmsAsync(phone, msg);
        }

        public async Task<bool> SendAppointmentCancellationAsync(Appointment appointment)
        {
            var name = appointment.Customer?.Name ?? "müşterimiz";
            var when = appointment.StartAt.ToString("dd.MM.yyyy HH:mm");
            var phone = appointment.Customer?.PhoneNumber ?? "";
            var msg = $"Üzgünüz {name}, {when} tarihli randevunuz iptal edilmiştir.";
            return await SendSmsAsync(phone, msg);
        }

        public async Task<bool> SendAppointmentRescheduleAsync(Appointment appointment, DateTime newDateTime)
        {
            var name = appointment.Customer?.Name ?? "müşterimiz";
            var oldWhen = appointment.StartAt.ToString("dd.MM.yyyy HH:mm");
            var newWhen = newDateTime.ToString("dd.MM.yyyy HH:mm");
            var phone = appointment.Customer?.PhoneNumber ?? "";
            var msg = $"Bilgi: Randevunuz {oldWhen} tarihinden {newWhen} tarihine ertelendi, {name}.";
            return await SendSmsAsync(phone, msg);
        }

        // >>> Eksik olan ikisi:
        public async Task<bool> SendWelcomeMessageAsync(Customer customer)
        {
            var name = customer?.Name ?? "müşterimiz";
            var phone = customer?.PhoneNumber ?? "";
            if (string.IsNullOrWhiteSpace(phone)) return false;

            var msg = $"Hoş geldiniz {name}! Randevularınızı artık kolayca oluşturabilirsiniz.";
            return await SendSmsAsync(phone, msg);
        }

        public async Task<bool> SendPromotionalMessageAsync(string phoneNumber, string message)
        {
            // Burada direkt kullanıcının girdisi gönderiliyor -> isterseniz whitelist/keyword kontrolü ekleyin
            return await SendSmsAsync(phoneNumber, message);
        }
    }
}

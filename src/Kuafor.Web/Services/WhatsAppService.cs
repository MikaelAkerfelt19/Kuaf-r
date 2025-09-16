using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using System.Text.Json;
using System.Text;

namespace Kuafor.Web.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppService> _logger;
        private readonly ITimeZoneService _timeZoneService;

        public WhatsAppService(
            IConfiguration configuration,
            ILogger<WhatsAppService> logger,
            ITimeZoneService timeZoneService)
        {
            _configuration = configuration;
            _logger = logger;
            _timeZoneService = timeZoneService;
        }

        public async Task<bool> SendMessageAsync(string phoneNumber, string message)
        {
            try
            {
                var cleanPhone = CleanPhoneNumber(phoneNumber);
                if (string.IsNullOrEmpty(cleanPhone))
                {
                    _logger.LogWarning("Geçersiz WhatsApp telefon numarası: {PhoneNumber}", phoneNumber);
                    return false;
                }

                var provider = _configuration["WhatsAppSettings:Provider"] ?? "Mock";
                var accessToken = _configuration["WhatsAppSettings:AccessToken"];
                var phoneNumberId = _configuration["WhatsAppSettings:PhoneNumberId"];

                switch (provider.ToLower())
                {
                    case "meta":
                        return await SendViaMetaApi(cleanPhone, message, accessToken, phoneNumberId);
                    case "twilio":
                        return await SendViaTwilio(cleanPhone, message, accessToken);
                    case "mock":
                    default:
                        return await SendMockWhatsApp(cleanPhone, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp mesajı gönderilirken hata oluştu: {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public async Task<bool> SendAppointmentConfirmationAsync(Appointment appointment)
        {
            var localTime = _timeZoneService.ConvertToLocalTime(appointment.StartAt);
            var message = $"🎉 Randevunuz Onaylandı!\n\n" +
                         $"📅 Tarih: {localTime:dd MMMM yyyy dddd}\n" +
                         $"⏰ Saat: {localTime:HH:mm}\n" +
                         $"💇‍♀️ Hizmet: {appointment.Service?.Name}\n" +
                         $"👨‍💼 Stilist: {appointment.Stylist?.FirstName} {appointment.Stylist?.LastName}\n" +
                         $"🏢 Şube: {appointment.Branch?.Name}\n" +
                         $"💰 Tutar: {appointment.FinalPrice:C}\n\n" +
                         $"Randevunuzu iptal etmek için: {_configuration["AppSettings:BaseUrl"]}/Customer/Appointments";

            return await SendMessageAsync(appointment.Customer?.PhoneNumber ?? appointment.Customer?.Phone ?? "", message);
        }

        public async Task<bool> SendAppointmentReminderAsync(Appointment appointment)
        {
            var localTime = _timeZoneService.ConvertToLocalTime(appointment.StartAt);
            var message = $"⏰ Randevu Hatırlatması\n\n" +
                         $"Yarın saat {localTime:HH:mm}'de randevunuz var!\n\n" +
                         $"💇‍♀️ Hizmet: {appointment.Service?.Name}\n" +
                         $"👨‍💼 Stilist: {appointment.Stylist?.FirstName} {appointment.Stylist?.LastName}\n" +
                         $"🏢 Şube: {appointment.Branch?.Name}\n\n" +
                         $"Görüşmek üzere! 😊";

            return await SendMessageAsync(appointment.Customer?.PhoneNumber ?? appointment.Customer?.Phone ?? "", message);
        }

        public async Task<bool> SendAppointmentCancellationAsync(Appointment appointment)
        {
            var localTime = _timeZoneService.ConvertToLocalTime(appointment.StartAt);
            var message = $"❌ Randevu İptal Edildi\n\n" +
                         $"Tarih: {localTime:dd MMMM yyyy dddd}\n" +
                         $"Saat: {localTime:HH:mm}\n" +
                         $"Hizmet: {appointment.Service?.Name}\n\n" +
                         $"Yeni randevu almak için: {_configuration["AppSettings:BaseUrl"]}/Customer/Appointments/Create";

            return await SendMessageAsync(appointment.Customer?.PhoneNumber ?? appointment.Customer?.Phone ?? "", message);
        }

        public async Task<bool> SendAppointmentRescheduleAsync(Appointment appointment, DateTime newDateTime)
        {
            var oldTime = _timeZoneService.ConvertToLocalTime(appointment.StartAt);
            var newTime = _timeZoneService.ConvertToLocalTime(newDateTime);
            var message = $"🔄 Randevu Tarihi Değiştirildi\n\n" +
                         $"Eski Tarih: {oldTime:dd MMMM yyyy dddd HH:mm}\n" +
                         $"Yeni Tarih: {newTime:dd MMMM yyyy dddd HH:mm}\n\n" +
                         $"Hizmet: {appointment.Service?.Name}\n" +
                         $"Stilist: {appointment.Stylist?.FirstName} {appointment.Stylist?.LastName}\n\n" +
                         $"Görüşmek üzere! 😊";

            return await SendMessageAsync(appointment.Customer?.PhoneNumber ?? appointment.Customer?.Phone ?? "", message);
        }

        public async Task<bool> SendWelcomeMessageAsync(Customer customer)
        {
            var message = $"🎉 Hoş Geldiniz {customer.Name}!\n\n" +
                         $"Kuaför randevu sistemimize kaydoldunuz.\n" +
                         $"Artık kolayca randevu alabilir, mevcut randevularınızı yönetebilirsiniz.\n\n" +
                         $"Randevu almak için: {_configuration["AppSettings:BaseUrl"]}/Customer/Appointments/Create\n\n" +
                         $"Sorularınız için bize ulaşabilirsiniz. 😊";

            return await SendMessageAsync(customer.PhoneNumber ?? customer.Phone ?? "", message);
        }

        public async Task<bool> SendPromotionalMessageAsync(string phoneNumber, string message)
        {
            var promotionalMessage = $"🎉 Özel Kampanya!\n\n" +
                                   $"{message}\n\n" +
                                   $"Randevu için: {_configuration["AppSettings:BaseUrl"]}/Customer/Appointments/Create\n\n" +
                                   $"Bu mesajı almak istemiyorsanız 'DUR' yazın.";

            return await SendMessageAsync(phoneNumber, promotionalMessage);
        }

        public async Task<bool> SendTemplateMessageAsync(string phoneNumber, string templateName, Dictionary<string, string> parameters)
        {
            try
            {
                var cleanPhone = CleanPhoneNumber(phoneNumber);
                if (string.IsNullOrEmpty(cleanPhone))
                    return false;

                var accessToken = _configuration["WhatsAppSettings:AccessToken"];
                var phoneNumberId = _configuration["WhatsAppSettings:PhoneNumberId"];

                var request = new
                {
                    messaging_product = "whatsapp",
                    to = cleanPhone,
                    type = "template",
                    template = new
                    {
                        name = templateName,
                        language = new { code = "tr" },
                        components = parameters.Select(p => new
                        {
                            type = "body",
                            parameters = new[] { new { type = "text", text = p.Value } }
                        }).ToArray()
                    }
                };

                return await SendViaMetaApi(cleanPhone, JsonSerializer.Serialize(request), accessToken, phoneNumberId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp template mesajı gönderilirken hata oluştu");
                return false;
            }
        }

        public async Task<bool> SendMediaMessageAsync(string phoneNumber, string mediaUrl, string caption = "")
        {
            try
            {
                var cleanPhone = CleanPhoneNumber(phoneNumber);
                if (string.IsNullOrEmpty(cleanPhone))
                    return false;

                var accessToken = _configuration["WhatsAppSettings:AccessToken"];
                var phoneNumberId = _configuration["WhatsAppSettings:PhoneNumberId"];

                var request = new
                {
                    messaging_product = "whatsapp",
                    to = cleanPhone,
                    type = "image",
                    image = new
                    {
                        link = mediaUrl,
                        caption = caption
                    }
                };

                return await SendViaMetaApi(cleanPhone, JsonSerializer.Serialize(request), accessToken, phoneNumberId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp medya mesajı gönderilirken hata oluştu");
                return false;
            }
        }

        private string CleanPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return string.Empty;

            var clean = new string(phoneNumber.Where(char.IsDigit).ToArray());

            if (clean.StartsWith("0"))
                clean = "90" + clean.Substring(1);
            else if (!clean.StartsWith("90"))
                clean = "90" + clean;

            return clean.Length == 12 ? clean : string.Empty;
        }

        private async Task<bool> SendViaMetaApi(string phoneNumber, string message, string? accessToken, string? phoneNumberId)
        {
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
            {
                _logger.LogWarning("WhatsApp Meta API ayarları eksik");
                return false;
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var url = $"https://graph.facebook.com/v18.0/{phoneNumberId}/messages";

            var request = new
            {
                messaging_product = "whatsapp",
                to = phoneNumber,
                type = "text",
                text = new { body = message }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("WhatsApp Meta API Response: {Response}", result);
            return response.IsSuccessStatusCode;
        }

        private async Task<bool> SendViaTwilio(string phoneNumber, string message, string? accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("WhatsApp Twilio API ayarları eksik");
                return false;
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var url = "https://api.twilio.com/2010-04-01/Accounts/AC.../Messages.json";

            var request = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("From", "whatsapp:+14155238886"),
                new KeyValuePair<string, string>("To", $"whatsapp:+{phoneNumber}"),
                new KeyValuePair<string, string>("Body", message)
            });

            var response = await client.PostAsync(url, request);
            var result = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("WhatsApp Twilio API Response: {Response}", result);
            return response.IsSuccessStatusCode;
        }

        private async Task<bool> SendMockWhatsApp(string phoneNumber, string message)
        {
            _logger.LogInformation("MOCK WHATSAPP - To: {PhoneNumber}, Message: {Message}", phoneNumber, message);
            await Task.Delay(1000);
            return true;
        }
    }
}
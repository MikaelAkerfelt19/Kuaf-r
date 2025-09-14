using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services;
using System.Text.Json;

namespace Kuafor.Web.Services
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;
        private readonly ITimeZoneService _timeZoneService;

        public SmsService(
            IConfiguration configuration,
            ILogger<SmsService> logger,
            ITimeZoneService timeZoneService)
        {
            _configuration = configuration;
            _logger = logger;
            _timeZoneService = timeZoneService;
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                // Türkiye telefon numarası formatını düzelt
                var cleanPhone = CleanPhoneNumber(phoneNumber);
                if (string.IsNullOrEmpty(cleanPhone))
                {
                    _logger.LogWarning("Geçersiz telefon numarası: {PhoneNumber}", phoneNumber);
                    return false;
                }

                // SMS sağlayıcısı ayarları
                var smsProvider = _configuration["SmsSettings:Provider"] ?? "Mock";
                var apiKey = _configuration["SmsSettings:ApiKey"];
                var senderName = _configuration["SmsSettings:SenderName"] ?? "KUAFOR";

                switch (smsProvider.ToLower())
                {
                    case "netgsm":
                        return await SendViaNetGsm(cleanPhone, message, apiKey, senderName);
                    case "iletimerkezi":
                        return await SendViaIletiMerkezi(cleanPhone, message, apiKey, senderName);
                    case "mock":
                    default:
                        return await SendMockSms(cleanPhone, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS gönderilirken hata oluştu: {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public async Task<bool> SendAppointmentConfirmationAsync(Appointment appointment)
        {
            var localStart = _timeZoneService.ConvertToLocalTime(appointment.StartAt);
            var message = $"Merhaba {appointment.Customer?.FirstName}," +
                         $"\n\nRandevunuz onaylandı!" +
                         $"\n📅 Tarih: {localStart:dd MMMM yyyy, dddd}" +
                         $"\n🕐 Saat: {localStart:HH:mm}" +
                         $"\n💇‍♀️ Kuaför: {appointment.Stylist?.FirstName} {appointment.Stylist?.LastName}" +
                         $"\n🏢 Şube: {appointment.Branch?.Name}" +
                         $"\n💅 Hizmet: {appointment.Service?.Name}" +
                         $"\n💰 Fiyat: {appointment.FinalPrice:C}" +
                         $"\n\nRandevu saatinden 15 dakika önce gelmenizi rica ederiz." +
                         $"\n\nİptal için: {_configuration["AppSettings:BaseUrl"]}/Appointments/Cancel/{appointment.Id}";

            return await SendSmsAsync(appointment.Customer?.Phone ?? "", message);
        }

        public async Task<bool> SendAppointmentReminderAsync(Appointment appointment)
        {
            var localStart = _timeZoneService.ConvertToLocalTime(appointment.StartAt);
            var message = $"Merhaba {appointment.Customer?.FirstName}," +
                         $"\n\nRandevu hatırlatması!" +
                         $"\n📅 Yarın saat {localStart:HH:mm}'de randevunuz var." +
                         $"\n💇‍♀️ Kuaför: {appointment.Stylist?.FirstName} {appointment.Stylist?.LastName}" +
                         $"\n🏢 Şube: {appointment.Branch?.Name}" +
                         $"\n\nRandevu saatinden 15 dakika önce gelmenizi rica ederiz." +
                         $"\n\nİptal için: {_configuration["AppSettings:BaseUrl"]}/Appointments/Cancel/{appointment.Id}";

            return await SendSmsAsync(appointment.Customer?.Phone ?? "", message);
        }

        public async Task<bool> SendAppointmentCancellationAsync(Appointment appointment)
        {
            var localStart = _timeZoneService.ConvertToLocalTime(appointment.StartAt);
            var message = $"Merhaba {appointment.Customer?.FirstName}," +
                         $"\n\nRandevunuz iptal edildi." +
                         $"\n📅 Tarih: {localStart:dd MMMM yyyy, dddd}" +
                         $"\n🕐 Saat: {localStart:HH:mm}" +
                         $"\n\nYeni randevu için: {_configuration["AppSettings:BaseUrl"]}/Appointments/New" +
                         $"\n\nTeşekkürler!";

            return await SendSmsAsync(appointment.Customer?.Phone ?? "", message);
        }

        public async Task<bool> SendAppointmentRescheduleAsync(Appointment appointment, DateTime newDateTime)
        {
            var localStart = _timeZoneService.ConvertToLocalTime(appointment.StartAt);
            var localNewStart = _timeZoneService.ConvertToLocalTime(newDateTime);
            
            var message = $"Merhaba {appointment.Customer?.FirstName}," +
                         $"\n\nRandevunuz ertelendi!" +
                         $"\n📅 Eski Tarih: {localStart:dd MMMM yyyy, dddd} {localStart:HH:mm}" +
                         $"\n📅 Yeni Tarih: {localNewStart:dd MMMM yyyy, dddd} {localNewStart:HH:mm}" +
                         $"\n💇‍♀️ Kuaför: {appointment.Stylist?.FirstName} {appointment.Stylist?.LastName}" +
                         $"\n🏢 Şube: {appointment.Branch?.Name}" +
                         $"\n\nRandevu saatinden 15 dakika önce gelmenizi rica ederiz." +
                         $"\n\nİptal için: {_configuration["AppSettings:BaseUrl"]}/Appointments/Cancel/{appointment.Id}";

            return await SendSmsAsync(appointment.Customer?.Phone ?? "", message);
        }

        public async Task<bool> SendWelcomeMessageAsync(Customer customer)
        {
            var message = $"Merhaba {customer.FirstName}," +
                         $"\n\nHoş geldiniz! 🎉" +
                         $"\n\nKuaför randevu sistemimize başarıyla kayıt oldunuz." +
                         $"\n\nYeni randevu için: {_configuration["AppSettings:BaseUrl"]}/Appointments/New" +
                         $"\n\nProfilinizi güncellemek için: {_configuration["AppSettings:BaseUrl"]}/Customer/Profile" +
                         $"\n\nTeşekkürler!";

            return await SendSmsAsync(customer.Phone ?? "", message);
        }

        public async Task<bool> SendPromotionalMessageAsync(string phoneNumber, string message)
        {
            var promotionalMessage = $"🎉 Özel Kampanya!" +
                                   $"\n\n{message}" +
                                   $"\n\nRandevu için: {_configuration["AppSettings:BaseUrl"]}/Appointments/New" +
                                   $"\n\nBu mesajı almak istemiyorsanız 'DUR' yazın.";

            return await SendSmsAsync(phoneNumber, promotionalMessage);
        }

        private string CleanPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return string.Empty;

            // Sadece rakamları al
            var clean = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Türkiye formatına çevir
            if (clean.StartsWith("0"))
                clean = "90" + clean.Substring(1);
            else if (!clean.StartsWith("90"))
                clean = "90" + clean;

            return clean.Length == 12 ? clean : string.Empty;
        }

        private async Task<bool> SendViaNetGsm(string phoneNumber, string message, string? apiKey, string senderName)
        {
            // NetGSM API entegrasyonu
            var client = new HttpClient();
            var url = "https://api.netgsm.com.tr/sms/send/get";
            var parameters = new Dictionary<string, string>
            {
                {"usercode", _configuration["SmsSettings:Username"] ?? ""},
                {"password", _configuration["SmsSettings:Password"] ?? ""},
                {"gsmno", phoneNumber},
                {"message", message},
                {"msgheader", senderName}
            };

            var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("NetGSM SMS Response: {Response}", result);
            return result.StartsWith("00");
        }

        private async Task<bool> SendViaIletiMerkezi(string phoneNumber, string message, string? apiKey, string senderName)
        {
            // İleti Merkezi API entegrasyonu
            var client = new HttpClient();
            var url = "https://api.iletimerkezi.com/v1/send-sms";
            
            var request = new
            {
                username = _configuration["SmsSettings:Username"] ?? "",
                password = _configuration["SmsSettings:Password"] ?? "",
                source_addr = senderName,
                dest_addr = phoneNumber,
                message = message
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("İleti Merkezi SMS Response: {Response}", result);
            return response.IsSuccessStatusCode;
        }

        private async Task<bool> SendMockSms(string phoneNumber, string message)
        {
            // Test ortamı için mock SMS
            _logger.LogInformation("MOCK SMS - To: {PhoneNumber}, Message: {Message}", phoneNumber, message);
            
            // Simüle edilmiş gecikme
            await Task.Delay(1000);
            
            // Test için her zaman başarılı döndür
            return true;
        }
    }
}

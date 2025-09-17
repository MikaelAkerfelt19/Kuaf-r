using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Controllers.Api.V1
{
    [ApiController]
    [Route("api/v1/whatsapp")]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly ILogger<WhatsAppWebhookController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWhatsAppService _whatsAppService;

        public WhatsAppWebhookController(
            ILogger<WhatsAppWebhookController> logger,
            IConfiguration configuration,
            IWhatsAppService whatsAppService)
        {
            _logger = logger;
            _configuration = configuration;
            _whatsAppService = whatsAppService;
        }

        [HttpGet]
        public IActionResult VerifyWebhook([FromQuery] string hub_mode, [FromQuery] string hub_verify_token, [FromQuery] string hub_challenge)
        {
            var verifyToken = _configuration["WhatsAppSettings:WebhookVerifyToken"];
            
            if (hub_mode == "subscribe" && hub_verify_token == verifyToken)
            {
                _logger.LogInformation("WhatsApp webhook doğrulandı");
                return Ok(hub_challenge);
            }
            
            return BadRequest("Webhook doğrulama başarısız");
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveMessage()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();
                
                _logger.LogInformation("WhatsApp webhook mesajı alındı: {Body}", body);
                
                var webhookData = JsonSerializer.Deserialize<WhatsAppWebhookData>(body);
                
                if (webhookData?.entry != null)
                {
                    foreach (var entry in webhookData.entry)
                    {
                        if (entry.changes != null)
                        {
                            foreach (var change in entry.changes)
                            {
                                if (change.value?.messages != null)
                                {
                                    foreach (var message in change.value.messages)
                                    {
                                        await ProcessIncomingMessage(message, change.value.metadata);
                                    }
                                }
                            }
                        }
                    }
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp webhook mesajı işlenirken hata oluştu");
                return BadRequest();
            }
        }

        private async Task ProcessIncomingMessage(WhatsAppMessage message, WhatsAppMetadata? metadata)
        {
            try
            {
                var phoneNumber = message.from;
                var messageText = message.text?.body ?? "";

                _logger.LogInformation("Gelen WhatsApp mesajı: {PhoneNumber} - {Message}", phoneNumber, messageText);

                // "DUR" komutu kontrolü
                if (messageText.ToUpper().Contains("DUR"))
                {
                    await HandleOptOut(phoneNumber);
                    return;
                }

                // Diğer komutlar burada işlenebilir
                // Örneğin: "RANDEVU" yazarsa randevu alma sürecini başlat
                if (messageText.ToUpper().Contains("RANDEVU"))
                {
                    await HandleAppointmentRequest(phoneNumber);
                    return;
                }

                // Varsayılan yanıt
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    await _whatsAppService.SendMessageAsync(phoneNumber, 
                        "Merhaba! Size nasıl yardımcı olabilirim?\n\n" +
                        "• Randevu almak için 'RANDEVU' yazın\n" +
                        "• Kampanya mesajlarını durdurmak için 'DUR' yazın");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gelen mesaj işlenirken hata oluştu: {PhoneNumber}", message.from);
            }
        }

        private async Task HandleOptOut(string? phoneNumber)
        {
            // Müşteriyi kampanya listesinden çıkar
            _logger.LogInformation("Müşteri kampanya listesinden çıkarıldı: {PhoneNumber}", phoneNumber);
            
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                await _whatsAppService.SendMessageAsync(phoneNumber, 
                    "✅ Kampanya mesajlarınız durduruldu. " +
                    "Tekrar almak istediğinizde bizimle iletişime geçebilirsiniz.");
            }
        }

        private async Task HandleAppointmentRequest(string? phoneNumber)
        {
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                await _whatsAppService.SendMessageAsync(phoneNumber, 
                    "📅 Randevu almak için lütfen web sitemizi ziyaret edin:\n\n" +
                    "🌐 " + _configuration["AppSettings:BaseUrl"] + "/Customer/Appointments/Create\n\n" +
                    "Veya telefon ile arayabilirsiniz: 📞 0212 555 0123");
            }
        }
    }

    // Webhook veri modelleri
    public class WhatsAppWebhookData
    {
        public List<WhatsAppEntry>? entry { get; set; }
    }

    public class WhatsAppEntry
    {
        public string? id { get; set; }
        public List<WhatsAppChange>? changes { get; set; }
    }

    public class WhatsAppChange
    {
        public string? field { get; set; }
        public WhatsAppValue? value { get; set; }
    }

    public class WhatsAppValue
    {
        public string? messaging_product { get; set; }
        public WhatsAppMetadata? metadata { get; set; }
        public List<WhatsAppMessage>? messages { get; set; }
    }

    public class WhatsAppMetadata
    {
        public string? display_phone_number { get; set; }
        public string? phone_number_id { get; set; }
    }

    public class WhatsAppMessage
    {
        public string? from { get; set; }
        public string? id { get; set; }
        public string? timestamp { get; set; }
        public WhatsAppText? text { get; set; }
    }

    public class WhatsAppText
    {
        public string? body { get; set; }
    }
}
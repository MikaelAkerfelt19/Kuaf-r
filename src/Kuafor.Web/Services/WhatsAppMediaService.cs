using Kuafor.Web.Services.Interfaces;
using System.Text.Json;

namespace Kuafor.Web.Services
{
    public class WhatsAppMediaService : IWhatsAppMediaService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppMediaService> _logger;
        private readonly IWhatsAppService _whatsAppService;

        public WhatsAppMediaService(
            IConfiguration configuration,
            ILogger<WhatsAppMediaService> logger,
            IWhatsAppService whatsAppService)
        {
            _configuration = configuration;
            _logger = logger;
            _whatsAppService = whatsAppService;
        }

        public async Task<string> UploadMediaAsync(Stream mediaStream, string fileName, string contentType)
        {
            try
            {
                // Bu örnekte basit bir URL döndürüyoruz
                // Gerçek implementasyonda Azure Blob Storage veya başka bir CDN kullanılmalı
                var baseUrl = _configuration["AppSettings:BaseUrl"];
                var mediaId = Guid.NewGuid().ToString();
                var fileExtension = Path.GetExtension(fileName);
                
                // Simüle edilmiş upload
                await Task.Delay(1000);
                
                return $"{baseUrl}/media/whatsapp/{mediaId}{fileExtension}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Medya yüklenirken hata oluştu: {FileName}", fileName);
                throw;
            }
        }

        public async Task<bool> SendImageAsync(string phoneNumber, string mediaUrl, string caption = "")
        {
            return await _whatsAppService.SendMediaMessageAsync(phoneNumber, mediaUrl, caption);
        }

        public async Task<bool> SendDocumentAsync(string phoneNumber, string mediaUrl, string fileName, string caption = "")
        {
            try
            {
                var accessToken = _configuration["WhatsAppSettings:AccessToken"];
                var phoneNumberId = _configuration["WhatsAppSettings:PhoneNumberId"];

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
                {
                    _logger.LogWarning("WhatsApp API ayarları eksik");
                    return false;
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var url = $"https://graph.facebook.com/v18.0/{phoneNumberId}/messages";

                var request = new
                {
                    messaging_product = "whatsapp",
                    to = phoneNumber,
                    type = "document",
                    document = new
                    {
                        link = mediaUrl,
                        filename = fileName,
                        caption = caption
                    }
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("WhatsApp Document Response: {Response}", result);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Doküman gönderilirken hata oluştu");
                return false;
            }
        }

        public async Task<bool> SendVideoAsync(string phoneNumber, string mediaUrl, string caption = "")
        {
            try
            {
                var accessToken = _configuration["WhatsAppSettings:AccessToken"];
                var phoneNumberId = _configuration["WhatsAppSettings:PhoneNumberId"];

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
                {
                    _logger.LogWarning("WhatsApp API ayarları eksik");
                    return false;
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var url = $"https://graph.facebook.com/v18.0/{phoneNumberId}/messages";

                var request = new
                {
                    messaging_product = "whatsapp",
                    to = phoneNumber,
                    type = "video",
                    video = new
                    {
                        link = mediaUrl,
                        caption = caption
                    }
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("WhatsApp Video Response: {Response}", result);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video gönderilirken hata oluştu");
                return false;
            }
        }

        public async Task<bool> SendAudioAsync(string phoneNumber, string mediaUrl)
        {
            try
            {
                var accessToken = _configuration["WhatsAppSettings:AccessToken"];
                var phoneNumberId = _configuration["WhatsAppSettings:PhoneNumberId"];

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
                {
                    _logger.LogWarning("WhatsApp API ayarları eksik");
                    return false;
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var url = $"https://graph.facebook.com/v18.0/{phoneNumberId}/messages";

                var request = new
                {
                    messaging_product = "whatsapp",
                    to = phoneNumber,
                    type = "audio",
                    audio = new
                    {
                        link = mediaUrl
                    }
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("WhatsApp Audio Response: {Response}", result);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ses gönderilirken hata oluştu");
                return false;
            }
        }

        public async Task<bool> SendLocationAsync(string phoneNumber, double latitude, double longitude, string name = "", string address = "")
        {
            try
            {
                var accessToken = _configuration["WhatsAppSettings:AccessToken"];
                var phoneNumberId = _configuration["WhatsAppSettings:PhoneNumberId"];

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
                {
                    _logger.LogWarning("WhatsApp API ayarları eksik");
                    return false;
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var url = $"https://graph.facebook.com/v18.0/{phoneNumberId}/messages";

                var request = new
                {
                    messaging_product = "whatsapp",
                    to = phoneNumber,
                    type = "location",
                    location = new
                    {
                        latitude = latitude,
                        longitude = longitude,
                        name = name,
                        address = address
                    }
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("WhatsApp Location Response: {Response}", result);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Konum gönderilirken hata oluştu");
                return false;
            }
        }

        public async Task<bool> SendContactAsync(string phoneNumber, string contactName, string contactPhone)
        {
            try
            {
                var accessToken = _configuration["WhatsAppSettings:AccessToken"];
                var phoneNumberId = _configuration["WhatsAppSettings:PhoneNumberId"];

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
                {
                    _logger.LogWarning("WhatsApp API ayarları eksik");
                    return false;
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var url = $"https://graph.facebook.com/v18.0/{phoneNumberId}/messages";

                var request = new
                {
                    messaging_product = "whatsapp",
                    to = phoneNumber,
                    type = "contacts",
                    contacts = new[]
                    {
                        new
                        {
                            name = new
                            {
                                formatted_name = contactName
                            },
                            phones = new[]
                            {
                                new
                                {
                                    phone = contactPhone,
                                    type = "MAIN"
                                }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("WhatsApp Contact Response: {Response}", result);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kişi gönderilirken hata oluştu");
                return false;
            }
        }

        public async Task<bool> SendInteractiveMessageAsync(string phoneNumber, string headerText, string bodyText, string footerText, List<WhatsAppButton> buttons)
        {
            try
            {
                var accessToken = _configuration["WhatsAppSettings:AccessToken"];
                var phoneNumberId = _configuration["WhatsAppSettings:PhoneNumberId"];

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(phoneNumberId))
                {
                    _logger.LogWarning("WhatsApp API ayarları eksik");
                    return false;
                }

                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var url = $"https://graph.facebook.com/v18.0/{phoneNumberId}/messages";

                var request = new
                {
                    messaging_product = "whatsapp",
                    to = phoneNumber,
                    type = "interactive",
                    interactive = new
                    {
                        type = "button",
                        header = new
                        {
                            type = "text",
                            text = headerText
                        },
                        body = new
                        {
                            text = bodyText
                        },
                        footer = new
                        {
                            text = footerText
                        },
                        action = new
                        {
                            buttons = buttons.Select(b => new
                            {
                                type = b.Type,
                                reply = new
                                {
                                    id = b.Id,
                                    title = b.Title
                                }
                            }).ToArray()
                        }
                    }
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                var result = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("WhatsApp Interactive Response: {Response}", result);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İnteraktif mesaj gönderilirken hata oluştu");
                return false;
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingsController : Controller
    {
        private readonly IConfiguration _configuration;

        public SettingsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult WhatsApp()
        {
            var model = new WhatsAppSettingsViewModel
            {
                PhoneNumber = _configuration["WhatsAppSettings:PhoneNumber"] ?? "+905559998877",
                CallToAction = _configuration["WhatsAppSettings:WidgetCallToAction"] ?? "Nasıl yardımcı olabilirim?",
                Position = _configuration["WhatsAppSettings:WidgetPosition"] ?? "right",
                AccessToken = _configuration["WhatsAppSettings:AccessToken"] ?? "",
                PhoneNumberId = _configuration["WhatsAppSettings:PhoneNumberId"] ?? "",
                BusinessAccountId = _configuration["WhatsAppSettings:BusinessAccountId"] ?? ""
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult WhatsApp(WhatsAppSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Burada appsettings.json'ı güncellemek için bir servis kullanılabilir
                // Şimdilik sadece model'i View'a geri döndürüyoruz
                TempData["SuccessMessage"] = "WhatsApp ayarları güncellendi! (Not: Bu ayarlar appsettings.json dosyasında manuel olarak güncellenmelidir)";
                return View(model);
            }

            return View(model);
        }
    }

    public class WhatsAppSettingsViewModel
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string CallToAction { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string PhoneNumberId { get; set; } = string.Empty;
        public string BusinessAccountId { get; set; } = string.Empty;
    }
}

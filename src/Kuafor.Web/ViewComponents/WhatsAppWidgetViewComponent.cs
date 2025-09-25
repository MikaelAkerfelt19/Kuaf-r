using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Kuafor.Web.ViewComponents
{
    public class WhatsAppWidgetViewComponent : ViewComponent
    {
        private readonly IConfiguration _configuration;

        public WhatsAppWidgetViewComponent(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IViewComponentResult Invoke()
        {
            var whatsappNumber = _configuration["WhatsAppSettings:PhoneNumber"] ?? "+905559998877";
            var callToAction = _configuration["WhatsAppSettings:WidgetCallToAction"] ?? "Nasıl yardımcı olabilirim?";
            var position = _configuration["WhatsAppSettings:WidgetPosition"] ?? "right";

            var model = new WhatsAppWidgetViewModel
            {
                PhoneNumber = whatsappNumber,
                CallToAction = callToAction,
                Position = position
            };

            return View(model);
        }
    }

    public class WhatsAppWidgetViewModel
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string CallToAction { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }
}

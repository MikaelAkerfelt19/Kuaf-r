using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net;

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
            var whatsappSettings = _configuration.GetSection("WhatsAppSettings");

            var model = new WhatsAppWidgetModel
            {
                PhoneNumber = whatsappSettings["PhoneNumber"] ?? "+905314687179",
                CallToAction = whatsappSettings["WidgetCallToAction"] ?? @"Nasıl yardımcı olabilirim?",
                Position = whatsappSettings["WidgetPosition"] ?? "right"
            };

            return View(model);
        }
    }

    public class WhatsAppWidgetModel
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string CallToAction { get; set; } = string.Empty;
        public string Position { get; set; } = "right";
    }
}

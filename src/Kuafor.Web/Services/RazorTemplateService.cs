using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class RazorTemplateService : ITemplateService
{
    
    public string GetTemplate(string templateName)
    {
        // Şimdilik basit template'ler döndürelim
        return templateName switch
        {
            "AppointmentConfirmation" => @"
                <h2>Randevu Onaylandı</h2>
                <p>Sayın {{appointment.Customer.FirstName}} {{appointment.Customer.LastName}},</p>
                <p>{{appointment.StartAt:dd.MM.yyyy HH:mm}} tarihindeki randevunuz onaylanmıştır.</p>
                <p>Hizmet: {{appointment.Service.Name}}</p>
                <p>Kuaför: {{appointment.Stylist.FirstName}} {{appointment.Stylist.LastName}}</p>
                <p>Şube: {{appointment.Branch.Name}}</p>",
            
            "AppointmentReminder" => @"
                <h2>Randevu Hatırlatması</h2>
                <p>Sayın {{appointment.Customer.FirstName}} {{appointment.Customer.LastName}},</p>
                <p>Yarın {{appointment.StartAt:dd.MM.yyyy HH:mm}} tarihindeki randevunuzu hatırlatmak isteriz.</p>
                <p>Hizmet: {{appointment.Service.Name}}</p>
                <p>Kuaför: {{appointment.Stylist.FirstName}} {{appointment.Stylist.LastName}}</p>
                <p>Şube: {{appointment.Branch.Name}}</p>",
            
            "AppointmentCancellation" => @"
                <h2>Randevu İptal Edildi</h2>
                <p>Sayın {{appointment.Customer.FirstName}} {{appointment.Customer.LastName}},</p>
                <p>{{appointment.StartAt:dd.MM.yyyy HH:mm}} tarihindeki randevunuz iptal edilmiştir.</p>
                <p>Yeni randevu almak için web sitemizi ziyaret edebilirsiniz.</p>",
            
            _ => "<p>Template bulunamadı.</p>"
        };
    }
    
    public Task<string> RenderTemplateAsync(string templateName, object model)
    {
        var template = GetTemplate(templateName);
        
        // Basit string replacement (gerçek Razor engine yerine)
        var result = template;
        
        // Model property'lerini replace et
        foreach (var prop in model.GetType().GetProperties())
        {
            var value = prop.GetValue(model);
            if (value != null)
            {
                result = result.Replace($"{{{{{prop.Name}}}}}", value.ToString());
            }
        }
        
        return Task.FromResult(result);
    }
}

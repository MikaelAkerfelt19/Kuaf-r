using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces
{
    public interface IWhatsAppTemplateService
    {
        Task<WhatsAppTemplate> CreateTemplateAsync(WhatsAppTemplate template);
        Task<WhatsAppTemplate> UpdateTemplateAsync(WhatsAppTemplate template);
        Task<bool> DeleteTemplateAsync(int templateId);
        Task<WhatsAppTemplate?> GetTemplateAsync(int templateId);
        Task<WhatsAppTemplate?> GetTemplateByNameAsync(string name);
        Task<List<WhatsAppTemplate>> GetTemplatesByCategoryAsync(string category);
        Task<List<WhatsAppTemplate>> GetActiveTemplatesAsync();
        Task<bool> SendTemplateMessageAsync(string phoneNumber, string templateName, Dictionary<string, string> parameters);
        Task<bool> SendTemplateMessageAsync(string phoneNumber, int templateId, Dictionary<string, string> parameters);
        Task<List<WhatsAppTemplateUsage>> GetTemplateUsageStatsAsync(int templateId, DateTime? from = null, DateTime? to = null);
        Task<bool> ValidateTemplateParametersAsync(string templateName, Dictionary<string, string> parameters);
    }
}
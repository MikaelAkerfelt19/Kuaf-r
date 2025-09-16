using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;
using System.Text.Json;


namespace Kuafor.Web.Services
{
    public class WhatsAppTemplateService : IWhatsAppTemplateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<WhatsAppTemplateService> _logger;

        public WhatsAppTemplateService(
            ApplicationDbContext context,
            IWhatsAppService whatsAppService,
            ILogger<WhatsAppTemplateService> logger)
        {
            _context = context;
            _whatsAppService = whatsAppService;
            _logger = logger;
        }

        public async Task<WhatsAppTemplate> CreateTemplateAsync(WhatsAppTemplate template)
        {
            template.CreatedAt = DateTime.UtcNow;
            template.Status = "PENDING";
            _context.WhatsAppTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<WhatsAppTemplate> UpdateTemplateAsync(WhatsAppTemplate template)
        {
            template.UpdatedAt = DateTime.UtcNow;
            _context.WhatsAppTemplates.Update(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<bool> DeleteTemplateAsync(int templateId)
        {
            var template = await _context.WhatsAppTemplates.FindAsync(templateId);
            if (template == null) return false;

            _context.WhatsAppTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<WhatsAppTemplate?> GetTemplateAsync(int templateId)
        {
            return await _context.WhatsAppTemplates
                .Include(t => t.Parameters)
                .FirstOrDefaultAsync(t => t.Id == templateId);
        }

        public async Task<WhatsAppTemplate?> GetTemplateByNameAsync(string name)
        {
            return await _context.WhatsAppTemplates
                .Include(t => t.Parameters)
                .FirstOrDefaultAsync(t => t.Name == name && t.IsActive);
        }

        public async Task<List<WhatsAppTemplate>> GetTemplatesByCategoryAsync(string category)
        {
            return await _context.WhatsAppTemplates
                .Include(t => t.Parameters)
                .Where(t => t.Category == category && t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<List<WhatsAppTemplate>> GetActiveTemplatesAsync()
        {
            return await _context.WhatsAppTemplates
                .Include(t => t.Parameters)
                .Where(t => t.IsActive && t.Status == "APPROVED")
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<bool> SendTemplateMessageAsync(string phoneNumber, string templateName, Dictionary<string, string> parameters)
        {
            try
            {
                var template = await GetTemplateByNameAsync(templateName);
                if (template == null)
                {
                    _logger.LogWarning("WhatsApp template bulunamadı: {TemplateName}", templateName);
                    return false;
                }

                return await SendTemplateMessageAsync(phoneNumber, template.Id, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Template mesajı gönderilirken hata oluştu: {TemplateName}", templateName);
                return false;
            }
        }

        public async Task<bool> SendTemplateMessageAsync(string phoneNumber, int templateId, Dictionary<string, string> parameters)
        {
            try
            {
                var template = await GetTemplateAsync(templateId);
                if (template == null)
                {
                    _logger.LogWarning("WhatsApp template bulunamadı: {TemplateId}", templateId);
                    return false;
                }

                // Parametreleri doğrula
                if (!await ValidateTemplateParametersAsync(template.Name, parameters))
                {
                    _logger.LogWarning("Template parametreleri geçersiz: {TemplateName}", template.Name);
                    return false;
                }

                // Template mesajını gönder
                var success = await _whatsAppService.SendTemplateMessageAsync(phoneNumber, template.Name, parameters);

                // Kullanım istatistiğini kaydet
                var usage = new WhatsAppTemplateUsage
                {
                    TemplateId = templateId,
                    PhoneNumber = phoneNumber,
                    Status = success ? "SENT" : "FAILED",
                    SentAt = DateTime.UtcNow
                };

                _context.WhatsAppTemplateUsages.Add(usage);
                await _context.SaveChangesAsync();

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Template mesajı gönderilirken hata oluştu: {TemplateId}", templateId);
                return false;
            }
        }

        public async Task<List<WhatsAppTemplateUsage>> GetTemplateUsageStatsAsync(int templateId, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.WhatsAppTemplateUsages
                .Where(u => u.TemplateId == templateId);

            if (from.HasValue)
                query = query.Where(u => u.SentAt >= from.Value);

            if (to.HasValue)
                query = query.Where(u => u.SentAt <= to.Value);

            return await query
                .OrderByDescending(u => u.SentAt)
                .ToListAsync();
        }

        public async Task<bool> ValidateTemplateParametersAsync(string templateName, Dictionary<string, string> parameters)
        {
            var template = await GetTemplateByNameAsync(templateName);
            if (template == null) return false;

            var requiredParams = template.Parameters
                .Where(p => p.IsRequired)
                .Select(p => p.ParameterName)
                .ToList();

            // Gerekli parametrelerin varlığını kontrol et
            foreach (var requiredParam in requiredParams)
            {
                if (!parameters.ContainsKey(requiredParam) || string.IsNullOrEmpty(parameters[requiredParam]))
                {
                    _logger.LogWarning("Gerekli parametre eksik: {ParameterName}", requiredParam);
                    return false;
                }
            }

            return true;
        }
    }
}
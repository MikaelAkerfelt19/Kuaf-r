using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class MarketingService : IMarketingService
{
    private readonly ApplicationDbContext _context;

    public MarketingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Campaign> CreateCampaignAsync(Campaign campaign)
    {
        campaign.CreatedAt = DateTime.UtcNow;
        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync();
        return campaign;
    }

    public async Task<Campaign> UpdateCampaignAsync(Campaign campaign)
    {
        campaign.UpdatedAt = DateTime.UtcNow;
        _context.Campaigns.Update(campaign);
        await _context.SaveChangesAsync();
        return campaign;
    }

    public async Task<bool> DeleteCampaignAsync(int campaignId)
    {
        var campaign = await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign == null) return false;

        _context.Campaigns.Remove(campaign);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Campaign?> GetCampaignAsync(int campaignId)
    {
        return await _context.Campaigns
            .Include(c => c.Targets)
            .Include(c => c.Messages)
            .Include(c => c.Performance)
            .FirstOrDefaultAsync(c => c.Id == campaignId);
    }

    public async Task<List<Campaign>> GetActiveCampaignsAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Campaigns
            .Include(c => c.Targets)
            .Include(c => c.Messages)
            .Where(c => c.Status == "Active" && 
                       c.StartDate <= now && 
                       c.EndDate >= now)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Campaign>> GetAllCampaignsAsync()
    {
        return await _context.Campaigns
            .Include(c => c.Targets)
            .Include(c => c.Messages)
            .Include(c => c.Performance)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<CampaignTarget> CreateCampaignTargetAsync(CampaignTarget target)
    {
        _context.CampaignTargets.Add(target);
        await _context.SaveChangesAsync();
        return target;
    }

    public async Task<List<CampaignTarget>> GetCampaignTargetsAsync(int campaignId)
    {
        return await _context.CampaignTargets
            .Where(ct => ct.CampaignId == campaignId)
            .ToListAsync();
    }

    public async Task<CampaignMessage> CreateCampaignMessageAsync(CampaignMessage message)
    {
        message.CreatedAt = DateTime.UtcNow;
        _context.CampaignMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<List<CampaignMessage>> GetCampaignMessagesAsync(int campaignId)
    {
        return await _context.CampaignMessages
            .Where(cm => cm.CampaignId == campaignId)
            .OrderBy(cm => cm.Sequence)
            .ToListAsync();
    }

    public async Task<Models.Entities.CampaignPerformance> GetCampaignPerformanceAsync(int campaignId)
    {
        return await _context.CampaignPerformances
            .FirstOrDefaultAsync(cp => cp.CampaignId == campaignId);
    }

    public async Task<Models.Entities.CampaignPerformance> UpdateCampaignPerformanceAsync(Models.Entities.CampaignPerformance performance)
    {
        performance.UpdatedAt = DateTime.UtcNow;
        _context.CampaignPerformances.Update(performance);
        await _context.SaveChangesAsync();
        return performance;
    }

    public async Task<List<Customer>> GetTargetCustomersAsync(int campaignId)
    {
        var targets = await _context.CampaignTargets
            .Where(ct => ct.CampaignId == campaignId)
            .ToListAsync();

        var customers = new List<Customer>();

        foreach (var target in targets)
        {
            switch (target.TargetType)
            {
                case "All":
                    customers.AddRange(await _context.Customers.ToListAsync());
                    break;
                case "Segment":
                    if (!string.IsNullOrEmpty(target.TargetValue))
                    {
                        var segmentCustomers = await _context.Customers
                            .Where(c => c.Segment == target.TargetValue)
                            .ToListAsync();
                        customers.AddRange(segmentCustomers);
                    }
                    break;
                case "Age":
                    if (int.TryParse(target.TargetValue, out int age))
                    {
                        var ageCustomers = await _context.Customers
                            .Where(c => c.BirthDate.HasValue && 
                                      DateTime.Now.Year - c.BirthDate.Value.Year == age)
                            .ToListAsync();
                        customers.AddRange(ageCustomers);
                    }
                    break;
                case "Gender":
                    if (!string.IsNullOrEmpty(target.TargetValue))
                    {
                        var genderCustomers = await _context.Customers
                            .Where(c => c.Gender == target.TargetValue)
                            .ToListAsync();
                        customers.AddRange(genderCustomers);
                    }
                    break;
                case "LastVisit":
                    if (int.TryParse(target.TargetValue, out int days))
                    {
                        var cutoffDate = DateTime.UtcNow.AddDays(-days);
                        var lastVisitCustomers = await _context.Customers
                            .Where(c => c.LastVisitDate.HasValue && c.LastVisitDate.Value >= cutoffDate)
                            .ToListAsync();
                        customers.AddRange(lastVisitCustomers);
                    }
                    break;
            }
        }

        return customers.Distinct().ToList();
    }

    public async Task<bool> SendCampaignAsync(int campaignId)
    {
        var campaign = await GetCampaignAsync(campaignId);
        if (campaign == null || campaign.Status != "Active") return false;

        var targetCustomers = await GetTargetCustomersAsync(campaignId);
        var messages = await GetCampaignMessagesAsync(campaignId);

        if (!messages.Any()) return false;

        // Her müşteri için mesaj gönder
        foreach (var customer in targetCustomers)
        {
            foreach (var message in messages)
            {
                // Burada gerçek mesaj gönderme işlemi yapılacak
                // Email, SMS, Push notification vb.
                await SendMessageToCustomerAsync(customer, message);
            }
        }

        // Kampanya performansını güncelle
        var performance = await GetCampaignPerformanceAsync(campaignId);
        if (performance == null)
        {
            performance = new Models.Entities.CampaignPerformance
            {
                CampaignId = campaignId,
                SentCount = targetCustomers.Count,
                DeliveredCount = 0,
                OpenedCount = 0,
                ClickedCount = 0,
                ConvertedCount = 0,
                CreatedAt = DateTime.UtcNow
            };
            _context.CampaignPerformances.Add(performance);
        }
        else
        {
            performance.SentCount += targetCustomers.Count;
            performance.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    private Task SendMessageToCustomerAsync(Customer customer, CampaignMessage message)
    {
        // Burada gerçek mesaj gönderme işlemi yapılacak
        // EmailService, SMSService vb. kullanılabilir
        
        // Şimdilik sadece log tutuyoruz
        Console.WriteLine($"Sending {message.MessageType} to {customer.Name} ({customer.Email}): {message.Subject}");
        
        // Gerçek implementasyonda:
        // - Email için EmailService kullanılacak
        // - SMS için SMSService kullanılacak
        // - Push notification için NotificationService kullanılacak
        
        return Task.CompletedTask;
    }

    public async Task<List<Campaign>> GetCampaignsByTypeAsync(string campaignType)
    {
        return await _context.Campaigns
            .Include(c => c.Targets)
            .Include(c => c.Messages)
            .Where(c => c.CampaignType == campaignType)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Campaign>> GetCampaignsByStatusAsync(string status)
    {
        return await _context.Campaigns
            .Include(c => c.Targets)
            .Include(c => c.Messages)
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ActivateCampaignAsync(int campaignId)
    {
        var campaign = await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign == null) return false;

        campaign.Status = "Active";
        campaign.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateCampaignAsync(int campaignId)
    {
        var campaign = await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign == null) return false;

        campaign.Status = "Inactive";
        campaign.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Models.Entities.CampaignPerformance> GetCampaignAnalyticsAsync(int campaignId)
    {
        var performance = await GetCampaignPerformanceAsync(campaignId);
        if (performance == null) return null;

        // Ek analitik hesaplamalar
        performance.DeliveryRate = performance.SentCount > 0 ? 
            (double)performance.DeliveredCount / performance.SentCount * 100 : 0;
        
        performance.OpenRate = performance.DeliveredCount > 0 ? 
            (double)performance.OpenedCount / performance.DeliveredCount * 100 : 0;
        
        performance.ClickRate = performance.OpenedCount > 0 ? 
            (double)performance.ClickedCount / performance.OpenedCount * 100 : 0;
        
        performance.ConversionRate = performance.ClickedCount > 0 ? 
            (double)performance.ConvertedCount / performance.ClickedCount * 100 : 0;

        return performance;
    }

    // Interface'den eksik olan metodları ekliyoruz
    public async Task<List<Campaign>> GetCampaignsAsync()
    {
        return await GetAllCampaignsAsync();
    }

    public async Task<bool> ScheduleCampaignAsync(int campaignId, DateTime scheduledDate)
    {
        var campaign = await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign == null) return false;

        campaign.Status = "Scheduled";
        campaign.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelCampaignAsync(int campaignId)
    {
        var campaign = await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign == null) return false;

        campaign.Status = "Cancelled";
        campaign.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // Şablon yönetimi - şimdilik basit implementasyon
    public async Task<CampaignTemplate> CreateTemplateAsync(CampaignTemplate template)
    {
        template.CreatedAt = DateTime.UtcNow;
        _context.CampaignTemplates.Add(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<CampaignTemplate> UpdateTemplateAsync(CampaignTemplate template)
    {
        template.UpdatedAt = DateTime.UtcNow;
        _context.CampaignTemplates.Update(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<bool> DeleteTemplateAsync(int templateId)
    {
        var template = await _context.CampaignTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId);

        if (template == null) return false;

        _context.CampaignTemplates.Remove(template);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<CampaignTemplate>> GetTemplatesAsync()
    {
        return await _context.CampaignTemplates
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<CampaignTemplate?> GetTemplateAsync(int templateId)
    {
        return await _context.CampaignTemplates
            .FirstOrDefaultAsync(t => t.Id == templateId);
    }

    // Referans sistemi - şimdilik basit implementasyon
    public async Task<ReferralProgram> CreateReferralProgramAsync(ReferralProgram program)
    {
        program.CreatedAt = DateTime.UtcNow;
        _context.ReferralPrograms.Add(program);
        await _context.SaveChangesAsync();
        return program;
    }

    public async Task<ReferralProgram> UpdateReferralProgramAsync(ReferralProgram program)
    {
        program.UpdatedAt = DateTime.UtcNow;
        _context.ReferralPrograms.Update(program);
        await _context.SaveChangesAsync();
        return program;
    }

    public async Task<List<ReferralProgram>> GetReferralProgramsAsync()
    {
        return await _context.ReferralPrograms
            .Where(rp => rp.IsActive)
            .OrderByDescending(rp => rp.CreatedAt)
            .ToListAsync();
    }

    public async Task<ReferralProgram?> GetActiveReferralProgramAsync()
    {
        return await _context.ReferralPrograms
            .FirstOrDefaultAsync(rp => rp.IsActive);
    }

    public async Task<Referral> CreateReferralAsync(int referrerId, int refereeId, string? referralCode = null)
    {
        var referral = new Referral
        {
            ReferrerCustomerId = referrerId,
            RefereeCustomerId = refereeId,
            ReferralCode = referralCode,
            ReferralDate = DateTime.UtcNow
        };

        _context.Referrals.Add(referral);
        await _context.SaveChangesAsync();
        return referral;
    }

    public async Task<List<Referral>> GetReferralsByReferrerAsync(int customerId)
    {
        return await _context.Referrals
            .Where(r => r.ReferrerCustomerId == customerId)
            .OrderByDescending(r => r.ReferralDate)
            .ToListAsync();
    }

    public async Task<List<Referral>> GetReferralsByRefereeAsync(int customerId)
    {
        return await _context.Referrals
            .Where(r => r.RefereeCustomerId == customerId)
            .OrderByDescending(r => r.ReferralDate)
            .ToListAsync();
    }

    public async Task<bool> CompleteReferralAsync(int referralId)
    {
        var referral = await _context.Referrals
            .FirstOrDefaultAsync(r => r.Id == referralId);

        if (referral == null) return false;

        referral.Status = "Completed";
        referral.CompletionDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // Doğum günü kampanyaları - şimdilik basit implementasyon
    public async Task<BirthdayCampaign> CreateBirthdayCampaignAsync(BirthdayCampaign campaign)
    {
        campaign.CreatedAt = DateTime.UtcNow;
        _context.BirthdayCampaigns.Add(campaign);
        await _context.SaveChangesAsync();
        return campaign;
    }

    public async Task<BirthdayCampaign> UpdateBirthdayCampaignAsync(BirthdayCampaign campaign)
    {
        campaign.UpdatedAt = DateTime.UtcNow;
        _context.BirthdayCampaigns.Update(campaign);
        await _context.SaveChangesAsync();
        return campaign;
    }

    public async Task<List<BirthdayCampaign>> GetBirthdayCampaignsAsync()
    {
        return await _context.BirthdayCampaigns
            .Where(bc => bc.IsActive)
            .OrderByDescending(bc => bc.CreatedAt)
            .ToListAsync();
    }

    public Task<bool> ProcessBirthdayMessagesAsync()
    {
        // Doğum günü mesajlarını işleme mantığı
        // Bu metod günlük olarak çalışacak
        return Task.FromResult(true);
    }

    // İstatistikler - şimdilik basit implementasyon
    public async Task<CampaignStatistics> GetCampaignStatisticsAsync(int campaignId)
    {
        var campaign = await GetCampaignAsync(campaignId);
        var performance = await GetCampaignPerformanceAsync(campaignId);

        return new CampaignStatistics
        {
            CampaignId = campaignId,
            CampaignName = campaign?.Name ?? "",
            TotalRecipients = performance?.SentCount ?? 0,
            SentCount = performance?.SentCount ?? 0,
            DeliveredCount = performance?.DeliveredCount ?? 0,
            OpenedCount = performance?.OpenedCount ?? 0,
            ClickedCount = performance?.ClickedCount ?? 0,
            ConvertedCount = performance?.ConvertedCount ?? 0,
            DeliveryRate = performance?.DeliveryRate ?? 0,
            OpenRate = performance?.OpenRate ?? 0,
            ClickRate = performance?.ClickRate ?? 0,
            ConversionRate = performance?.ConversionRate ?? 0,
            TotalCost = 0,
            CostPerConversion = 0
        };
    }

    public async Task<List<Models.Entities.CampaignPerformance>> GetCampaignPerformanceAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.CampaignPerformances.AsQueryable();

        if (from.HasValue)
            query = query.Where(cp => cp.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(cp => cp.CreatedAt <= to.Value);

        return await query.ToListAsync();
    }

    public async Task<ReferralStatistics> GetReferralStatisticsAsync()
    {
        var referrals = await _context.Referrals.ToListAsync();

        return new ReferralStatistics
        {
            TotalReferrals = referrals.Count,
            CompletedReferrals = referrals.Count(r => r.Status == "Completed"),
            PendingReferrals = referrals.Count(r => r.Status == "Pending"),
            TotalRewardsPaid = referrals.Where(r => r.Status == "Completed").Sum(r => r.ReferrerRewardAmount + r.RefereeRewardAmount),
            TotalRevenueFromReferrals = 0, // Bu hesaplama daha karmaşık olabilir
            ConversionRate = referrals.Count > 0 ? (double)referrals.Count(r => r.Status == "Completed") / referrals.Count * 100 : 0,
            TopReferrers = new List<TopReferrer>()
        };
    }

    // Otomatik kampanyalar - şimdilik basit implementasyon
    public Task<bool> SendWelcomeMessageAsync(int customerId)
    {
        // Hoş geldin mesajı gönderme mantığı
        return Task.FromResult(true);
    }

    public Task<bool> SendAppointmentReminderAsync(int appointmentId)
    {
        // Randevu hatırlatma mantığı
        return Task.FromResult(true);
    }

    public Task<bool> SendFollowUpMessageAsync(int appointmentId)
    {
        // Takip mesajı gönderme mantığı
        return Task.FromResult(true);
    }

    public Task<bool> SendChurnPreventionMessageAsync(int customerId)
    {
        // Müşteri kaybını önleme mesajı mantığı
        return Task.FromResult(true);
    }
}



using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IMarketingService
{
    // Kampanya yönetimi
    Task<Campaign> CreateCampaignAsync(Campaign campaign);
    Task<Campaign> UpdateCampaignAsync(Campaign campaign);
    Task<bool> DeleteCampaignAsync(int campaignId);
    Task<Campaign?> GetCampaignAsync(int campaignId);
    Task<List<Campaign>> GetCampaignsAsync();
    Task<List<Campaign>> GetActiveCampaignsAsync();
    
    // Kampanya gönderimi
    Task<bool> SendCampaignAsync(int campaignId);
    Task<bool> ScheduleCampaignAsync(int campaignId, DateTime scheduledDate);
    Task<bool> CancelCampaignAsync(int campaignId);
    
    // Şablon yönetimi
    Task<CampaignTemplate> CreateTemplateAsync(CampaignTemplate template);
    Task<CampaignTemplate> UpdateTemplateAsync(CampaignTemplate template);
    Task<bool> DeleteTemplateAsync(int templateId);
    Task<List<CampaignTemplate>> GetTemplatesAsync();
    Task<CampaignTemplate?> GetTemplateAsync(int templateId);
    
    // Referans sistemi
    Task<ReferralProgram> CreateReferralProgramAsync(ReferralProgram program);
    Task<ReferralProgram> UpdateReferralProgramAsync(ReferralProgram program);
    Task<List<ReferralProgram>> GetReferralProgramsAsync();
    Task<ReferralProgram?> GetActiveReferralProgramAsync();
    
    Task<Referral> CreateReferralAsync(int referrerId, int refereeId, string? referralCode = null);
    Task<List<Referral>> GetReferralsByReferrerAsync(int customerId);
    Task<List<Referral>> GetReferralsByRefereeAsync(int customerId);
    Task<bool> CompleteReferralAsync(int referralId);
    
    // Doğum günü kampanyaları
    Task<BirthdayCampaign> CreateBirthdayCampaignAsync(BirthdayCampaign campaign);
    Task<BirthdayCampaign> UpdateBirthdayCampaignAsync(BirthdayCampaign campaign);
    Task<List<BirthdayCampaign>> GetBirthdayCampaignsAsync();
    Task<bool> ProcessBirthdayMessagesAsync(); // Günlük çalışacak
    
    // İstatistikler
    Task<CampaignStatistics> GetCampaignStatisticsAsync(int campaignId);
    Task<List<Models.Entities.CampaignPerformance>> GetCampaignPerformanceAsync(DateTime? from = null, DateTime? to = null);
    Task<ReferralStatistics> GetReferralStatisticsAsync();
    
    // Otomatik kampanyalar
    Task<bool> SendWelcomeMessageAsync(int customerId);
    Task<bool> SendAppointmentReminderAsync(int appointmentId);
    Task<bool> SendFollowUpMessageAsync(int appointmentId);
    Task<bool> SendChurnPreventionMessageAsync(int customerId);
}

public class CampaignStatistics
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = "";
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int DeliveredCount { get; set; }
    public int OpenedCount { get; set; }
    public int ClickedCount { get; set; }
    public int ConvertedCount { get; set; }
    public double DeliveryRate { get; set; }
    public double OpenRate { get; set; }
    public double ClickRate { get; set; }
    public double ConversionRate { get; set; }
    public decimal TotalCost { get; set; }
    public decimal CostPerConversion { get; set; }
}

public class CampaignPerformance
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; } = "";
    public string Type { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public int SentCount { get; set; }
    public int ConvertedCount { get; set; }
    public double ConversionRate { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Revenue { get; set; }
    public decimal ROI { get; set; }
}

public class ReferralStatistics
{
    public int TotalReferrals { get; set; }
    public int CompletedReferrals { get; set; }
    public int PendingReferrals { get; set; }
    public decimal TotalRewardsPaid { get; set; }
    public decimal TotalRevenueFromReferrals { get; set; }
    public double ConversionRate { get; set; }
    public List<TopReferrer> TopReferrers { get; set; } = new();
}

public class TopReferrer
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public int ReferralCount { get; set; }
    public int CompletedReferrals { get; set; }
    public decimal TotalRewards { get; set; }
}

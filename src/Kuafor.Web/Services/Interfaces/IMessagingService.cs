using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Entities.Analytics;

namespace Kuafor.Web.Services.Interfaces
{
    public interface IMessagingService
    {
        // Grup yönetimi
        Task<MessageGroup> CreateGroupAsync(string name, string? description = null);
        Task<MessageGroup?> GetGroupAsync(int groupId);
        Task<List<MessageGroup>> GetAllGroupsAsync();
        Task<bool> UpdateGroupAsync(MessageGroup group);
        Task<bool> DeleteGroupAsync(int groupId);
        Task<bool> AddCustomerToGroupAsync(int groupId, int customerId);
        Task<bool> RemoveCustomerFromGroupAsync(int groupId, int customerId);
        Task<List<Customer>> GetGroupMembersAsync(int groupId);
        Task<int> GetGroupMemberCountAsync(int groupId);

        // Toplu mesajlaşma
        Task<bool> SendBulkMessageAsync(List<int> customerIds, string message, string messageType = "WhatsApp");
        Task<bool> SendGroupMessageAsync(int groupId, string message, string messageType = "WhatsApp");
        Task<MessageCampaign> CreateMessageCampaignAsync(string name, string content, string type, int? groupId = null);
        Task<bool> SendMessageCampaignAsync(int campaignId);

        // Filtreli mesajlaşma
        Task<List<Customer>> GetFilteredCustomersAsync(MessagingCustomerFilter filter);
        Task<bool> SendFilteredMessageAsync(MessagingCustomerFilter filter, string message, string messageType = "WhatsApp", List<int>? excludeCustomerIds = null);

        // Kara liste yönetimi
        Task<bool> AddToBlacklistAsync(string phoneNumber, string reason, string? customerName = null, string? notes = null);
        Task<bool> RemoveFromBlacklistAsync(string phoneNumber);
        Task<List<MessageBlacklist>> GetBlacklistedNumbersAsync();
        Task<bool> IsBlacklistedAsync(string phoneNumber);

        // Raporlama
        Task<List<Kuafor.Web.Models.Entities.MessageReport>> GetMessageReportsAsync(DateTime? from = null, DateTime? to = null, string? messageType = null);
        Task<Kuafor.Web.Models.Entities.MessageReport> CreateMessageReportAsync(int campaignId, string phoneNumber, string messageType, string deliveryStatus, string messageContent, decimal? cost = null);
        Task<bool> UpdateMessageReportAsync(int reportId, string deliveryStatus, DateTime? deliveredAt = null, DateTime? readAt = null, string? errorMessage = null);
        Task<Kuafor.Web.Models.Entities.Analytics.MessageReport> GetMessageStatisticsAsync(DateTime startDate, DateTime endDate);

        // Kredi yönetimi
        Task<MessageCredit> GetCreditAsync(string type);
        Task<bool> UpdateCreditAsync(string type, int amount, decimal? costPerCredit = null);
        Task<bool> DeductCreditAsync(string type, int amount);

        // Şablon yönetimi
        Task<MessageTemplate> CreateTemplateAsync(string name, string type, string content, string? description = null);
        Task<List<MessageTemplate>> GetTemplatesAsync(string? type = null);
        Task<bool> UpdateTemplateAsync(MessageTemplate template);
        Task<bool> DeleteTemplateAsync(int templateId);

        // Müşteri mesaj yönetimi
        Task<List<MessageRecipient>> GetCustomerMessagesAsync(int customerId);
        Task<bool> SendWhatsAppMessageToCustomerAsync(int customerId, string message);
        Task<List<Customer>> GetAllCustomersForMessagingAsync();
    }

    public class MessagingCustomerFilter
    {
        public bool IncludePassiveCustomers { get; set; } = false;
        public string? DebtStatus { get; set; }
        public string? RiskStatus { get; set; }
        public string? ServiceReceived { get; set; }
        public int? StaffId { get; set; }
        public string? Gender { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public DateTime? LastVisitFrom { get; set; }
        public DateTime? LastVisitTo { get; set; }
        public string? SearchTerm { get; set; }
    }
}

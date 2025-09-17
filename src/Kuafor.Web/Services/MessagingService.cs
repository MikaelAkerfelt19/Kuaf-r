using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Enums;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services
{
    public class MessagingService : IMessagingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWhatsAppService _whatsAppService;
        private readonly ISmsService _smsService;
        private readonly ILogger<MessagingService> _logger;

        public MessagingService(
            ApplicationDbContext context,
            IWhatsAppService whatsAppService,
            ISmsService smsService,
            ILogger<MessagingService> logger)
        {
            _context = context;
            _whatsAppService = whatsAppService;
            _smsService = smsService;
            _logger = logger;
        }

        // Grup yönetimi metodları
        public async Task<MessageGroup> CreateGroupAsync(string name, string? description = null)
        {
            var group = new MessageGroup
            {
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            _context.MessageGroups.Add(group);
            await _context.SaveChangesAsync();
            return group;
        }

        public async Task<MessageGroup?> GetGroupAsync(int groupId)
        {
            return await _context.MessageGroups
                .Include(g => g.Members)
                .ThenInclude(m => m.Customer)
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task<List<MessageGroup>> GetAllGroupsAsync()
        {
            return await _context.MessageGroups
                .Include(g => g.Members)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<bool> UpdateGroupAsync(MessageGroup group)
        {
            try
            {
                _context.MessageGroups.Update(group);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Grup güncellenirken hata oluştu: {GroupId}", group.Id);
                return false;
            }
        }

        public async Task<bool> DeleteGroupAsync(int groupId)
        {
            try
            {
                var group = await _context.MessageGroups.FindAsync(groupId);
                if (group == null) return false;

                _context.MessageGroups.Remove(group);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Grup silinirken hata oluştu: {GroupId}", groupId);
                return false;
            }
        }

        public async Task<bool> AddCustomerToGroupAsync(int groupId, int customerId)
        {
            try
            {
                // Müşteri zaten grupta mı kontrol et
                var existingMember = await _context.MessageGroupMembers
                    .FirstOrDefaultAsync(m => m.MessageGroupId == groupId && m.CustomerId == customerId);

                if (existingMember != null) return true; // Zaten grupta

                var member = new MessageGroupMember
                {
                    MessageGroupId = groupId,
                    CustomerId = customerId,
                    AddedAt = DateTime.UtcNow
                };

                _context.MessageGroupMembers.Add(member);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteri gruba eklenirken hata oluştu: {GroupId}, {CustomerId}", groupId, customerId);
                return false;
            }
        }

        public async Task<bool> RemoveCustomerFromGroupAsync(int groupId, int customerId)
        {
            try
            {
                var member = await _context.MessageGroupMembers
                    .FirstOrDefaultAsync(m => m.MessageGroupId == groupId && m.CustomerId == customerId);

                if (member == null) return false;

                _context.MessageGroupMembers.Remove(member);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Müşteri gruptan çıkarılırken hata oluştu: {GroupId}, {CustomerId}", groupId, customerId);
                return false;
            }
        }

        public async Task<List<Customer>> GetGroupMembersAsync(int groupId)
        {
            return await _context.MessageGroupMembers
                .Where(m => m.MessageGroupId == groupId)
                .Include(m => m.Customer)
                .Select(m => m.Customer)
                .ToListAsync();
        }

        public async Task<int> GetGroupMemberCountAsync(int groupId)
        {
            return await _context.MessageGroupMembers
                .CountAsync(m => m.MessageGroupId == groupId);
        }

        // Toplu mesajlaşma metodları
        public async Task<bool> SendBulkMessageAsync(List<int> customerIds, string message, string messageType = "WhatsApp")
        {
            try
            {
                var customers = await _context.Customers
                    .Where(c => customerIds.Contains(c.Id))
                    .ToListAsync();

                foreach (var customer in customers)
                {
                    var phoneNumber = customer.PhoneNumber ?? customer.Phone ?? "";
                    if (string.IsNullOrEmpty(phoneNumber)) continue;

                    // Kara listede mi kontrol et
                    if (await IsBlacklistedAsync(phoneNumber)) continue;

                    bool success = false;
                    switch (messageType.ToLower())
                    {
                        case "whatsapp":
                            success = await _whatsAppService.SendPromotionalMessageAsync(phoneNumber, message);
                            break;
                        case "sms":
                            success = await _smsService.SendPromotionalMessageAsync(phoneNumber, message);
                            break;
                    }

                    // Rapor oluştur
                    await CreateMessageReportAsync(0, phoneNumber, messageType, 
                        success ? "Sent" : "Failed", message);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toplu mesaj gönderilirken hata oluştu");
                return false;
            }
        }

        public async Task<bool> SendGroupMessageAsync(int groupId, string message, string messageType = "WhatsApp")
        {
            var members = await GetGroupMembersAsync(groupId);
            var customerIds = members.Select(m => m.Id).ToList();
            return await SendBulkMessageAsync(customerIds, message, messageType);
        }

        public async Task<MessageCampaign> CreateMessageCampaignAsync(string name, string content, string type, int? groupId = null)
        {
            var campaign = new MessageCampaign
            {
                Name = name,
                Content = content,
                Type = type,
                MessageGroupId = groupId,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };

            _context.MessageCampaigns.Add(campaign);
            await _context.SaveChangesAsync();
            return campaign;
        }

        public async Task<bool> SendMessageCampaignAsync(int campaignId)
        {
            try
            {
                var campaign = await _context.MessageCampaigns
                    .Include(c => c.MessageGroup)
                    .ThenInclude(g => g!.Members)
                    .ThenInclude(m => m.Customer)
                    .FirstOrDefaultAsync(c => c.Id == campaignId);

                if (campaign == null) return false;

                var customers = campaign.MessageGroup?.Members?
                    .Select(m => m.Customer)
                    .Where(c => c != null)
                    .Cast<Customer>()
                    .ToList() ?? await _context.Customers.ToListAsync();

                var customerIds = customers.Select(c => c.Id).ToList();
                var success = await SendBulkMessageAsync(customerIds, campaign.Content, campaign.Type);

                if (success)
                {
                    campaign.Status = "Sent";
                    campaign.SentAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kampanya gönderilirken hata oluştu: {CampaignId}", campaignId);
                return false;
            }
        }

        // Filtreli mesajlaşma metodları
        public async Task<List<Customer>> GetFilteredCustomersAsync(MessagingCustomerFilter filter)
        {
            var query = _context.Customers.AsQueryable();

            // Aktif müşteri filtresi
            if (!filter.IncludePassiveCustomers)
            {
                query = query.Where(c => c.IsActive);
            }

            // Cinsiyet filtresi
            if (!string.IsNullOrEmpty(filter.Gender))
            {
                query = query.Where(c => c.Gender == filter.Gender);
            }

            // Yaş filtresi
            if (filter.MinAge.HasValue || filter.MaxAge.HasValue)
            {
                var now = DateTime.Now;
                if (filter.MinAge.HasValue)
                {
                    var minBirthYear = now.Year - filter.MinAge.Value;
                    query = query.Where(c => c.DateOfBirth.HasValue && c.DateOfBirth.Value.Year <= minBirthYear);
                }
                if (filter.MaxAge.HasValue)
                {
                    var maxBirthYear = now.Year - filter.MaxAge.Value;
                    query = query.Where(c => c.DateOfBirth.HasValue && c.DateOfBirth.Value.Year >= maxBirthYear);
                }
            }

            // Son ziyaret filtresi
            if (filter.LastVisitFrom.HasValue)
            {
                query = query.Where(c => c.LastVisitDate.HasValue && c.LastVisitDate >= filter.LastVisitFrom.Value);
            }
            if (filter.LastVisitTo.HasValue)
            {
                query = query.Where(c => c.LastVisitDate.HasValue && c.LastVisitDate <= filter.LastVisitTo.Value);
            }

            // Arama terimi
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(c => 
                    c.FirstName.ToLower().Contains(searchTerm) ||
                    c.LastName.ToLower().Contains(searchTerm) ||
                    (c.Phone ?? "").Contains(searchTerm) ||
                    (c.PhoneNumber ?? "").Contains(searchTerm));
            }

            return await query.ToListAsync();
        }

        public async Task<bool> SendFilteredMessageAsync(MessagingCustomerFilter filter, string message, string messageType = "WhatsApp", List<int>? excludeCustomerIds = null)
        {
            var customers = await GetFilteredCustomersAsync(filter);
            
            if (excludeCustomerIds != null && excludeCustomerIds.Any())
            {
                customers = customers.Where(c => !excludeCustomerIds.Contains(c.Id)).ToList();
            }

            var customerIds = customers.Select(c => c.Id).ToList();
            return await SendBulkMessageAsync(customerIds, message, messageType);
        }

        // Kara liste yönetimi metodları
        public async Task<bool> AddToBlacklistAsync(string phoneNumber, string reason, string? customerName = null, string? notes = null)
        {
            try
            {
                var blacklistEntry = new MessageBlacklist
                {
                    PhoneNumber = phoneNumber,
                    CustomerName = customerName,
                    Reason = reason,
                    Notes = notes,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MessageBlacklists.Add(blacklistEntry);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kara listeye eklenirken hata oluştu: {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public async Task<bool> RemoveFromBlacklistAsync(string phoneNumber)
        {
            try
            {
                var blacklistEntry = await _context.MessageBlacklists
                    .FirstOrDefaultAsync(b => b.PhoneNumber == phoneNumber && b.IsActive);

                if (blacklistEntry == null) return false;

                blacklistEntry.IsActive = false;
                blacklistEntry.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kara listeden çıkarılırken hata oluştu: {PhoneNumber}", phoneNumber);
                return false;
            }
        }

        public async Task<List<MessageBlacklist>> GetBlacklistedNumbersAsync()
        {
            return await _context.MessageBlacklists
                .Where(b => b.IsActive)
                .OrderBy(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsBlacklistedAsync(string phoneNumber)
        {
            return await _context.MessageBlacklists
                .AnyAsync(b => b.PhoneNumber == phoneNumber && b.IsActive);
        }

        // Raporlama metodları
        public async Task<List<MessageReport>> GetMessageReportsAsync(DateTime? from = null, DateTime? to = null, string? messageType = null)
        {
            var query = _context.MessageReports.AsQueryable();

            if (from.HasValue)
                query = query.Where(r => r.SentAt >= from.Value);
            if (to.HasValue)
                query = query.Where(r => r.SentAt <= to.Value);
            if (!string.IsNullOrEmpty(messageType))
                query = query.Where(r => r.MessageType == messageType);

            return await query
                .OrderByDescending(r => r.SentAt)
                .ToListAsync();
        }

        public async Task<MessageReport> CreateMessageReportAsync(int campaignId, string phoneNumber, string messageType, string deliveryStatus, string messageContent, decimal? cost = null)
        {
            var report = new MessageReport
            {
                MessageCampaignId = campaignId,
                PhoneNumber = phoneNumber,
                MessageType = messageType,
                DeliveryStatus = deliveryStatus,
                MessageContent = messageContent,
                Cost = cost,
                SentAt = DateTime.UtcNow
            };

            _context.MessageReports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<bool> UpdateMessageReportAsync(int reportId, string deliveryStatus, DateTime? deliveredAt = null, DateTime? readAt = null, string? errorMessage = null)
        {
            try
            {
                var report = await _context.MessageReports.FindAsync(reportId);
                if (report == null) return false;

                report.DeliveryStatus = deliveryStatus;
                if (deliveredAt.HasValue) report.DeliveredAt = deliveredAt.Value;
                if (readAt.HasValue) report.ReadAt = readAt.Value;
                if (!string.IsNullOrEmpty(errorMessage)) report.ErrorMessage = errorMessage;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mesaj raporu güncellenirken hata oluştu: {ReportId}", reportId);
                return false;
            }
        }

        // Kredi yönetimi metodları
        public async Task<MessageCredit> GetCreditAsync(string type)
        {
            var credit = await _context.MessageCredits
                .FirstOrDefaultAsync(c => c.Type == type);

            if (credit == null)
            {
                credit = new MessageCredit
                {
                    Type = type,
                    CreditAmount = 0,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };
                _context.MessageCredits.Add(credit);
                await _context.SaveChangesAsync();
            }

            return credit;
        }

        public async Task<bool> UpdateCreditAsync(string type, int amount, decimal? costPerCredit = null)
        {
            try
            {
                var credit = await GetCreditAsync(type);
                credit.CreditAmount = amount;
                credit.LastUpdated = DateTime.UtcNow;
                if (costPerCredit.HasValue) credit.CostPerCredit = costPerCredit.Value;

                _context.MessageCredits.Update(credit);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kredi güncellenirken hata oluştu: {Type}", type);
                return false;
            }
        }

        public async Task<bool> DeductCreditAsync(string type, int amount)
        {
            try
            {
                var credit = await GetCreditAsync(type);
                if (credit.CreditAmount < amount) return false;

                credit.CreditAmount -= amount;
                credit.LastUpdated = DateTime.UtcNow;

                _context.MessageCredits.Update(credit);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kredi düşülürken hata oluştu: {Type}", type);
                return false;
            }
        }

        // Şablon yönetimi metodları
        public async Task<MessageTemplate> CreateTemplateAsync(string name, string type, string content, string? description = null)
        {
            var template = new MessageTemplate
            {
                Name = name,
                Type = type,
                Content = content,
                Description = description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.MessageTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<List<MessageTemplate>> GetTemplatesAsync(string? type = null)
        {
            var query = _context.MessageTemplates.Where(t => t.IsActive);
            
            if (!string.IsNullOrEmpty(type))
                query = query.Where(t => t.Type == type);

            return await query.OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<bool> UpdateTemplateAsync(MessageTemplate template)
        {
            try
            {
                template.UpdatedAt = DateTime.UtcNow;
                _context.MessageTemplates.Update(template);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şablon güncellenirken hata oluştu: {TemplateId}", template.Id);
                return false;
            }
        }

        public async Task<bool> DeleteTemplateAsync(int templateId)
        {
            try
            {
                var template = await _context.MessageTemplates.FindAsync(templateId);
                if (template == null) return false;

                template.IsActive = false;
                template.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şablon silinirken hata oluştu: {TemplateId}", templateId);
                return false;
            }
        }

        public async Task<bool> SendScheduledMessageAsync(List<int> recipientIds, string message, DateTime scheduledTime, string messageType)
        {
            // Zamanlanmış mesaj gönderme işlemi yapar
            var campaign = new MessageCampaign
            {
                Name = "Zamanlanmış Mesaj",
                Content = message, // MessageCampaign'de Content kullanılıyor
                Type = messageType,
                ScheduledAt = scheduledTime,
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
            };
            
            _context.MessageCampaigns.Add(campaign);
            await _context.SaveChangesAsync();
            
            // Background service ile zamanlanmış gönderim yapılacak
            return true;
        }

        public async Task<bool> SendBirthdayMessagesAsync()
        {
            // Doğum günü mesajları gönderir
            var today = DateTime.Today;
            var birthdayCustomers = await _context.Customers
                .Where(c => c.DateOfBirth.HasValue && 
                           c.DateOfBirth.Value.Month == today.Month && 
                           c.DateOfBirth.Value.Day == today.Day)
                .ToListAsync();
            
            foreach (var customer in birthdayCustomers)
            {
                var message = $"Merhaba {customer.FirstName}, doğum gününüz kutlu olsun! 🎉";
                if (!string.IsNullOrEmpty(customer.Phone))
                {
                    await _smsService.SendSmsAsync(customer.Phone, message);
                    await _whatsAppService.SendMessageAsync(customer.Phone, message);
                }
            }
            
            return true;
        }

        public async Task<bool> SendAppointmentRemindersAsync()
        {
            // Randevu hatırlatma mesajları gönderir
            var tomorrow = DateTime.Today.AddDays(1);
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Service)
                .Where(a => a.StartAt.Date == tomorrow && a.Status == AppointmentStatus.Confirmed)
                .ToListAsync();
            
            foreach (var appointment in appointments)
            {
                var message = $"Merhaba {appointment.Customer.FirstName}, yarın saat {appointment.StartAt:HH:mm}'da {appointment.Service.Name} randevunuz bulunmaktadır.";
                // await _smsService.SendAppointmentReminderAsync(appointment, message); // TODO: Method signature düzelt
                // await _whatsAppService.SendAppointmentReminderAsync(appointment, message); // TODO: Method signature düzelt
            }
            
            return true;
        }

        public async Task<Kuafor.Web.Models.Entities.Analytics.MessageReport> GetMessageStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            // Mesaj istatistiklerini getirir
            var campaigns = await _context.MessageCampaigns
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .ToListAsync();
            
            return new Kuafor.Web.Models.Entities.Analytics.MessageReport
            {
                TotalSent = campaigns.Sum(c => c.SentCount),
                TotalDelivered = campaigns.Sum(c => c.DeliveredCount),
                TotalFailed = campaigns.Sum(c => c.FailedCount),
                DeliveryRate = campaigns.Any() ? (campaigns.Sum(c => c.DeliveredCount) * 100.0 / campaigns.Sum(c => c.SentCount)) : 0
            };
        }
    }
}

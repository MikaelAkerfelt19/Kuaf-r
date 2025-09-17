using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using CustomerEntity = Kuafor.Web.Models.Entities.Customer;

namespace Kuafor.Web.Areas.Admin.Models
{
    public class GroupMessagingViewModel
    {
        public List<MessageGroup> Groups { get; set; } = new();
        public List<int> SelectedGroupIds { get; set; } = new();
        
        [Required(ErrorMessage = "Mesaj içeriği gereklidir")]
        [StringLength(1000, ErrorMessage = "Mesaj 1000 karakterden uzun olamaz")]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public string MessageType { get; set; } = "WhatsApp";
    }

    public class BulkMessagingViewModel
    {
        public List<CustomerEntity> Customers { get; set; } = new();
        public List<int> SelectedCustomerIds { get; set; } = new();
        
        [Required(ErrorMessage = "Mesaj içeriği gereklidir")]
        [StringLength(1000, ErrorMessage = "Mesaj 1000 karakterden uzun olamaz")]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public string MessageType { get; set; } = "WhatsApp";
        
        public string SearchTerm { get; set; } = string.Empty;
        public bool IncludePassiveCustomers { get; set; } = false;
    }

    public class FilteredMessagingViewModel
    {
        public List<CustomerEntity> Customers { get; set; } = new();
        public Kuafor.Web.Services.Interfaces.MessagingCustomerFilter Filter { get; set; } = new();
        public List<int> ExcludeCustomerIds { get; set; } = new();
        
        [Required(ErrorMessage = "Mesaj içeriği gereklidir")]
        [StringLength(1000, ErrorMessage = "Mesaj 1000 karakterden uzun olamaz")]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public string MessageType { get; set; } = "WhatsApp";
    }

    public class MessageReportsViewModel
    {
        public List<MessageReport> Reports { get; set; } = new();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string MessageType { get; set; } = "All";
        public string CustomerSearch { get; set; } = string.Empty;
    }

    public class MessageBlacklistViewModel
    {
        [Required(ErrorMessage = "Telefon numarası gereklidir")]
        [StringLength(20, ErrorMessage = "Telefon numarası 20 karakterden uzun olamaz")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        public string? CustomerName { get; set; }
        
        [Required(ErrorMessage = "Sebep gereklidir")]
        [StringLength(50, ErrorMessage = "Sebep 50 karakterden uzun olamaz")]
        public string Reason { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Notlar 500 karakterden uzun olamaz")]
        public string? Notes { get; set; }
    }


    public class MessageCampaignViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = "SMS";
        public string Status { get; set; } = "Draft";
        public DateTime? ScheduledAt { get; set; }
        public int SentCount { get; set; }
        public int DeliveredCount { get; set; }
        public int FailedCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MessageTemplateViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "SMS";
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}

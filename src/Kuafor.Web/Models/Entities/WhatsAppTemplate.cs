using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities
{
    public class WhatsAppTemplate
    {
        public int Id { get; set; }
        
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required, StringLength(50)]
        public string Category { get; set; } = "UTILITY"; // UTILITY, MARKETING, AUTHENTICATION
        
        [Required, StringLength(1000)]
        public string Content { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [StringLength(20)]
        public string Language { get; set; } = "tr";
        
        [StringLength(20)]
        public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        
        // Template parametreleri
        public virtual ICollection<WhatsAppTemplateParameter> Parameters { get; set; } = new List<WhatsAppTemplateParameter>();
        
        // Kullanım istatistikleri
        public virtual ICollection<WhatsAppTemplateUsage> Usages { get; set; } = new List<WhatsAppTemplateUsage>();
    }

    public class WhatsAppTemplateParameter
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        
        [Required, StringLength(50)]
        public string ParameterName { get; set; } = string.Empty;
        
        [Required, StringLength(20)]
        public string ParameterType { get; set; } = "text"; // text, currency, date_time
        
        [StringLength(100)]
        public string? Example { get; set; }
        
        public bool IsRequired { get; set; } = true;
        public int Order { get; set; }
        
        public virtual WhatsAppTemplate Template { get; set; } = null!;
    }

    public class WhatsAppTemplateUsage
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "SENT"; // SENT, DELIVERED, READ, FAILED
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ReadAt { get; set; }
        
        public virtual WhatsAppTemplate Template { get; set; } = null!;
    }
}
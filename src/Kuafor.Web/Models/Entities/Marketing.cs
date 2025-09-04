using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

// Pazarlama kampanyaları
public class MarketingCampaign
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required, StringLength(20)]
    public string Type { get; set; } = string.Empty; // SMS, Email, Push, Birthday, Referral
    
    [Required, StringLength(20)]
    public string Status { get; set; } = "Draft"; // Draft, Scheduled, Running, Completed, Cancelled
    
    [StringLength(1000)]
    public string Content { get; set; } = string.Empty; // Mesaj içeriği
    
    [StringLength(200)]
    public string? Subject { get; set; } // Email konusu
    
    // Hedef kitle
    public string? TargetSegment { get; set; } // Müşteri segmenti
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public string? Gender { get; set; } // Male, Female, All
    
    // Zamanlama
    public DateTime? ScheduledDate { get; set; } // Planlanan gönderim tarihi
    public DateTime? StartDate { get; set; } // Başlangıç tarihi
    public DateTime? EndDate { get; set; } // Bitiş tarihi
    
    // İstatistikler
    public int TotalRecipients { get; set; } = 0; // Toplam alıcı sayısı
    public int SentCount { get; set; } = 0; // Gönderilen sayısı
    public int DeliveredCount { get; set; } = 0; // Teslim edilen sayısı
    public int OpenedCount { get; set; } = 0; // Açılan sayısı (Email)
    public int ClickedCount { get; set; } = 0; // Tıklanan sayısı
    public int ConvertedCount { get; set; } = 0; // Dönüşüm sayısı
    
    // Kampanya ayarları
    public bool IsActive { get; set; } = true;
    public decimal? Budget { get; set; } // Kampanya bütçesi
    public decimal? CostPerMessage { get; set; } // Mesaj başına maliyet
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; } // Oluşturan kullanıcı
    
    // Navigation properties
    public virtual ICollection<CampaignRecipient> Recipients { get; set; } = new List<CampaignRecipient>();
    public virtual ICollection<CampaignTemplate> Templates { get; set; } = new List<CampaignTemplate>();
}

// Kampanya alıcıları
public class CampaignRecipient
{
    public int Id { get; set; }
    
    [Required]
    public int CampaignId { get; set; }
    
    [Required]
    public int CustomerId { get; set; }
    
    [Required, StringLength(100)]
    public string ContactInfo { get; set; } = string.Empty; // Telefon veya email
    
    [Required, StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Sent, Delivered, Failed, Bounced
    
    public DateTime? SentAt { get; set; } // Gönderim zamanı
    public DateTime? DeliveredAt { get; set; } // Teslim zamanı
    public DateTime? OpenedAt { get; set; } // Açılma zamanı
    public DateTime? ClickedAt { get; set; } // Tıklama zamanı
    
    [StringLength(500)]
    public string? ErrorMessage { get; set; } // Hata mesajı
    
    // Navigation properties
    public virtual MarketingCampaign Campaign { get; set; } = null!;
    public virtual Customer Customer { get; set; } = null!;
}

// Kampanya şablonları
public class CampaignTemplate
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required, StringLength(20)]
    public string Type { get; set; } = string.Empty; // SMS, Email, Push
    
    [Required, StringLength(1000)]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Subject { get; set; } // Email konusu
    
    // Şablon değişkenleri
    [StringLength(500)]
    public string? Variables { get; set; } // JSON formatında değişkenler
    
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false; // Varsayılan şablon
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<MarketingCampaign> Campaigns { get; set; } = new List<MarketingCampaign>();
}

// Referans sistemi
public class ReferralProgram
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    // Referans kuralları
    public decimal ReferrerReward { get; set; } = 0; // Referans veren ödülü
    public decimal RefereeReward { get; set; } = 0; // Referans edilen ödülü
    public int MinAppointmentCount { get; set; } = 1; // Minimum randevu sayısı
    public decimal MinSpentAmount { get; set; } = 0; // Minimum harcama tutarı
    
    // Zaman sınırları
    public int ValidityDays { get; set; } = 30; // Geçerlilik süresi (gün)
    public DateTime? StartDate { get; set; } // Başlangıç tarihi
    public DateTime? EndDate { get; set; } // Bitiş tarihi
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<Referral> Referrals { get; set; } = new List<Referral>();
}

// Referans kayıtları
public class Referral
{
    public int Id { get; set; }
    
    [Required]
    public int ReferralProgramId { get; set; }
    
    [Required]
    public int ReferrerCustomerId { get; set; } // Referans veren müşteri
    
    [Required]
    public int RefereeCustomerId { get; set; } // Referans edilen müşteri
    
    [Required, StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Completed, Expired, Cancelled
    
    [StringLength(50)]
    public string? ReferralCode { get; set; } // Referans kodu
    
    public DateTime ReferralDate { get; set; } = DateTime.UtcNow; // Referans tarihi
    public DateTime? CompletionDate { get; set; } // Tamamlanma tarihi
    public DateTime? ExpiryDate { get; set; } // Son geçerlilik tarihi
    
    // Ödül bilgileri
    public decimal ReferrerRewardAmount { get; set; } = 0; // Referans veren ödülü
    public decimal RefereeRewardAmount { get; set; } = 0; // Referans edilen ödülü
    public bool ReferrerRewardPaid { get; set; } = false; // Referans veren ödülü ödendi mi
    public bool RefereeRewardPaid { get; set; } = false; // Referans edilen ödülü ödendi mi
    
    [StringLength(500)]
    public string? Notes { get; set; } // Notlar
    
    // Navigation properties
    public virtual ReferralProgram ReferralProgram { get; set; } = null!;
    public virtual Customer ReferrerCustomer { get; set; } = null!;
    public virtual Customer RefereeCustomer { get; set; } = null!;
}

// Doğum günü kampanyaları
public class BirthdayCampaign
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required, StringLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    // Kampanya ayarları
    public int DaysBeforeBirthday { get; set; } = 0; // Doğum gününden kaç gün önce gönderilsin
    public int DaysAfterBirthday { get; set; } = 7; // Doğum gününden kaç gün sonra gönderilsin
    
    // Ödül ayarları
    public decimal? DiscountPercentage { get; set; } // İndirim yüzdesi
    public decimal? DiscountAmount { get; set; } // İndirim tutarı
    public int? BonusPoints { get; set; } // Bonus puan
    public string? CouponCode { get; set; } // Kupon kodu
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<BirthdayMessage> BirthdayMessages { get; set; } = new List<BirthdayMessage>();
}

// Doğum günü mesajları
public class BirthdayMessage
{
    public int Id { get; set; }
    
    [Required]
    public int BirthdayCampaignId { get; set; }
    
    [Required]
    public int CustomerId { get; set; }
    
    [Required, StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Sent, Delivered, Failed
    
    public DateTime BirthdayDate { get; set; } // Doğum günü tarihi
    public DateTime? SentAt { get; set; } // Gönderim zamanı
    public DateTime? DeliveredAt { get; set; } // Teslim zamanı
    
    [StringLength(500)]
    public string? ErrorMessage { get; set; } // Hata mesajı
    
    // Navigation properties
    public virtual BirthdayCampaign BirthdayCampaign { get; set; } = null!;
    public virtual Customer Customer { get; set; } = null!;
}

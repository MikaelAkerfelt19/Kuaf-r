using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

// Müşteri analitikleri için yeni entity
public class CustomerAnalytics
{
    public int Id { get; set; }
    
    [Required]
    public int CustomerId { get; set; }
    
    // RFM Analizi
    public int RecencyScore { get; set; } // Son ziyaretten bu yana geçen gün sayısı
    public int FrequencyScore { get; set; } // Toplam randevu sayısı
    public decimal MonetaryScore { get; set; } // Toplam harcama
    
    // Segmentasyon
    [StringLength(50)]
    public string Segment { get; set; } = "Bronz"; // Bronz, Gümüş, Altın, Platin, VIP
    
    [StringLength(50)]
    public string LifecycleStage { get; set; } = "Yeni"; // Yeni, Aktif, Risk, Kayıp, Sadık
    
    // Davranış analizi
    public double AverageAppointmentInterval { get; set; } // Ortalama randevu aralığı (gün)
    public int PreferredServiceId { get; set; } // En çok tercih edilen hizmet
    public int PreferredStylistId { get; set; } // En çok tercih edilen kuaför
    public int PreferredBranchId { get; set; } // En çok tercih edilen şube
    
    // Zaman analizi
    public string PreferredDayOfWeek { get; set; } = "Pazartesi"; // En çok tercih edilen gün
    public string PreferredTimeSlot { get; set; } = "Sabah"; // Sabah, Öğle, Akşam
    
    // Memnuniyet
    public double AverageRating { get; set; } = 0; // Ortalama puan
    public int TotalRatings { get; set; } = 0; // Toplam puan sayısı
    
    // Finansal
    public decimal TotalSpent { get; set; } = 0; // Toplam harcama
    public decimal AverageTicketValue { get; set; } = 0; // Ortalama bilet değeri
    public decimal LifetimeValue { get; set; } = 0; // Yaşam boyu değer
    
    // Risk analizi
    public double ChurnRisk { get; set; } = 0; // Kayıp riski (0-100)
    public DateTime? LastActivityDate { get; set; } // Son aktivite tarihi
    public int DaysSinceLastVisit { get; set; } = 0; // Son ziyaretten bu yana geçen gün
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual Service? PreferredService { get; set; }
    public virtual Stylist? PreferredStylist { get; set; }
    public virtual Branch? PreferredBranch { get; set; }
}

// Müşteri segmentasyonu için yeni entity
public class CustomerSegment
{
    public int Id { get; set; }
    
    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty; // VIP, Sadık, Yeni, Risk
    
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;
    
    // Segment kriterleri
    public int? MinRecencyScore { get; set; }
    public int? MaxRecencyScore { get; set; }
    public int? MinFrequencyScore { get; set; }
    public int? MaxFrequencyScore { get; set; }
    public decimal? MinMonetaryScore { get; set; }
    public decimal? MaxMonetaryScore { get; set; }
    
    // Segment özellikleri
    [StringLength(20)]
    public string Color { get; set; } = "#007bff"; // Segment rengi
    public int Priority { get; set; } = 1; // Öncelik sırası
    
    // Kampanya ayarları
    public bool IsActive { get; set; } = true;
    public decimal? DiscountPercentage { get; set; } // Segment indirim oranı
    public int? BonusPoints { get; set; } // Bonus puan
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<CustomerAnalytics> CustomerAnalytics { get; set; } = new List<CustomerAnalytics>();
}

// Müşteri davranış takibi
public class CustomerBehavior
{
    public int Id { get; set; }
    
    [Required]
    public int CustomerId { get; set; }
    
    [Required, StringLength(50)]
    public string Action { get; set; } = string.Empty; // Appointment, Login, View, Purchase
    
    [StringLength(200)]
    public string? Details { get; set; } // Detay bilgisi
    
    [StringLength(100)]
    public string? PageUrl { get; set; } // Sayfa URL'i
    
    [StringLength(50)]
    public string? DeviceType { get; set; } // Mobile, Desktop, Tablet
    
    [StringLength(100)]
    public string? UserAgent { get; set; } // Tarayıcı bilgisi
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
}

// Müşteri tercihleri
public class CustomerPreference
{
    public int Id { get; set; }
    
    [Required]
    public int CustomerId { get; set; }
    
    [Required, StringLength(50)]
    public string PreferenceType { get; set; } = string.Empty; // Service, Stylist, Time, Day
    
    [Required, StringLength(100)]
    public string PreferenceValue { get; set; } = string.Empty; // Tercih değeri
    
    public int Weight { get; set; } = 1; // Tercih ağırlığı
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
}

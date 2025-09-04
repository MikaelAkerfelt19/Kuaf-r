using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

// Finansal işlem kategorileri
public class FinancialCategory
{
    public int Id { get; set; }
    
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required, StringLength(20)]
    public string Type { get; set; } = string.Empty; // Income, Expense, Asset, Liability
    
    [StringLength(20)]
    public string? SubType { get; set; } // Revenue, Cost, Operating, Capital
    
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    
    [StringLength(100)]
    public string? CreatedBy { get; set; }
    
    public int? ParentCategoryId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual FinancialCategory? ParentCategory { get; set; }
    
    // Navigation properties
    public virtual ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
}


// Personel performans takibi
public class StaffPerformance
{
    public int Id { get; set; }
    
    [Required]
    public int StylistId { get; set; }
    
    // Performans metrikleri
    public int TotalAppointments { get; set; } = 0; // Toplam randevu sayısı
    public int CompletedAppointments { get; set; } = 0; // Tamamlanan randevu sayısı
    public int CancelledAppointments { get; set; } = 0; // İptal edilen randevu sayısı
    public int NoShowAppointments { get; set; } = 0; // Gelmediği randevu sayısı
    
    // Finansal metrikler
    public decimal TotalRevenue { get; set; } = 0; // Toplam ciro
    public decimal AverageTicketValue { get; set; } = 0; // Ortalama bilet değeri
    public decimal CommissionEarned { get; set; } = 0; // Kazanılan komisyon
    
    // Kalite metrikleri
    public double AverageRating { get; set; } = 0; // Ortalama müşteri puanı
    public int TotalRatings { get; set; } = 0; // Toplam puan sayısı
    public int Complaints { get; set; } = 0; // Şikayet sayısı
    public int Compliments { get; set; } = 0; // Övgü sayısı
    
    // Verimlilik metrikleri
    public double AverageServiceTime { get; set; } = 0; // Ortalama hizmet süresi (dakika)
    public double UtilizationRate { get; set; } = 0; // Kullanım oranı (%)
    public int OvertimeHours { get; set; } = 0; // Fazla mesai saatleri
    
    // Dönemsel veriler
    public DateTime PeriodStart { get; set; } // Dönem başlangıcı
    public DateTime PeriodEnd { get; set; } // Dönem bitişi
    public string PeriodType { get; set; } = "Monthly"; // Daily, Weekly, Monthly, Yearly
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Stylist Stylist { get; set; } = null!;
}

// Personel maaş ve prim sistemi
public class StaffSalary
{
    public int Id { get; set; }
    
    [Required]
    public int StylistId { get; set; }
    
    // Maaş bilgileri
    public decimal BaseSalary { get; set; } = 0; // Temel maaş
    public decimal HourlyRate { get; set; } = 0; // Saatlik ücret
    public decimal CommissionRate { get; set; } = 0; // Komisyon oranı (%)
    
    // Prim sistemi
    public decimal PerformanceBonus { get; set; } = 0; // Performans primi
    public decimal SalesBonus { get; set; } = 0; // Satış primi
    public decimal QualityBonus { get; set; } = 0; // Kalite primi
    public decimal AttendanceBonus { get; set; } = 0; // Devam primi
    
    // Kesintiler
    public decimal TaxDeduction { get; set; } = 0; // Vergi kesintisi
    public decimal InsuranceDeduction { get; set; } = 0; // Sigorta kesintisi
    public decimal OtherDeductions { get; set; } = 0; // Diğer kesintiler
    
    // Ödeme bilgileri
    public decimal NetSalary { get; set; } = 0; // Net maaş
    public string PaymentMethod { get; set; } = "Bank Transfer"; // Ödeme yöntemi
    public string? BankAccount { get; set; } // Banka hesap bilgisi
    
    // Dönem bilgisi
    public DateTime PayPeriodStart { get; set; } // Ödeme dönemi başlangıcı
    public DateTime PayPeriodEnd { get; set; } // Ödeme dönemi bitişi
    public DateTime? PaidAt { get; set; } // Ödeme tarihi
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Stylist Stylist { get; set; } = null!;
}

// Personel eğitim takibi
public class StaffTraining
{
    public int Id { get; set; }
    
    [Required]
    public int StylistId { get; set; }
    
    [Required, StringLength(100)]
    public string TrainingName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty; // Alias for TrainingName
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required, StringLength(50)]
    public string TrainingType { get; set; } = string.Empty; // Technical, Safety, Customer Service, Product
    
    [StringLength(100)]
    public string? Trainer { get; set; } // Eğitmen
    
    [StringLength(100)]
    public string? Location { get; set; } // Eğitim yeri
    
    public DateTime TrainingDate { get; set; } // Eğitim tarihi
    public int DurationHours { get; set; } = 0; // Eğitim süresi (saat)
    public TimeSpan Duration { get; set; } = TimeSpan.Zero; // Alias for DurationHours
    
    [StringLength(20)]
    public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled, Failed
    
    public double? Score { get; set; } // Eğitim puanı
    public string? Certificate { get; set; } // Sertifika bilgisi
    
    [StringLength(500)]
    public string? Notes { get; set; } // Notlar
    
    [StringLength(100)]
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Stylist Stylist { get; set; } = null!;
}

// Personel devam takibi
public class StaffAttendance
{
    public int Id { get; set; }
    
    [Required]
    public int StylistId { get; set; }
    
    public DateTime Date { get; set; } // Tarih
    
    public DateTime? CheckInTime { get; set; } // Giriş saati
    public DateTime? CheckOutTime { get; set; } // Çıkış saati
    
    public int TotalHours { get; set; } = 0; // Toplam çalışma saati
    public int BreakHours { get; set; } = 0; // Mola saati
    public int OvertimeHours { get; set; } = 0; // Fazla mesai saati
    
    [StringLength(20)]
    public string Status { get; set; } = "Present"; // Present, Absent, Late, Early Leave, Sick Leave, Vacation
    
    [StringLength(200)]
    public string? Notes { get; set; } // Notlar
    
    [StringLength(100)]
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Stylist Stylist { get; set; } = null!;
}

// Personel değerlendirme sistemi
public class StaffEvaluation
{
    public int Id { get; set; }
    
    [Required]
    public int StylistId { get; set; }
    
    [Required]
    public int EvaluatorId { get; set; } // Değerlendiren kişi (Admin/Manager)
    
    [Required, StringLength(50)]
    public string EvaluationType { get; set; } = string.Empty; // Monthly, Quarterly, Annual, Performance Review
    
    // Değerlendirme kriterleri (1-5 puan)
    public double TechnicalSkills { get; set; } = 0; // Teknik beceriler
    public double CustomerService { get; set; } = 0; // Müşteri hizmetleri
    public double Teamwork { get; set; } = 0; // Takım çalışması
    public double Punctuality { get; set; } = 0; // Dakiklik
    public double Professionalism { get; set; } = 0; // Profesyonellik
    public double Communication { get; set; } = 0; // İletişim
    public double ProblemSolving { get; set; } = 0; // Problem çözme
    public double Adaptability { get; set; } = 0; // Uyum sağlama
    
    public double OverallScore { get; set; } = 0; // Genel puan
    
    [StringLength(1000)]
    public string? Strengths { get; set; } // Güçlü yönler
    
    [StringLength(1000)]
    public string? AreasForImprovement { get; set; } // Geliştirilmesi gereken alanlar
    
    [StringLength(1000)]
    public string? Goals { get; set; } // Hedefler
    
    [StringLength(1000)]
    public string? Comments { get; set; } // Yorumlar
    
    public DateTime EvaluationDate { get; set; } = DateTime.UtcNow;
    public DateTime? NextEvaluationDate { get; set; } // Sonraki değerlendirme tarihi
    
    [StringLength(100)]
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Stylist Stylist { get; set; } = null!;
    public virtual Stylist Evaluator { get; set; } = null!;
}

// Personel hedefleri
public class StaffGoal
{
    public int Id { get; set; }
    
    [Required]
    public int StylistId { get; set; }
    
    [Required, StringLength(100)]
    public string GoalName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required, StringLength(50)]
    public string GoalType { get; set; } = string.Empty; // Revenue, Appointments, Rating, Training, Sales
    
    public decimal TargetValue { get; set; } = 0; // Hedef değer
    public decimal CurrentValue { get; set; } = 0; // Mevcut değer
    public string Unit { get; set; } = ""; // Birim (₺, adet, puan, %)
    
    public DateTime StartDate { get; set; } // Başlangıç tarihi
    public DateTime EndDate { get; set; } // Bitiş tarihi
    
    [StringLength(20)]
    public string Status { get; set; } = "Active"; // Active, Completed, Failed, Cancelled
    
    public double ProgressPercentage { get; set; } = 0; // İlerleme yüzdesi
    
    [StringLength(500)]
    public string? Notes { get; set; } // Notlar
    
    [StringLength(100)]
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Stylist Stylist { get; set; } = null!;
}

using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Entities.Analytics;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class CustomerAnalyticsService : ICustomerAnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly IAppointmentService _appointmentService;

    public CustomerAnalyticsService(ApplicationDbContext context, IAppointmentService appointmentService)
    {
        _context = context;
        _appointmentService = appointmentService;
    }

    public async Task<CustomerAnalytics> CalculateRFMAsync(int customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null) throw new ArgumentException("Müşteri bulunamadı");

        var appointments = await _appointmentService.GetByCustomerAsync(customerId);
        var now = DateTime.UtcNow;

        // Recency: Son randevudan bu yana geçen gün sayısı
        var lastAppointment = appointments.OrderByDescending(a => a.StartAt).FirstOrDefault();
        var recency = lastAppointment != null ? (now - lastAppointment.StartAt).Days : 999;

        // Frequency: Toplam randevu sayısı
        var frequency = appointments.Count();

        // Monetary: Toplam harcama
        var monetary = appointments.Sum(a => a.FinalPrice);

        var analytics = new CustomerAnalytics
        {
            CustomerId = customerId,
            RecencyScore = recency,
            FrequencyScore = frequency,
            MonetaryScore = monetary,
            TotalSpent = monetary,
            AverageTicketValue = frequency > 0 ? monetary / frequency : 0,
            LastActivityDate = lastAppointment?.StartAt,
            DaysSinceLastVisit = recency,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Segment hesaplama
        analytics.Segment = CalculateSegment(recency, frequency, monetary);
        analytics.LifecycleStage = CalculateLifecycleStage(recency, frequency);
        analytics.ChurnRisk = await CalculateChurnRiskAsync(customerId);

        // Tercih analizi
        await AnalyzePreferencesAsync(analytics, appointments);

        return analytics;
    }

    public async Task<List<CustomerAnalytics>> CalculateAllRFMAsync()
    {
        var customers = await _context.Customers.ToListAsync();
        var analyticsList = new List<CustomerAnalytics>();

        foreach (var customer in customers)
        {
            try
            {
                var analytics = await CalculateRFMAsync(customer.Id);
                analyticsList.Add(analytics);
            }
            catch (Exception ex)
            {
                // Log error and continue
                Console.WriteLine($"RFM hesaplama hatası - Müşteri ID: {customer.Id}, Hata: {ex.Message}");
            }
        }

        return analyticsList;
    }

    public async Task UpdateCustomerSegmentAsync(int customerId)
    {
        // Geçici olarak devre dışı - CustomerAnalytics tablosu yok
        await Task.CompletedTask;
    }

    public async Task<List<CustomerSegment>> GetCustomerSegmentsAsync()
    {
        return await _context.CustomerSegments
            .Where(cs => cs.IsActive)
            .OrderBy(cs => cs.Priority)
            .ToListAsync();
    }

    public async Task<CustomerSegment> CreateSegmentAsync(CustomerSegment segment)
    {
        segment.CreatedAt = DateTime.UtcNow;
        _context.CustomerSegments.Add(segment);
        await _context.SaveChangesAsync();
        return segment;
    }

    public async Task<CustomerSegment> UpdateSegmentAsync(CustomerSegment segment)
    {
        segment.UpdatedAt = DateTime.UtcNow;
        _context.CustomerSegments.Update(segment);
        await _context.SaveChangesAsync();
        return segment;
    }

    public async Task<bool> DeleteSegmentAsync(int segmentId)
    {
        var segment = await _context.CustomerSegments.FindAsync(segmentId);
        if (segment == null) return false;

        _context.CustomerSegments.Remove(segment);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<CustomerAnalytics?> GetCustomerAnalyticsAsync(int customerId)
    {
        // Geçici olarak null döndür - CustomerAnalytics tablosu yok
        return Task.FromResult<CustomerAnalytics?>(null);
    }

    public Task<List<CustomerAnalytics>> GetCustomersBySegmentAsync(string segment)
    {
        // Geçici olarak boş liste döndür - CustomerAnalytics tablosu yok
        return Task.FromResult(new List<CustomerAnalytics>());
    }

    public Task<List<CustomerAnalytics>> GetHighValueCustomersAsync(int count = 10)
    {
        // Geçici olarak boş liste döndür - CustomerAnalytics tablosu yok
        return Task.FromResult(new List<CustomerAnalytics>());
    }

    public Task<List<CustomerAnalytics>> GetAtRiskCustomersAsync(int count = 10)
    {
        // Geçici olarak boş liste döndür - CustomerAnalytics tablosu yok
        return Task.FromResult(new List<CustomerAnalytics>());
    }

    public async Task RecordCustomerBehaviorAsync(int customerId, string action, string? details = null, string? pageUrl = null)
    {
        var behavior = new CustomerBehavior
        {
            CustomerId = customerId,
            Action = action,
            Details = details,
            PageUrl = pageUrl,
            Timestamp = DateTime.UtcNow
        };

        _context.CustomerBehaviors.Add(behavior);
        await _context.SaveChangesAsync();
    }

    public async Task<List<CustomerBehavior>> GetCustomerBehaviorsAsync(int customerId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.CustomerBehaviors
            .Where(cb => cb.CustomerId == customerId);

        if (from.HasValue)
            query = query.Where(cb => cb.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(cb => cb.Timestamp <= to.Value);

        return await query
            .OrderByDescending(cb => cb.Timestamp)
            .ToListAsync();
    }

    public async Task<List<CustomerBehavior>> GetPopularActionsAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _context.CustomerBehaviors.AsQueryable();

        if (from.HasValue)
            query = query.Where(cb => cb.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(cb => cb.Timestamp <= to.Value);

        return await query
            .GroupBy(cb => cb.Action)
            .Select(g => new CustomerBehavior
            {
                Action = g.Key,
                Timestamp = g.Max(cb => cb.Timestamp)
            })
            .OrderByDescending(cb => cb.Timestamp)
            .ToListAsync();
    }

    public async Task<List<CustomerPreference>> GetCustomerPreferencesAsync(int customerId)
    {
        return await _context.CustomerPreferences
            .Where(cp => cp.CustomerId == customerId)
            .OrderByDescending(cp => cp.Weight)
            .ToListAsync();
    }

    public async Task UpdateCustomerPreferenceAsync(int customerId, string preferenceType, string preferenceValue, int weight = 1)
    {
        var preference = await _context.CustomerPreferences
            .FirstOrDefaultAsync(cp => cp.CustomerId == customerId && cp.PreferenceType == preferenceType);

        if (preference == null)
        {
            preference = new CustomerPreference
            {
                CustomerId = customerId,
                PreferenceType = preferenceType,
                PreferenceValue = preferenceValue,
                Weight = weight,
                CreatedAt = DateTime.UtcNow
            };
            _context.CustomerPreferences.Add(preference);
        }
        else
        {
            preference.PreferenceValue = preferenceValue;
            preference.Weight = weight;
            preference.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public Task<List<CustomerAnalytics>> GetChurnRiskCustomersAsync(double riskThreshold = 70)
    {
        // Geçici olarak boş liste döndür - CustomerAnalytics tablosu yok
        return Task.FromResult(new List<CustomerAnalytics>());
    }

    public Task<double> CalculateChurnRiskAsync(int customerId)
    {
        // Geçici olarak 0 döndür - CustomerAnalytics tablosu yok
        return Task.FromResult(0.0);
    }

    public async Task<CustomerAnalyticsReport> GetAnalyticsReportAsync()
    {
        var customers = await _context.Customers.ToListAsync();
        // var analytics = await _context.CustomerAnalytics.ToListAsync(); // Geçici olarak kapatıldı
        var appointments = await _appointmentService.GetAllAsync();

        var totalRevenue = appointments.Sum(a => a.FinalPrice);
        var activeCustomers = customers.Count(c => c.CreatedAt >= DateTime.UtcNow.AddDays(-30)); // Geçici hesaplama
        var newCustomers = customers.Count(c => c.CreatedAt >= DateTime.UtcNow.AddDays(-30));
        var atRiskCustomers = 0; // Geçici olarak 0

        return new CustomerAnalyticsReport
        {
            TotalCustomers = customers.Count,
            ActiveCustomers = activeCustomers,
            NewCustomers = newCustomers,
            AtRiskCustomers = atRiskCustomers,
            TotalRevenue = totalRevenue,
            AverageCustomerValue = customers.Any() ? totalRevenue / customers.Count : 0,
            AverageChurnRisk = 0, // Geçici olarak 0
            SegmentDistribution = new List<SegmentDistribution>(), // Geçici olarak boş
            TopCustomers = new List<TopCustomer>() // Geçici olarak boş
        };
    }

    public Task<List<SegmentPerformance>> GetSegmentPerformanceAsync()
    {
        // Geçici olarak boş liste döndür - CustomerAnalytics tablosu yok
        return Task.FromResult(new List<SegmentPerformance>());
    }

    public async Task<List<CustomerJourney>> GetCustomerJourneyAsync(int customerId)
    {
        var appointments = await _appointmentService.GetByCustomerAsync(customerId);
        var behaviors = await GetCustomerBehaviorsAsync(customerId);

        var journey = new List<CustomerJourney>();

        // Randevuları journey'ye ekle
        foreach (var appointment in appointments.OrderBy(a => a.StartAt))
        {
            journey.Add(new CustomerJourney
            {
                Date = appointment.StartAt,
                Action = "Appointment",
                Description = $"{appointment.Service.Name} - {appointment.Stylist.FirstName} {appointment.Stylist.LastName}",
                ServiceName = appointment.Service.Name,
                StylistName = $"{appointment.Stylist.FirstName} {appointment.Stylist.LastName}",
                Amount = appointment.FinalPrice,
                Rating = appointment.CustomerRating
            });
        }

        // Davranışları journey'ye ekle
        foreach (var behavior in behaviors.OrderBy(b => b.Timestamp))
        {
            journey.Add(new CustomerJourney
            {
                Date = behavior.Timestamp,
                Action = behavior.Action,
                Description = behavior.Details ?? behavior.Action
            });
        }

        return journey.OrderBy(j => j.Date).ToList();
    }

    private string CalculateSegment(int recency, int frequency, decimal monetary)
    {
        // Basit segmentasyon algoritması
        if (recency <= 30 && frequency >= 10 && monetary >= 2000)
            return "VIP";
        else if (recency <= 60 && frequency >= 5 && monetary >= 1000)
            return "Altın";
        else if (recency <= 90 && frequency >= 3 && monetary >= 500)
            return "Gümüş";
        else if (recency <= 180 && frequency >= 1)
            return "Bronz";
        else
            return "Risk";
    }

    private string CalculateLifecycleStage(int recency, int frequency)
    {
        if (frequency == 0)
            return "Yeni";
        else if (recency <= 30)
            return "Aktif";
        else if (recency <= 90)
            return "Risk";
        else if (recency <= 180)
            return "Kayıp";
        else
            return "Uyuyan";
    }

    private Task AnalyzePreferencesAsync(CustomerAnalytics analytics, IEnumerable<Models.Entities.Appointment> appointments)
    {
        if (!appointments.Any()) return Task.CompletedTask;

        // En çok tercih edilen hizmet
        var preferredService = appointments
            .GroupBy(a => a.ServiceId)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
        if (preferredService != null)
            analytics.PreferredServiceId = preferredService.Key;

        // En çok tercih edilen kuaför
        var preferredStylist = appointments
            .GroupBy(a => a.StylistId)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
        if (preferredStylist != null)
            analytics.PreferredStylistId = preferredStylist.Key;

        // En çok tercih edilen şube
        var preferredBranch = appointments
            .GroupBy(a => a.BranchId)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
        if (preferredBranch != null)
            analytics.PreferredBranchId = preferredBranch.Key;

        // En çok tercih edilen gün
        var preferredDay = appointments
            .GroupBy(a => a.StartAt.DayOfWeek)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
        if (preferredDay != null)
            analytics.PreferredDayOfWeek = preferredDay.Key.ToString();

        // En çok tercih edilen saat dilimi
        var timeSlots = appointments.Select(a => GetTimeSlot(a.StartAt.Hour)).ToList();
        var preferredTimeSlot = timeSlots
            .GroupBy(ts => ts)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
        if (preferredTimeSlot != null)
            analytics.PreferredTimeSlot = preferredTimeSlot.Key;

        // Ortalama puan
        var ratings = appointments.Where(a => a.CustomerRating.HasValue).Select(a => a.CustomerRating!.Value).ToList();
        if (ratings.Any())
        {
            analytics.AverageRating = ratings.Average();
            analytics.TotalRatings = ratings.Count;
        }
        
        return Task.CompletedTask;
    }

    private string GetTimeSlot(int hour)
    {
        return hour switch
        {
            >= 6 and < 12 => "Sabah",
            >= 12 and < 18 => "Öğle",
            _ => "Akşam"
        };
    }

    private List<SegmentDistribution> GetSegmentDistribution(List<CustomerAnalytics> analytics)
    {
        var total = analytics.Count;
        if (total == 0) return new List<SegmentDistribution>();

        return analytics
            .GroupBy(ca => ca.Segment)
            .Select(g => new SegmentDistribution
            {
                Segment = g.Key,
                Count = g.Count(),
                Percentage = $"{g.Count() * 100.0 / total:F1}%",
                Revenue = g.Sum(ca => ca.MonetaryScore)
            })
            .OrderByDescending(sd => sd.Count)
            .ToList();
    }

    private List<TopCustomer> GetTopCustomers(List<CustomerAnalytics> analytics)
    {
        return analytics
            .OrderByDescending(ca => ca.MonetaryScore)
            .Take(10)
            .Select(ca => new TopCustomer
            {
                CustomerId = ca.CustomerId,
                CustomerName = $"{ca.Customer.FirstName} {ca.Customer.LastName}",
                Segment = ca.Segment,
                TotalSpent = ca.MonetaryScore,
                AppointmentCount = ca.FrequencyScore,
                ChurnRisk = ca.ChurnRisk,
                LastVisit = ca.LastActivityDate ?? DateTime.MinValue
            })
            .ToList();
    }

    public async Task<List<AnalyticsCustomerSegment>> GetCustomerSegmentationAsync()
    {
        // Müşteri segmentlerini getirir (VIP, Sadık, Risk, Yeni vs.)
        var customers = await _context.Customers.Include(c => c.Appointments).ToListAsync();
        var segments = new List<AnalyticsCustomerSegment>();
        
        foreach (var customer in customers)
        {
            try
            {
                // Basit segmentasyon logic'i - CustomerAnalytics tablosu olmadan
                var appointments = customer.Appointments;
                var totalSpent = appointments.Sum(a => a.FinalPrice);
                var lastVisit = appointments.OrderByDescending(a => a.StartAt).FirstOrDefault()?.StartAt;
                var daysSinceLastVisit = lastVisit.HasValue ? (DateTime.Now - lastVisit.Value).Days : 999;
                
                string segment = "Yeni";
                string lifecycleStage = "New";
                double riskScore = 0;
                
                // Segment belirleme
                if (totalSpent > 1000 && appointments.Count() > 5)
                {
                    segment = "VIP";
                    lifecycleStage = "Champion";
                    riskScore = daysSinceLastVisit > 30 ? 50 : 10;
                }
                else if (totalSpent > 500 && appointments.Count() > 3)
                {
                    segment = "Sadık";
                    lifecycleStage = "Loyal";
                    riskScore = daysSinceLastVisit > 45 ? 60 : 20;
                }
                else if (daysSinceLastVisit > 90)
                {
                    segment = "Risk";
                    lifecycleStage = "AtRisk";
                    riskScore = 80;
                }
                else if (appointments.Count() == 0)
                {
                    segment = "Yeni";
                    lifecycleStage = "New";
                    riskScore = 30;
                }
                else
                {
                    segment = "Potansiyel";
                    lifecycleStage = "Potential";
                    riskScore = daysSinceLastVisit > 60 ? 70 : 40;
                }
                
                segments.Add(new AnalyticsCustomerSegment
                {
                    CustomerId = customer.Id,
                    CustomerName = customer.FullName,
                    Segment = segment,
                    LifecycleStage = lifecycleStage,
                    TotalSpent = totalSpent,
                    LastVisitDate = lastVisit,
                    RiskScore = riskScore
                });
            }
            catch (Exception ex)
            {
                // Hata durumunda varsayılan değerlerle devam et
                Console.WriteLine($"Customer segmentation error for ID {customer.Id}: {ex.Message}");
                segments.Add(new AnalyticsCustomerSegment
                {
                    CustomerId = customer.Id,
                    CustomerName = customer.FullName,
                    Segment = "Belirsiz",
                    LifecycleStage = "Unknown",
                    TotalSpent = 0,
                    LastVisitDate = null,
                    RiskScore = 50
                });
            }
        }
        
        return segments.OrderByDescending(s => s.TotalSpent).ToList();
    }

    public async Task<List<Customer>> GetCustomersAtRiskAsync()
    {
        // Risk altındaki müşterileri getirir - CustomerAnalytics tablosu olmadan
        var customers = await _context.Customers.Include(c => c.Appointments).ToListAsync();
        var riskCustomers = new List<Customer>();
        
        foreach (var customer in customers)
        {
            try
            {
                var appointments = customer.Appointments;
                var lastVisit = appointments.OrderByDescending(a => a.StartAt).FirstOrDefault()?.StartAt;
                var daysSinceLastVisit = lastVisit.HasValue ? (DateTime.Now - lastVisit.Value).Days : 999;
                var totalAppointments = appointments.Count();
                
                // Risk kriterleri
                bool isAtRisk = false;
                
                // 60 günden fazla gelmemiş
                if (daysSinceLastVisit > 60 && totalAppointments > 0)
                    isAtRisk = true;
                
                // Hiç randevu almamış ama 30 günden fazla kayıtlı
                if (totalAppointments == 0 && (DateTime.Now - customer.CreatedAt).Days > 30)
                    isAtRisk = true;
                
                // Eskiden düzenli gelip son 90 günde gelmemiş
                if (totalAppointments >= 3 && daysSinceLastVisit > 90)
                    isAtRisk = true;
                
                if (isAtRisk)
                {
                    riskCustomers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Risk analysis error for customer ID {customer.Id}: {ex.Message}");
            }
        }
        
        return riskCustomers.OrderByDescending(c => c.CreatedAt).ToList();
    }

    public async Task<CustomerLifetimeValue> CalculateCustomerLTVAsync(int customerId)
    {
        // Müşteri yaşam boyu değerini hesaplar
        var appointments = await _context.Appointments
            .Where(a => a.CustomerId == customerId)
            .ToListAsync();
        
        if (!appointments.Any()) return new CustomerLifetimeValue { CustomerId = customerId, LTV = 0 };
        
        var totalSpent = appointments.Sum(a => a.FinalPrice);
        var avgOrderValue = appointments.Average(a => a.FinalPrice);
        var appointmentFrequency = CalculateAppointmentFrequency(appointments);
        var customerLifespan = CalculateCustomerLifespan(appointments);
        
        var ltv = (double)avgOrderValue * appointmentFrequency * customerLifespan;
        
        return new CustomerLifetimeValue
        {
            CustomerId = customerId,
            LTV = (decimal)ltv,
            AverageOrderValue = avgOrderValue,
            AppointmentFrequency = appointmentFrequency,
            CustomerLifespan = customerLifespan
        };
    }

    public async Task<List<CustomerBehaviorPattern>> AnalyzeCustomerBehaviorAsync()
    {
        // Müşteri davranış kalıplarını analiz eder
        var patterns = new List<CustomerBehaviorPattern>();
        var customers = await _context.Customers.Include(c => c.Appointments).ToListAsync();
        
        foreach (var customer in customers)
        {
            if (!customer.Appointments.Any()) continue;
            
            var pattern = new CustomerBehaviorPattern
            {
                CustomerId = customer.Id,
                PreferredDayOfWeek = GetPreferredDayOfWeek(customer.Appointments.ToList()),
                PreferredTimeSlot = GetPreferredTimeSlot(customer.Appointments.ToList()),
                AverageAppointmentInterval = CalculateAverageInterval(customer.Appointments.ToList()),
                SeasonalTrends = AnalyzeSeasonalTrends(customer.Appointments.ToList())
            };
            
            patterns.Add(pattern);
        }
        
        return patterns;
    }

    // Yardımcı metodlar
    private double CalculateAppointmentFrequency(List<Appointment> appointments)
    {
        // Randevu sıklığını hesaplar (yıllık)
        if (appointments.Count < 2) return 1;
        
        var firstAppointment = appointments.OrderBy(a => a.StartAt).First();
        var lastAppointment = appointments.OrderByDescending(a => a.StartAt).First();
        var daysDiff = (lastAppointment.StartAt - firstAppointment.StartAt).TotalDays;
        
        if (daysDiff == 0) return appointments.Count;
        
        return (appointments.Count / daysDiff) * 365;
    }

    private double CalculateCustomerLifespan(List<Appointment> appointments)
    {
        // Müşteri yaşam süresini hesaplar (yıl)
        if (appointments.Count < 2) return 1;
        
        var firstAppointment = appointments.OrderBy(a => a.StartAt).First();
        var lastAppointment = appointments.OrderByDescending(a => a.StartAt).First();
        var daysDiff = (lastAppointment.StartAt - firstAppointment.StartAt).TotalDays;
        
        return Math.Max(daysDiff / 365.0, 0.1); // Minimum 0.1 yıl
    }

    private double CalculateAverageInterval(List<Appointment> appointments)
    {
        // Ortalama randevu aralığını hesaplar
        if (appointments.Count < 2) return 0;
        
        var sortedAppointments = appointments.OrderBy(a => a.StartAt).ToList();
        var intervals = new List<double>();
        
        for (int i = 1; i < sortedAppointments.Count; i++)
        {
            var interval = (sortedAppointments[i].StartAt - sortedAppointments[i - 1].StartAt).TotalDays;
            intervals.Add(interval);
        }
        
        return intervals.Average();
    }

    private string GetPreferredDayOfWeek(List<Appointment> appointments)
    {
        // En çok tercih edilen günü bulur
        var dayGroups = appointments.GroupBy(a => a.StartAt.DayOfWeek);
        var preferredDay = dayGroups.OrderByDescending(g => g.Count()).FirstOrDefault();
        
        return preferredDay?.Key.ToString() ?? "Bilinmiyor";
    }

    private string GetPreferredTimeSlot(List<Appointment> appointments)
    {
        // En çok tercih edilen zaman dilimini bulur
        var timeSlots = appointments.Select(a => 
        {
            var hour = a.StartAt.Hour;
            if (hour < 12) return "Sabah";
            if (hour < 17) return "Öğle";
            return "Akşam";
        }).GroupBy(x => x);
        
        var preferredSlot = timeSlots.OrderByDescending(g => g.Count()).FirstOrDefault();
        return preferredSlot?.Key ?? "Bilinmiyor";
    }

    private string AnalyzeSeasonalTrends(List<Appointment> appointments)
    {
        // Mevsimsel trendleri analiz eder
        var seasonGroups = appointments.GroupBy(a => 
        {
            var month = a.StartAt.Month;
            if (month >= 3 && month <= 5) return "İlkbahar";
            if (month >= 6 && month <= 8) return "Yaz";
            if (month >= 9 && month <= 11) return "Sonbahar";
            return "Kış";
        });
        
        var preferredSeason = seasonGroups.OrderByDescending(g => g.Count()).FirstOrDefault();
        return preferredSeason?.Key ?? "Bilinmiyor";
    }
}



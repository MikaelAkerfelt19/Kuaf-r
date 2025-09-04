using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface ICustomerAnalyticsService
{
    // RFM Analizi
    Task<CustomerAnalytics> CalculateRFMAsync(int customerId);
    Task<List<CustomerAnalytics>> CalculateAllRFMAsync();
    Task UpdateCustomerSegmentAsync(int customerId);
    
    // Segmentasyon
    Task<List<CustomerSegment>> GetCustomerSegmentsAsync();
    Task<CustomerSegment> CreateSegmentAsync(CustomerSegment segment);
    Task<CustomerSegment> UpdateSegmentAsync(CustomerSegment segment);
    Task<bool> DeleteSegmentAsync(int segmentId);
    
    // Müşteri analizi
    Task<CustomerAnalytics?> GetCustomerAnalyticsAsync(int customerId);
    Task<List<CustomerAnalytics>> GetCustomersBySegmentAsync(string segment);
    Task<List<CustomerAnalytics>> GetHighValueCustomersAsync(int count = 10);
    Task<List<CustomerAnalytics>> GetAtRiskCustomersAsync(int count = 10);
    
    // Davranış analizi
    Task RecordCustomerBehaviorAsync(int customerId, string action, string? details = null, string? pageUrl = null);
    Task<List<CustomerBehavior>> GetCustomerBehaviorsAsync(int customerId, DateTime? from = null, DateTime? to = null);
    Task<List<CustomerBehavior>> GetPopularActionsAsync(DateTime? from = null, DateTime? to = null);
    
    // Tercih analizi
    Task<List<CustomerPreference>> GetCustomerPreferencesAsync(int customerId);
    Task UpdateCustomerPreferenceAsync(int customerId, string preferenceType, string preferenceValue, int weight = 1);
    
    // Churn analizi
    Task<List<CustomerAnalytics>> GetChurnRiskCustomersAsync(double riskThreshold = 70);
    Task<double> CalculateChurnRiskAsync(int customerId);
    
    // Raporlar
    Task<CustomerAnalyticsReport> GetAnalyticsReportAsync();
    Task<List<SegmentPerformance>> GetSegmentPerformanceAsync();
    Task<List<CustomerJourney>> GetCustomerJourneyAsync(int customerId);
}

public class CustomerAnalyticsReport
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int NewCustomers { get; set; }
    public int AtRiskCustomers { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageCustomerValue { get; set; }
    public double AverageChurnRisk { get; set; }
    public List<SegmentDistribution> SegmentDistribution { get; set; } = new();
    public List<TopCustomer> TopCustomers { get; set; } = new();
}

public class SegmentPerformance
{
    public string SegmentName { get; set; } = "";
    public int CustomerCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageValue { get; set; }
    public double AverageChurnRisk { get; set; }
    public double GrowthRate { get; set; }
}

public class CustomerJourney
{
    public DateTime Date { get; set; }
    public string Action { get; set; } = "";
    public string Description { get; set; } = "";
    public string? ServiceName { get; set; }
    public string? StylistName { get; set; }
    public decimal? Amount { get; set; }
    public int? Rating { get; set; }
}

public class SegmentDistribution
{
    public string Segment { get; set; } = "";
    public int Count { get; set; }
    public string Percentage { get; set; } = "0%";
    public decimal Revenue { get; set; }
}

public class TopCustomer
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public string Segment { get; set; } = "";
    public decimal TotalSpent { get; set; }
    public int AppointmentCount { get; set; }
    public double ChurnRisk { get; set; }
    public DateTime LastVisit { get; set; }
}

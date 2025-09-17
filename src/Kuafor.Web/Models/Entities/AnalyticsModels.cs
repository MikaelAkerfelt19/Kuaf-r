using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities.Analytics
{
    // Zaman dilimi modeli  
    public class TimeSlot
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public string? StylistName { get; set; }
        public int? StylistId { get; set; }
    }

    // Randevu istatistikleri modeli
    public class AppointmentStatistics
    {
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageAppointmentValue { get; set; }
        public double CompletionRate => TotalAppointments > 0 ? (CompletedAppointments * 100.0 / TotalAppointments) : 0;
        public double CancellationRate => TotalAppointments > 0 ? (CancelledAppointments * 100.0 / TotalAppointments) : 0;
    }

    // Müşteri yaşam boyu değeri modeli
    public class CustomerLifetimeValue
    {
        public int CustomerId { get; set; }
        public decimal LTV { get; set; }
        public decimal AverageOrderValue { get; set; }
        public double AppointmentFrequency { get; set; }
        public double CustomerLifespan { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual Customer? Customer { get; set; }
    }

    // Müşteri davranış kalıbı modeli
    public class CustomerBehaviorPattern
    {
        public int CustomerId { get; set; }
        public string PreferredDayOfWeek { get; set; } = string.Empty;
        public string PreferredTimeSlot { get; set; } = string.Empty;
        public double AverageAppointmentInterval { get; set; }
        public string SeasonalTrends { get; set; } = string.Empty;
        public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
        
        public virtual Customer? Customer { get; set; }
    }

    // Müşteri segmenti modeli - Analytics namespace'i için
    public class AnalyticsCustomerSegment
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Segment { get; set; } = string.Empty;
        public string LifecycleStage { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public DateTime? LastVisitDate { get; set; }
        public double RiskScore { get; set; }
        
        public virtual Customer? Customer { get; set; }
    }

    // Günlük satış modeli
    public class DailySales
    {
        public DateTime Date { get; set; }
        public decimal TotalSales { get; set; }
        public int AppointmentCount { get; set; }
        public decimal AverageTicketValue => AppointmentCount > 0 ? TotalSales / AppointmentCount : 0;
    }

    // Hizmet performansı modeli
    public class ServicePerformance
    {
        public string ServiceName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int AppointmentCount { get; set; }
        public decimal AveragePrice { get; set; }
        public double PopularityPercentage { get; set; }
    }

    // Nakit akış raporu modeli
    public class CashFlowReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetCashFlow { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
    }

    // Finansal rapor modeli - Analytics namespace'i için
    public class AnalyticsFinancialReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public int TotalAppointments { get; set; }
        public decimal AverageTicketValue { get; set; }
        public decimal ProfitMargin => TotalRevenue > 0 ? (NetProfit / TotalRevenue) * 100 : 0;
    }

    // Mesaj raporu modeli
    public class MessageReport
    {
        public int TotalSent { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalFailed { get; set; }
        public double DeliveryRate { get; set; }
        public DateTime ReportDate { get; set; } = DateTime.UtcNow;
        public string MessageType { get; set; } = string.Empty;
    }
}

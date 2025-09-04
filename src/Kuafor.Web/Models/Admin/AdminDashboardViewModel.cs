namespace Kuafor.Web.Models.Admin
{
    public class KpiVm
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string? SubText { get; set; }
        public string? Badge { get; set; } // "+12%" gibi
        public string Icon { get; set; } = "bi bi-graph-up";
        public string Color { get; set; } = "primary";
    }

    public class AdminDashboardViewModel
    {
        public List<KpiVm> Kpis { get; set; } = new();
        public List<UpcomingApptRow> Upcoming { get; set; } = new();
        public List<TopServiceRow> TopServices { get; set; } = new();
        
        // Yeni gelişmiş özellikler
        public List<ChartDataPoint> RevenueChart { get; set; } = new();
        public List<ChartDataPoint> AppointmentChart { get; set; } = new();
        public List<StylistPerformance> TopStylists { get; set; } = new();
        public List<BranchPerformance> BranchPerformances { get; set; } = new();
        public List<CustomerSegment> CustomerSegments { get; set; } = new();
        public List<RecentActivity> RecentActivities { get; set; } = new();
        public List<InventoryAlert> InventoryAlerts { get; set; } = new();
        public List<FinancialSummary> FinancialSummaries { get; set; } = new();
        
        // Trend analizi
        public TrendAnalysis RevenueTrend { get; set; } = new();
        public TrendAnalysis AppointmentTrend { get; set; } = new();
        public TrendAnalysis CustomerTrend { get; set; } = new();
    }

    public record UpcomingApptRow(int Id, DateTime When, string Customer, string Service, string Stylist, string Branch);
    public record TopServiceRow(string Service, int Count, string? Trend);
    
    // Yeni gelişmiş modeller
    public class ChartDataPoint
    {
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
        public string? Color { get; set; }
    }
    
    public class StylistPerformance
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int AppointmentCount { get; set; }
        public decimal Revenue { get; set; }
        public double Rating { get; set; }
        public string Trend { get; set; } = "+0%";
    }
    
    public class BranchPerformance
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int AppointmentCount { get; set; }
        public decimal Revenue { get; set; }
        public int ActiveStylists { get; set; }
        public double UtilizationRate { get; set; }
    }
    
    public class CustomerSegment
    {
        public string Segment { get; set; } = "";
        public int Count { get; set; }
        public decimal Revenue { get; set; }
        public string Percentage { get; set; } = "0%";
    }
    
    public class RecentActivity
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "bi bi-info-circle";
        public string Color { get; set; } = "primary";
    }
    
    public class InventoryAlert
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public string AlertLevel { get; set; } = "warning"; // info, warning, danger
    }
    
    public class FinancialSummary
    {
        public string Period { get; set; } = "";
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
    }
    
    public class TrendAnalysis
    {
        public decimal CurrentValue { get; set; }
        public decimal PreviousValue { get; set; }
        public decimal ChangePercentage { get; set; }
        public string Trend { get; set; } = "stable"; // up, down, stable
        public string TrendIcon { get; set; } = "bi bi-dash";
    }
}
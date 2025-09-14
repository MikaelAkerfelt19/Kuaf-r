namespace Kuafor.Web.Models.Admin.Reports
{
    // Dashboard Raporu
    public class DashboardReport
    {
        public string Period { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int ActiveStylists { get; set; }
        public int ActiveBranches { get; set; }
        public decimal AverageTicketValue { get; set; }
        public double ConversionRate { get; set; }
        public List<TopService> TopServices { get; set; } = new();
        public List<TopStylist> TopStylists { get; set; } = new();
        public ChartData RevenueByDay { get; set; } = new();
        public List<AppointmentStatusDistribution> AppointmentStatusDistribution { get; set; } = new();
    }

    // Gelir Raporu
    public class RevenueReport
    {
        public string Period { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalAppointments { get; set; }
        public decimal AverageTicketValue { get; set; }
        public List<BranchRevenue> RevenueByBranch { get; set; } = new();
        public List<StylistRevenue> RevenueByStylist { get; set; } = new();
        public List<ServiceRevenue> RevenueByService { get; set; } = new();
        public ChartData RevenueByDay { get; set; } = new();
        public ChartData RevenueByHour { get; set; } = new();
    }

    // Randevu Raporu
    public class AppointmentReport
    {
        public string Period { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int NoShowAppointments { get; set; }
        public double AverageAppointmentsPerDay { get; set; }
        public List<PeakHour> PeakHours { get; set; } = new();
        public List<AppointmentStatusDistribution> AppointmentStatusDistribution { get; set; } = new();
        public List<AppointmentsByBranch> AppointmentsByBranch { get; set; } = new();
        public List<AppointmentsByStylist> AppointmentsByStylist { get; set; } = new();
        public ChartData AppointmentsByDay { get; set; } = new();
        public ChartData AppointmentsByHour { get; set; } = new();
    }

    // Müşteri Raporu
    public class CustomerReport
    {
        public string Period { get; set; } = string.Empty;
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public double AverageAppointmentsPerCustomer { get; set; }
        public double CustomerRetentionRate { get; set; }
        public List<TopCustomer> TopCustomers { get; set; } = new();
        public List<CustomerSegment> CustomerSegments { get; set; } = new();
        public double CustomerLifetimeValue { get; set; }
    }

    // Kuaför Raporu
    public class StylistReport
    {
        public string Period { get; set; } = string.Empty;
        public int TotalStylists { get; set; }
        public int ActiveStylists { get; set; }
        public double AverageRating { get; set; }
        public List<TopPerformer> TopPerformers { get; set; } = new();
        public List<StylistUtilization> StylistUtilization { get; set; } = new();
        public List<StylistRevenue> RevenueByStylist { get; set; } = new();
        public List<AppointmentsByStylist> AppointmentsByStylist { get; set; } = new();
        public List<CustomerSatisfaction> CustomerSatisfaction { get; set; } = new();
    }

    // Şube Raporu
    public class BranchReport
    {
        public string Period { get; set; } = string.Empty;
        public int TotalBranches { get; set; }
        public int ActiveBranches { get; set; }
        public List<BranchPerformance> BranchPerformance { get; set; } = new();
        public List<BranchRevenue> RevenueByBranch { get; set; } = new();
        public List<AppointmentsByBranch> AppointmentsByBranch { get; set; } = new();
        public List<BranchUtilization> BranchUtilization { get; set; } = new();
    }

    // Grafik Verisi
    public class ChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<double> Values { get; set; } = new();
        public string Title { get; set; } = string.Empty;
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
    }

    // Karşılaştırma Raporları
    public class ComparisonReport
    {
        public string CurrentPeriod { get; set; } = string.Empty;
        public string PreviousPeriod { get; set; } = string.Empty;
        public ComparisonMetric RevenueComparison { get; set; } = new();
        public ComparisonMetric AppointmentComparison { get; set; } = new();
        public ComparisonMetric AverageTicketValueComparison { get; set; } = new();
    }

    public class ComparisonMetric
    {
        public double Current { get; set; }
        public double Previous { get; set; }
        public double Change { get; set; }
        public double ChangePercentage { get; set; }
    }

    public class YearOverYearReport
    {
        public int Year { get; set; }
        public decimal CurrentYearRevenue { get; set; }
        public decimal PreviousYearRevenue { get; set; }
        public int CurrentYearAppointments { get; set; }
        public int PreviousYearAppointments { get; set; }
        public double RevenueGrowth { get; set; }
        public double AppointmentGrowth { get; set; }
        public List<MonthlyComparison> MonthlyComparison { get; set; } = new();
    }

    public class MonthlyComparison
    {
        public int Month { get; set; }
        public decimal CurrentYearRevenue { get; set; }
        public decimal PreviousYearRevenue { get; set; }
        public int CurrentYearAppointments { get; set; }
        public int PreviousYearAppointments { get; set; }
    }

    // Detay Model'leri
    public class TopService
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopStylist
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class BranchRevenue
    {
        public string BranchName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int AppointmentCount { get; set; }
        public double Percentage { get; set; }
    }

    public class StylistRevenue
    {
        public string StylistName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int AppointmentCount { get; set; }
        public double AverageRating { get; set; }
        public double Percentage { get; set; }
    }

    public class ServiceRevenue
    {
        public string ServiceName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int AppointmentCount { get; set; }
        public double Percentage { get; set; }
    }

    public class AppointmentStatusDistribution
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class PeakHour
    {
        public int Hour { get; set; }
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class AppointmentsByBranch
    {
        public string BranchName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class AppointmentsByStylist
    {
        public string StylistName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class TopCustomer
    {
        public string CustomerName { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastAppointment { get; set; }
    }

    public class CustomerSegment
    {
        public string SegmentName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class TopPerformer
    {
        public string StylistName { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
        public decimal Revenue { get; set; }
        public double Rating { get; set; }
    }

    public class StylistUtilization
    {
        public string StylistName { get; set; } = string.Empty;
        public double TotalHours { get; set; }
        public double BookedHours { get; set; }
        public double UtilizationRate { get; set; }
    }

    public class CustomerSatisfaction
    {
        public string StylistName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalAppointments { get; set; }
        public int SatisfiedCustomers { get; set; }
    }

    public class BranchPerformance
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AppointmentCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
    }

    public class BranchUtilization
    {
        public string BranchName { get; set; } = string.Empty;
        public double TotalHours { get; set; }
        public double BookedHours { get; set; }
        public double UtilizationRate { get; set; }
    }

    // Rapor Şablonları
    public class ReportTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class CustomReport
    {
        public string ReportType { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public object Data { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }
}
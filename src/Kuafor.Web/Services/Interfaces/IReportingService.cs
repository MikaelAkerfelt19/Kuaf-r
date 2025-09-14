using Kuafor.Web.Models.Admin.Reports;

namespace Kuafor.Web.Services.Interfaces
{
    public interface IReportingService
    {
        // Dashboard Raporları
        Task<DashboardReport> GetDashboardReportAsync(DateTime? from = null, DateTime? to = null);
        Task<RevenueReport> GetRevenueReportAsync(DateTime from, DateTime to);
        Task<AppointmentReport> GetAppointmentReportAsync(DateTime from, DateTime to);
        Task<CustomerReport> GetCustomerReportAsync(DateTime from, DateTime to);
        Task<StylistReport> GetStylistReportAsync(DateTime from, DateTime to);
        Task<BranchReport> GetBranchReportAsync(DateTime from, DateTime to);
        
        // Grafik Verileri
        Task<ChartData> GetRevenueChartDataAsync(DateTime from, DateTime to, string period = "daily");
        Task<ChartData> GetAppointmentChartDataAsync(DateTime from, DateTime to, string period = "daily");
        Task<ChartData> GetCustomerChartDataAsync(DateTime from, DateTime to, string period = "daily");
        
        // Karşılaştırma Raporları
        Task<ComparisonReport> GetPeriodComparisonAsync(DateTime currentFrom, DateTime currentTo, DateTime previousFrom, DateTime previousTo);
        Task<YearOverYearReport> GetYearOverYearReportAsync(int year);
        
        // Export İşlemleri
        Task<byte[]> ExportToExcel<T>(IEnumerable<T> data, string fileName, string sheetName = "Rapor");
        Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, string fileName, string title);
        Task<byte[]> ExportToCsv<T>(IEnumerable<T> data, string fileName);
        
        // Özel Raporlar
        Task<CustomReport> GenerateCustomReportAsync(string reportType, Dictionary<string, object> parameters);
        Task<List<ReportTemplate>> GetReportTemplatesAsync();
        Task<ReportTemplate> SaveReportTemplateAsync(ReportTemplate template);
    }
}
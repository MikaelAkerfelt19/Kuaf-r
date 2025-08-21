namespace Kuafor.Web.Models.Admin
{
    public class KpiVm
    {
        public string Label { get; set; } = "";
        public string Value { get; set; } = "";
        public string? SubText { get; set; }
        public string? Badge { get; set; } // "+12%" gibi
    }

    public class AdminDashboardViewModel
    {
        public List<KpiVm> Kpis { get; set; } = new();
        public List<UpcomingApptRow> Upcoming { get; set; } = new();
        public List<TopServiceRow> TopServices { get; set; } = new();
    }

    public record UpcomingApptRow(int Id, DateTime When, string Customer, string Service, string Stylist, string Branch);
    public record TopServiceRow(string Service, int Count, string? Trend);
}
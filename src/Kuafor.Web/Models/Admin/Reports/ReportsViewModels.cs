using Kuafor.Web.Models.Entities;
using CustomerEntity = Kuafor.Web.Models.Entities.Customer;

namespace Kuafor.Web.Models.Admin.Reports
{
    public class ReportsIndexViewModel
    {
        public List<ReportCategory> Categories { get; set; } = new();
        public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime ToDate { get; set; } = DateTime.Today;
    }

    public class ReportCategory
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
    }

    public class AppointmentReportsViewModel
    {
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Appointment> RecentAppointments { get; set; } = new();
        public List<StylistPerformance> StylistPerformance { get; set; } = new();
    }

    public class StylistPerformance
    {
        public string StylistName { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
    }

    public class CustomerReportsViewModel
    {
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public List<CustomerEntity> TopCustomers { get; set; } = new();
        public List<CustomerGrowthData> GrowthData { get; set; } = new();
    }

    public class CustomerGrowthData
    {
        public string Month { get; set; } = string.Empty;
        public int NewCustomers { get; set; }
        public int TotalCustomers { get; set; }
    }

    public class RevenueReportsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal DailyRevenue { get; set; }
        public List<RevenueData> MonthlyData { get; set; } = new();
        public List<RevenueServiceData> ServiceRevenues { get; set; } = new();
    }

    public class RevenueData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int AppointmentCount { get; set; }
    }

    public class RevenueServiceData
    {
        public string ServiceName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int AppointmentCount { get; set; }
        public decimal AveragePrice { get; set; }
    }
}

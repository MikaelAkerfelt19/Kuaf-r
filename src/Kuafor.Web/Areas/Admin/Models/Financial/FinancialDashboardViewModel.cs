using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Models.Admin.Financial
{
    public class FinancialDashboardViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalAppointments { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal AverageTicketValue => TotalAppointments > 0 ? TotalRevenue / TotalAppointments : 0;
        public decimal NetProfit => TotalRevenue - TotalExpenses;
        public List<MonthlyRevenue> MonthlyData { get; set; } = new();
        public List<ExpenseCategory> ExpenseCategories { get; set; } = new();
    }

    public class MonthlyRevenue
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int AppointmentCount { get; set; }
    }

    public class ExpenseCategory
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }
}

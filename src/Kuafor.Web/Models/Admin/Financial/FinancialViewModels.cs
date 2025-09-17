using Kuafor.Web.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Areas.Admin.Models
{
    public class FinancialDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        
        public List<MonthlyFinancialData> MonthlyData { get; set; } = new();
        public List<ExpenseCategory> ExpenseCategories { get; set; } = new();
        public List<RevenueSource> RevenueSources { get; set; } = new();
        
        public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime ToDate { get; set; } = DateTime.Today;
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Today;
        
        public int TotalTransactions { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public int TotalAppointments { get; set; }
        public decimal AverageTicketValue { get; set; }
    }

    public class CashFlowViewModel
    {
        public List<CashFlowEntry> CashFlowEntries { get; set; } = new();
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal TotalInflow { get; set; }
        public decimal TotalOutflow { get; set; }
        public decimal NetCashFlow { get; set; }
        
        public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime ToDate { get; set; } = DateTime.Today;
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Today;
        
        // Ek property'ler view'da kullanılan
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        
        public List<CashFlowCategory> Categories { get; set; } = new();
        public string SelectedCategory { get; set; } = "All";
    }

    public class ExpenseTrackingViewModel
    {
        public List<Expense> Expenses { get; set; } = new();
        public List<ExpenseCategory> Categories { get; set; } = new();
        public decimal TotalExpenses { get; set; }
        public decimal BudgetLimit { get; set; }
        public decimal RemainingBudget { get; set; }
        
        public DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime ToDate { get; set; } = DateTime.Today;
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime EndDate { get; set; } = DateTime.Today;
        public string SelectedCategory { get; set; } = "All";
        
        public ExpenseFormViewModel NewExpense { get; set; } = new();
    }

    public class ExpenseFormViewModel
    {
        [Required(ErrorMessage = "Açıklama gereklidir")]
        [StringLength(200, ErrorMessage = "Açıklama 200 karakterden uzun olamaz")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Tutar gereklidir")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0'dan büyük olmalıdır")]
        public decimal Amount { get; set; }
        
        [Required(ErrorMessage = "Kategori seçimi gereklidir")]
        public int CategoryId { get; set; }
        
        [Required(ErrorMessage = "Tarih gereklidir")]
        public DateTime Date { get; set; } = DateTime.Today;
        
        [StringLength(500, ErrorMessage = "Notlar 500 karakterden uzun olamaz")]
        public string? Notes { get; set; }
        
        public string? ReceiptPath { get; set; }
    }

    // Supporting models
    public class MonthlyFinancialData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal Profit { get; set; }
    }

    public class ExpenseCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal BudgetLimit { get; set; }
        public decimal CurrentSpent { get; set; }
        public string Color { get; set; } = "#007bff";
    }

    public class RevenueSource
    {
        public string Source { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public string Color { get; set; } = "#28a745";
    }

    public class CashFlowEntry
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Inflow" or "Outflow"
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal RunningBalance { get; set; }
    }

    public class CashFlowCategory
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class Expense
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? ReceiptPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IFinancialAnalyticsService
{
    // Maliyet analizi
    Task<CostAnalysis?> GetCostAnalysisAsync(int serviceId, int productId, DateTime startDate, DateTime endDate);
    Task<List<CostAnalysis>> GetAllCostAnalysesAsync(DateTime startDate, DateTime endDate);
    Task<CostAnalysis> CreateCostAnalysisAsync(CostAnalysis analysis);
    
    // Bütçe yönetimi
    Task<Budget?> GetBudgetAsync(int year, int? month, int? quarter);
    Task<List<Budget>> GetAllBudgetsAsync();
    Task<Budget> CreateBudgetAsync(Budget budget);
    Task<Budget> UpdateBudgetAsync(Budget budget);
    Task<bool> DeleteBudgetAsync(int budgetId);
    
    // Bütçe kalemleri
    Task<List<BudgetItem>> GetBudgetItemsAsync(int budgetId);
    Task<BudgetItem> CreateBudgetItemAsync(BudgetItem item);
    Task<BudgetItem> UpdateBudgetItemAsync(BudgetItem item);
    Task<bool> DeleteBudgetItemAsync(int itemId);
    
    // Nakit akış takibi
    Task<CashFlow?> GetCashFlowAsync(DateTime date);
    Task<List<CashFlow>> GetCashFlowRangeAsync(DateTime startDate, DateTime endDate);
    Task<CashFlow> CreateCashFlowAsync(CashFlow cashFlow);
    Task<CashFlow> UpdateCashFlowAsync(CashFlow cashFlow);
    
    // Finansal raporlar
    Task<List<FinancialReport>> GetFinancialReportsAsync(string reportType, DateTime startDate, DateTime endDate);
    Task<FinancialReport> CreateFinancialReportAsync(FinancialReport report);
    Task<FinancialReport> GenerateProfitLossReportAsync(DateTime startDate, DateTime endDate);
    Task<FinancialReport> GenerateCashFlowReportAsync(DateTime startDate, DateTime endDate);
    Task<FinancialReport> GenerateBudgetReportAsync(int budgetId);
    
    // Finansal kategoriler
    Task<List<FinancialCategory>> GetFinancialCategoriesAsync(string type);
    Task<FinancialCategory> CreateFinancialCategoryAsync(FinancialCategory category);
    Task<FinancialCategory> UpdateFinancialCategoryAsync(FinancialCategory category);
    Task<bool> DeleteFinancialCategoryAsync(int categoryId);
}

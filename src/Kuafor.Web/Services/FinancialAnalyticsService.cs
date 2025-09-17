using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Entities.Analytics;
using Kuafor.Web.Models.Enums;
using Kuafor.Web.Services.Interfaces;
using System.Text.Json;

namespace Kuafor.Web.Services;

public class FinancialAnalyticsService : IFinancialAnalyticsService
{
    private readonly ApplicationDbContext _context;

    public FinancialAnalyticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CostAnalysis?> GetCostAnalysisAsync(int serviceId, int productId, DateTime startDate, DateTime endDate)
    {
        return await _context.CostAnalyses
            .Include(ca => ca.Service)
            .Include(ca => ca.Product)
            .FirstOrDefaultAsync(ca => ca.ServiceId == serviceId && 
                                      ca.ProductId == productId && 
                                      ca.PeriodStart >= startDate && 
                                      ca.PeriodEnd <= endDate);
    }

    public async Task<List<CostAnalysis>> GetAllCostAnalysesAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.CostAnalyses
            .Include(ca => ca.Service)
            .Include(ca => ca.Product)
            .Where(ca => ca.PeriodStart >= startDate && ca.PeriodEnd <= endDate)
            .ToListAsync();
    }

    public async Task<CostAnalysis> CreateCostAnalysisAsync(CostAnalysis analysis)
    {
        analysis.CreatedAt = DateTime.UtcNow;
        _context.CostAnalyses.Add(analysis);
        await _context.SaveChangesAsync();
        return analysis;
    }

    public async Task<Budget?> GetBudgetAsync(int year, int? month, int? quarter)
    {
        return await _context.Budgets
            .Include(b => b.BudgetItems)
            .FirstOrDefaultAsync(b => b.Year == year && 
                                     b.Month == month && 
                                     b.Quarter == quarter);
    }

    public async Task<List<Budget>> GetAllBudgetsAsync()
    {
        return await _context.Budgets
            .Include(b => b.BudgetItems)
            .OrderByDescending(b => b.Year)
            .ThenByDescending(b => b.Month)
            .ToListAsync();
    }

    public async Task<Budget> CreateBudgetAsync(Budget budget)
    {
        budget.CreatedAt = DateTime.UtcNow;
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();
        return budget;
    }

    public async Task<Budget> UpdateBudgetAsync(Budget budget)
    {
        budget.UpdatedAt = DateTime.UtcNow;
        _context.Budgets.Update(budget);
        await _context.SaveChangesAsync();
        return budget;
    }

    public async Task<bool> DeleteBudgetAsync(int budgetId)
    {
        var budget = await _context.Budgets
            .FirstOrDefaultAsync(b => b.Id == budgetId);

        if (budget == null) return false;

        _context.Budgets.Remove(budget);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<BudgetItem>> GetBudgetItemsAsync(int budgetId)
    {
        return await _context.BudgetItems
            .Include(bi => bi.Category)
            .Where(bi => bi.BudgetId == budgetId)
            .ToListAsync();
    }

    public async Task<BudgetItem> CreateBudgetItemAsync(BudgetItem item)
    {
        item.CreatedAt = DateTime.UtcNow;
        _context.BudgetItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<BudgetItem> UpdateBudgetItemAsync(BudgetItem item)
    {
        item.UpdatedAt = DateTime.UtcNow;
        _context.BudgetItems.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteBudgetItemAsync(int itemId)
    {
        var item = await _context.BudgetItems
            .FirstOrDefaultAsync(bi => bi.Id == itemId);

        if (item == null) return false;

        _context.BudgetItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CashFlow?> GetCashFlowAsync(DateTime date)
    {
        return await _context.CashFlows
            .FirstOrDefaultAsync(cf => cf.Date.Date == date.Date);
    }

    public async Task<List<CashFlow>> GetCashFlowRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.CashFlows
            .Where(cf => cf.Date >= startDate && cf.Date <= endDate)
            .OrderBy(cf => cf.Date)
            .ToListAsync();
    }

    public async Task<CashFlow> CreateCashFlowAsync(CashFlow cashFlow)
    {
        cashFlow.CreatedAt = DateTime.UtcNow;
        _context.CashFlows.Add(cashFlow);
        await _context.SaveChangesAsync();
        return cashFlow;
    }

    public async Task<CashFlow> UpdateCashFlowAsync(CashFlow cashFlow)
    {
        cashFlow.UpdatedAt = DateTime.UtcNow;
        _context.CashFlows.Update(cashFlow);
        await _context.SaveChangesAsync();
        return cashFlow;
    }

    public async Task<List<FinancialReport>> GetFinancialReportsAsync(string reportType, DateTime startDate, DateTime endDate)
    {
        return await _context.FinancialReports
            .Where(fr => fr.ReportType == reportType && 
                        fr.PeriodStart >= startDate && 
                        fr.PeriodEnd <= endDate)
            .OrderByDescending(fr => fr.ReportDate)
            .ToListAsync();
    }

    public async Task<FinancialReport> CreateFinancialReportAsync(FinancialReport report)
    {
        report.CreatedAt = DateTime.UtcNow;
        _context.FinancialReports.Add(report);
        await _context.SaveChangesAsync();
        return report;
    }

    public async Task<AnalyticsFinancialReport> GenerateProfitLossReportAsync(DateTime startDate, DateTime endDate)
    {
        // Gelir hesaplama
        var revenue = await _context.Appointments
            .Where(a => a.StartAt >= startDate && a.StartAt <= endDate && a.Status == AppointmentStatus.Completed)
            .SumAsync(a => a.FinalPrice);

        // Gider hesaplama (örnek)
        var expenses = await _context.FinancialTransactions
            .Where(ft => ft.TransactionDate >= startDate && 
                        ft.TransactionDate <= endDate && 
                        ft.Type == "Expense")
            .SumAsync(ft => ft.Amount);

        var profit = revenue - expenses;

        var reportData = new
        {
            Period = new { Start = startDate, End = endDate },
            Revenue = revenue,
            Expenses = expenses,
            Profit = profit,
            ProfitMargin = revenue > 0 ? (profit / revenue) * 100 : 0
        };

        var report = new FinancialReport
        {
            Name = $"Kar-Zarar Raporu - {startDate:MM/yyyy}",
            Description = $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} dönemi kar-zarar raporu",
            ReportType = "P&L",
            ReportDate = DateTime.UtcNow,
            PeriodStart = startDate,
            PeriodEnd = endDate,
            Data = JsonSerializer.Serialize(reportData),
            Format = "JSON",
            CreatedBy = "System"
        };

        await CreateFinancialReportAsync(report);
        
        // AnalyticsFinancialReport'a dönüştür ve döndür
        return JsonSerializer.Deserialize<AnalyticsFinancialReport>(report.Data) ?? new AnalyticsFinancialReport();
    }

    public async Task<AnalyticsFinancialReport> GenerateCashFlowReportAsync(DateTime startDate, DateTime endDate)
    {
        var cashFlows = await GetCashFlowRangeAsync(startDate, endDate);
        
        var totalCashIn = cashFlows.Sum(cf => cf.CashIn);
        var totalCardIn = cashFlows.Sum(cf => cf.CardIn);
        var totalTransferIn = cashFlows.Sum(cf => cf.TransferIn);
        var totalOtherIn = cashFlows.Sum(cf => cf.OtherIn);
        
        var totalCashOut = cashFlows.Sum(cf => cf.CashOut);
        var totalCardOut = cashFlows.Sum(cf => cf.CardOut);
        var totalTransferOut = cashFlows.Sum(cf => cf.TransferOut);
        var totalOtherOut = cashFlows.Sum(cf => cf.OtherOut);
        
        var netCashFlow = (totalCashIn + totalCardIn + totalTransferIn + totalOtherIn) - 
                         (totalCashOut + totalCardOut + totalTransferOut + totalOtherOut);

        var reportData = new
        {
            Period = new { Start = startDate, End = endDate },
            CashIn = new { Cash = totalCashIn, Card = totalCardIn, Transfer = totalTransferIn, Other = totalOtherIn },
            CashOut = new { Cash = totalCashOut, Card = totalCardOut, Transfer = totalTransferOut, Other = totalOtherOut },
            NetCashFlow = netCashFlow,
            DailyFlows = cashFlows.Select(cf => new { Date = cf.Date, NetFlow = cf.NetCashFlow })
        };

        var report = new FinancialReport
        {
            Name = $"Nakit Akış Raporu - {startDate:MM/yyyy}",
            Description = $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} dönemi nakit akış raporu",
            ReportType = "Cash Flow",
            ReportDate = DateTime.UtcNow,
            PeriodStart = startDate,
            PeriodEnd = endDate,
            Data = JsonSerializer.Serialize(reportData),
            Format = "JSON",
            CreatedBy = "System"
        };

        await CreateFinancialReportAsync(report);
        
        // AnalyticsFinancialReport'a dönüştür ve döndür
        return JsonSerializer.Deserialize<AnalyticsFinancialReport>(report.Data) ?? new AnalyticsFinancialReport();
    }

    public async Task<AnalyticsFinancialReport> GenerateBudgetReportAsync(int budgetId)
    {
        var budget = await _context.Budgets
            .Include(b => b.BudgetItems)
            .FirstOrDefaultAsync(b => b.Id == budgetId);

        if (budget == null) return null!;

        var reportData = new
        {
            Budget = new
            {
                Id = budget.Id,
                Name = budget.Name,
                Type = budget.Type,
                Year = budget.Year,
                Month = budget.Month,
                Quarter = budget.Quarter
            },
            Budgeted = new
            {
                Revenue = budget.RevenueBudget,
                Expense = budget.ExpenseBudget,
                Profit = budget.ProfitBudget
            },
            Actual = new
            {
                Revenue = budget.ActualRevenue,
                Expense = budget.ActualExpense,
                Profit = budget.ActualProfit
            },
            Variance = new
            {
                Revenue = budget.RevenueVariance,
                Expense = budget.ExpenseVariance,
                Profit = budget.ProfitVariance
            },
            Items = budget.BudgetItems.Select(bi => new
            {
                Name = bi.Name,
                Type = bi.Type,
                Budgeted = bi.BudgetedAmount,
                Actual = bi.ActualAmount,
                Variance = bi.Variance,
                VariancePercentage = bi.VariancePercentage
            })
        };

        var report = new FinancialReport
        {
            Name = $"Bütçe Raporu - {budget.Name}",
            Description = $"{budget.Name} bütçe raporu",
            ReportType = "Budget",
            ReportDate = DateTime.UtcNow,
            PeriodStart = new DateTime(budget.Year, budget.Month ?? 1, 1),
            PeriodEnd = new DateTime(budget.Year, budget.Month ?? 12, DateTime.DaysInMonth(budget.Year, budget.Month ?? 12)),
            Data = JsonSerializer.Serialize(reportData),
            Format = "JSON",
            CreatedBy = "System"
        };

        await CreateFinancialReportAsync(report);
        
        // AnalyticsFinancialReport'a dönüştür ve döndür  
        return JsonSerializer.Deserialize<AnalyticsFinancialReport>(report.Data) ?? new AnalyticsFinancialReport();
    }

    public async Task<List<FinancialCategory>> GetFinancialCategoriesAsync(string type)
    {
        return await _context.FinancialCategories
            .Where(fc => fc.Type == type && fc.IsActive)
            .OrderBy(fc => fc.DisplayOrder)
            .ToListAsync();
    }

    public async Task<FinancialCategory> CreateFinancialCategoryAsync(FinancialCategory category)
    {
        category.CreatedAt = DateTime.UtcNow;
        _context.FinancialCategories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<FinancialCategory> UpdateFinancialCategoryAsync(FinancialCategory category)
    {
        category.UpdatedAt = DateTime.UtcNow;
        _context.FinancialCategories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> DeleteFinancialCategoryAsync(int categoryId)
    {
        var category = await _context.FinancialCategories
            .FirstOrDefaultAsync(fc => fc.Id == categoryId);

        if (category == null) return false;

        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<FinancialReport> GenerateFinancialReportAsync(DateTime startDate, DateTime endDate)
    {
        // Belirtilen tarih aralığında finansal rapor oluşturur
        var appointments = await _context.Appointments
            .Where(a => a.StartAt >= startDate && a.StartAt <= endDate)
            .ToListAsync();

        var transactions = await _context.FinancialTransactions
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .ToListAsync();

        var analyticsReport = new AnalyticsFinancialReport
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalRevenue = appointments.Sum(a => a.FinalPrice),
            TotalExpenses = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount),
            NetProfit = appointments.Sum(a => a.FinalPrice) - transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount),
            TotalAppointments = appointments.Count,
            AverageTicketValue = appointments.Any() ? appointments.Average(a => a.FinalPrice) : 0
        };
        
        // FinancialReport entity'sine dönüştür
        return new FinancialReport
        {
            Name = "Genel Finansal Rapor",
            Description = $"{startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd} arası finansal rapor",
            ReportType = "General",
            ReportDate = DateTime.UtcNow,
            PeriodStart = startDate,
            PeriodEnd = endDate,
            Data = JsonSerializer.Serialize(analyticsReport),
            Format = "JSON",
            CreatedBy = "System"
        };
    }

    public async Task<List<DailySales>> GetDailySalesAsync(DateTime startDate, DateTime endDate)
    {
        // Günlük satış verilerini getirir
        var dailySales = await _context.Appointments
            .Where(a => a.StartAt >= startDate && a.StartAt <= endDate)
            .GroupBy(a => a.StartAt.Date)
            .Select(g => new DailySales
            {
                Date = g.Key,
                TotalSales = g.Sum(a => a.FinalPrice),
                AppointmentCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        return dailySales;
    }

    public async Task<List<ServicePerformance>> GetServicePerformanceAsync(DateTime startDate, DateTime endDate)
    {
        // Hizmet performans verilerini getirir
        var servicePerformance = await _context.Appointments
            .Include(a => a.Service)
            .Where(a => a.StartAt >= startDate && a.StartAt <= endDate)
            .GroupBy(a => a.Service)
            .Select(g => new ServicePerformance
            {
                ServiceName = g.Key.Name,
                TotalRevenue = g.Sum(a => a.FinalPrice),
                AppointmentCount = g.Count(),
                AveragePrice = g.Average(a => a.FinalPrice)
            })
            .OrderByDescending(s => s.TotalRevenue)
            .ToListAsync();

        return servicePerformance;
    }

    public async Task<CashFlowReport> GetCashFlowAsync(DateTime startDate, DateTime endDate)
    {
        // Nakit akış raporunu getirir
        var income = await _context.FinancialTransactions
            .Where(t => t.Type == "Income" && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .SumAsync(t => t.Amount);

        var expenses = await _context.FinancialTransactions
            .Where(t => t.Type == "Expense" && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .SumAsync(t => t.Amount);

        return new CashFlowReport
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalIncome = income,
            TotalExpenses = expenses,
            NetCashFlow = income - expenses
        };
    }
}

using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class LoyaltyService : ILoyaltyService
{
    private readonly ApplicationDbContext _context;
    
    public LoyaltyService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Loyalty?> GetByCustomerAsync(int customerId)
    {
        var loyalty = await _context.Loyalties
            .FirstOrDefaultAsync(l => l.CustomerId == customerId);
            
        if (loyalty == null)
        {
            // Yeni loyalty oluştur
            loyalty = new Loyalty
            {
                CustomerId = customerId,
                Points = 0,
                Tier = "Bronz",
                TotalSpent = 0,
                AppointmentCount = 0
            };
            
            _context.Loyalties.Add(loyalty);
            await _context.SaveChangesAsync();
        }
        
        return loyalty;
    }

    public async Task<Loyalty?> GetByCustomerIdAsync(int customerId)
    {
        return await _context.Loyalties
            .FirstOrDefaultAsync(l => l.CustomerId == customerId);
    }
    
    public async Task<int> AddPointsAsync(int customerId, int points, string reason)
    {
        var loyalty = await GetByCustomerAsync(customerId);
        if (loyalty != null)
        {
            loyalty.Points += points;
            loyalty.LastActivity = DateTime.UtcNow;
            
            // Transaction kaydı ekle
            var transaction = new LoyaltyTransaction
            {
                CustomerId = customerId,
                Points = points,
                Reason = reason,
                Type = points > 0 ? "Earned" : "Spent"
            };
            
            _context.LoyaltyTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            
            // Tier güncelle
            loyalty.Tier = await CalculateTierAsync(loyalty.Points);
            await _context.SaveChangesAsync();
            
            return loyalty.Points;
        }
        
        return 0;
    }
    
    public Task<string> CalculateTierAsync(int points)
    {
        // TODO: Bu değerler admin panelinden ayarlanabilir olmalı
        // Şimdilik sabit değerler kullanılıyor
        var tier = points switch
        {
            >= 1000 => "Platin",  // 1000+ puan
            >= 500 => "Altın",    // 500-999 puan
            >= 200 => "Gümüş",    // 200-499 puan
            _ => "Bronz"          // 0-199 puan
        };
        
        return Task.FromResult(tier);
    }
    
    public async Task<IEnumerable<LoyaltyTransaction>> GetTransactionHistoryAsync(int customerId)
    {
        return await _context.LoyaltyTransactions
            .Where(lt => lt.CustomerId == customerId)
            .OrderByDescending(lt => lt.CreatedAt)
            .ToListAsync();
    }
    
    public async Task UpdateAppointmentCountAsync(int customerId)
    {
        var loyalty = await GetByCustomerAsync(customerId);
        if (loyalty != null)
        {
            loyalty.AppointmentCount++;
            loyalty.LastActivity = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task UpdateTotalSpentAsync(int customerId, decimal amount)
    {
        var loyalty = await GetByCustomerAsync(customerId);
        if (loyalty != null)
        {
            loyalty.TotalSpent += (int)amount;
            loyalty.LastActivity = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}

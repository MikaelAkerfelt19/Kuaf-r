using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface ILoyaltyService
{
    Task<Loyalty?> GetByCustomerAsync(int customerId);
    Task<Loyalty?> GetByCustomerIdAsync(int customerId);
    Task<int> AddPointsAsync(int customerId, int points, string reason);
    Task<string> CalculateTierAsync(int points);
    Task<IEnumerable<LoyaltyTransaction>> GetTransactionHistoryAsync(int customerId);
    Task UpdateAppointmentCountAsync(int customerId);
    Task UpdateTotalSpentAsync(int customerId, decimal amount);
}

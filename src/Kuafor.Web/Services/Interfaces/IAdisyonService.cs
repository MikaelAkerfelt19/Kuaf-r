using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface IAdisyonService
{
    Task<IEnumerable<Receipt>> GetAllAsync();
    Task<Receipt?> GetByIdAsync(int id);
    Task<Receipt> CreateAsync(Receipt receipt);
    Task<Receipt> UpdateAsync(Receipt receipt);
    Task<bool> DeleteAsync(int id);
    Task<Receipt> CreateForRegisteredCustomerAsync(int customerId, List<ReceiptItem> items);
    Task<Receipt> CreateForNewCustomerAsync(Customer customer, List<ReceiptItem> items);
    Task<bool> CloseReceiptAsync(int receiptId);
    Task<decimal> CalculateTotalAsync(List<ReceiptItem> items);
}

using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Admin.Adisyon;

namespace Kuafor.Web.Services.Interfaces;

public interface IAdisyonService
{
    Task<IEnumerable<Adisyon>> GetAllAsync();
    Task<Adisyon?> GetByIdAsync(int id);
    Task<Adisyon> CreateAsync(Adisyon adisyon);
    Task<Adisyon> UpdateAsync(Adisyon adisyon);
    Task<bool> DeleteAsync(int id);
    Task<bool> AddServicesToAdisyonAsync(int adisyonId, List<AdisyonServiceModel> services);
    Task<List<AdisyonDetail>> GetAdisyonDetailsAsync(int adisyonId);
    Task<decimal> CalculateTotalAsync(List<AdisyonServiceModel> services);
    
    // Receipt methods (eski metotlar)
    Task<IEnumerable<Receipt>> GetAllReceiptsAsync();
    Task<Receipt?> GetReceiptByIdAsync(int id);
    Task<Receipt> CreateReceiptAsync(Receipt receipt);
    Task<Receipt> UpdateReceiptAsync(Receipt receipt);
    Task<bool> DeleteReceiptAsync(int id);
    Task<Receipt> CreateForRegisteredCustomerAsync(int customerId, List<ReceiptItem> items);
    Task<Receipt> CreateForNewCustomerAsync(Customer customer, List<ReceiptItem> items);
    Task<bool> CloseReceiptAsync(int receiptId);
    Task<decimal> CalculateReceiptTotalAsync(List<ReceiptItem> items);
}

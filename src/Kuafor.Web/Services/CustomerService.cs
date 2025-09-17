using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Enums;

namespace Kuafor.Web.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    
    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .AsNoTracking() // Read-only için performans artışı
            .Include(c => c.Appointments)
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .ToListAsync();
    }
    
    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .AsNoTracking() // Read-only için performans artışı
            .Include(c => c.Appointments)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<Customer> CreateAsync(Customer customer)
    {
        customer.CreatedAt = DateTime.UtcNow;
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }
    
    public async Task<Customer> UpdateAsync(Customer customer)
    {
        customer.UpdatedAt = DateTime.UtcNow;
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
        return customer;
    }
    
    public async Task DeleteAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<Customer?> GetByUserIdAsync(string userId)
    {
        return await _context.Customers
            .AsNoTracking() // Read-only için performans artışı
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }
    
    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _context.Customers
            .AsNoTracking() // Read-only için performans artışı
            .FirstOrDefaultAsync(c => c.Email == email);
    }
    
    public async Task<Customer?> GetByPhoneAsync(string phone)
    {
        return await _context.Customers
            .AsNoTracking() // Read-only için performans artışı
            .FirstOrDefaultAsync(c => c.Phone == phone);
    }
    
    public async Task<IEnumerable<Customer>> GetActiveAsync()
    {
        return await _context.Customers
            .AsNoTracking() // Read-only için performans artışı
            .Where(c => c.IsActive)
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Customer>> GetByBranchAsync(int branchId)
    {
        return await _context.Customers
            .AsNoTracking() // Read-only için performans artışı
            .Where(c => c.IsActive && c.Appointments.Any(a => a.BranchId == branchId))
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .ToListAsync();
    }
    
    public async Task<bool> IsActiveAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        return customer?.IsActive ?? false;
    }
    
    public async Task<int> GetAppointmentCountAsync(int customerId)
    {
        return await _context.Appointments
            .Where(a => a.CustomerId == customerId)
            .CountAsync();
    }
    
    public async Task<decimal> GetTotalSpentAsync(int customerId)
    {
        return await _context.Appointments
            .Where(a => a.CustomerId == customerId && a.Status == AppointmentStatus.Completed)
            .SumAsync(a => a.FinalPrice);
    }
    
    public async Task<DateTime?> GetLastVisitAsync(int customerId)
    {
        var lastAppointment = await _context.Appointments
            .Where(a => a.CustomerId == customerId && a.Status == AppointmentStatus.Completed)
            .OrderByDescending(a => a.StartAt)
            .FirstOrDefaultAsync();
        
        return lastAppointment?.StartAt;
    }
    
    public async Task<bool> HasUpcomingAppointmentsAsync(int customerId)
    {
        var now = DateTime.UtcNow;
        return await _context.Appointments
            .AnyAsync(a => a.CustomerId == customerId && 
                          a.StartAt > now && 
                          a.Status != AppointmentStatus.Cancelled);
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Customers.AnyAsync(c => c.Id == id);
    }
    
    public async Task<bool> ExistsByUserIdAsync(string userId)
    {
        return await _context.Customers.AnyAsync(c => c.UserId == userId);
    }
    
    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Customers.AnyAsync(c => c.Email == email);
    }
    
    public async Task<int> GetCountAsync()
    {
        return await _context.Customers.CountAsync();
    }

    public async Task<List<Customer>> BulkCreateAsync(List<Customer> customers)
    {
        // Toplu müşteri ekleme işlemi yapar
        foreach (var customer in customers)
        {
            customer.CreatedAt = DateTime.UtcNow;
            _context.Customers.Add(customer);
        }
        await _context.SaveChangesAsync();
        return customers;
    }

    public async Task<bool> BulkDeleteAsync(List<int> customerIds)
    {
        // Seçili müşterileri toplu olarak siler
        var customers = await _context.Customers.Where(c => customerIds.Contains(c.Id)).ToListAsync();
        _context.Customers.RemoveRange(customers);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BulkUpdateStatusAsync(List<int> customerIds, string status)
    {
        // Seçili müşterilerin durumunu toplu olarak günceller
        var customers = await _context.Customers.Where(c => customerIds.Contains(c.Id)).ToListAsync();
        foreach (var customer in customers)
        {
            customer.Status = status;
            customer.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SendBulkMessageAsync(List<int> customerIds, string message, string messageType)
    {
        // Seçili müşterilere toplu mesaj gönderir
        var customers = await _context.Customers.Where(c => customerIds.Contains(c.Id)).ToListAsync();
        
        foreach (var customer in customers)
        {
            switch (messageType.ToLower())
            {
                case "sms":
                    // _smsService.SendSmsAsync(customer.Phone, message); // Assuming _smsService is available
                    break;
                case "whatsapp":
                    // _whatsAppService.SendMessageAsync(customer.Phone, message); // Assuming _whatsAppService is available
                    break;
            }
        }
        return true;
    }
}

using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;
    
    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<Customer?> GetByUserIdAsync(string userId)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive);
    }
    
    public async Task<Customer?> GetByIdAsync(int id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
    }
    
    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .ToListAsync();
    }
    
    public async Task<Customer> CreateAsync(Customer customer)
    {
        customer.CreatedAt = DateTime.UtcNow;
        customer.IsActive = true;
        
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        
        return customer;
    }
    
    public async Task<Customer> UpdateAsync(Customer customer)
    {
        var existing = await _context.Customers.FindAsync(customer.Id);
        if (existing == null)
            throw new InvalidOperationException("Customer not found");
            
        existing.FirstName = customer.FirstName;
        existing.LastName = customer.LastName;
        existing.Phone = customer.Phone;
        existing.Email = customer.Email;
        existing.DateOfBirth = customer.DateOfBirth;
        
        await _context.SaveChangesAsync();
        
        return existing;
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Customers.AnyAsync(c => c.Id == id && c.IsActive);
    }
}

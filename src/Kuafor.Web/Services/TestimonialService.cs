using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class TestimonialService : ITestimonialService
{
    private readonly ApplicationDbContext _context;
    
    public TestimonialService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Testimonial>> GetAllAsync()
    {
        return await _context.Testimonials
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Testimonial>> GetApprovedForHomePageAsync()
    {
        return await _context.Testimonials
            .Where(t => t.IsActive && t.IsApproved && t.ShowOnHomePage)
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Testimonial>> GetPendingApprovalAsync()
    {
        return await _context.Testimonials
            .Where(t => !t.IsApproved)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<Testimonial?> GetByIdAsync(int id)
    {
        return await _context.Testimonials.FindAsync(id);
    }
    
    public async Task<Testimonial> CreateAsync(Testimonial testimonial)
    {
        testimonial.CreatedAt = DateTime.UtcNow;
        testimonial.IsActive = true;
        testimonial.IsApproved = false;
        
        _context.Testimonials.Add(testimonial);
        await _context.SaveChangesAsync();
        
        return testimonial;
    }
    
    public async Task<Testimonial> UpdateAsync(Testimonial testimonial)
    {
        var existing = await _context.Testimonials.FindAsync(testimonial.Id);
        if (existing == null)
            throw new InvalidOperationException("Testimonial not found");
            
        existing.Name = testimonial.Name;
        existing.Message = testimonial.Message;
        existing.Rating = testimonial.Rating;
        existing.IsActive = testimonial.IsActive;
        existing.IsApproved = testimonial.IsApproved;
        existing.DisplayOrder = testimonial.DisplayOrder;
        existing.AdminNotes = testimonial.AdminNotes;
        existing.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return existing;
    }
    
    public async Task DeleteAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial != null)
        {
            testimonial.IsActive = false;
            testimonial.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Testimonials.AnyAsync(t => t.Id == id);
    }
    
    public async Task ApproveAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial != null)
        {
            testimonial.IsApproved = true;
            testimonial.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task RejectAsync(int id, string reason)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial != null)
        {
            testimonial.IsApproved = false;
            testimonial.AdminNotes = reason;
            testimonial.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task UpdateDisplayOrderAsync(int id, int newOrder)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial != null)
        {
            testimonial.DisplayOrder = newOrder;
            testimonial.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}

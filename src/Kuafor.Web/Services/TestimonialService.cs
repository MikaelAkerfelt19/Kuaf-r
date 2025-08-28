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
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<Testimonial?> GetByIdAsync(int id)
    {
        return await _context.Testimonials.FindAsync(id);
    }
    
    public async Task<Testimonial> CreateAsync(Testimonial testimonial)
    {
        testimonial.CreatedAt = DateTime.UtcNow;
        _context.Testimonials.Add(testimonial);
        await _context.SaveChangesAsync();
        return testimonial;
    }
    
    public async Task<Testimonial> UpdateAsync(Testimonial testimonial)
    {
        testimonial.UpdatedAt = DateTime.UtcNow;
        _context.Testimonials.Update(testimonial);
        await _context.SaveChangesAsync();
        return testimonial;
    }
    
    public async Task DeleteAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial != null)
        {
            _context.Testimonials.Remove(testimonial);
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<IEnumerable<Testimonial>> GetActiveAsync()
    {
        return await _context.Testimonials
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Testimonial>> GetApprovedAsync()
    {
        return await _context.Testimonials
            .Where(t => t.IsActive && t.IsApproved)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Testimonial>> GetForHomePageAsync()
    {
        return await _context.Testimonials
            .Where(t => t.IsActive && t.IsApproved && t.ShowOnHomePage)
            .OrderBy(t => t.DisplayOrder)
            .ThenByDescending(t => t.CreatedAt)
            .Take(6) // Ana sayfa için maksimum 6 testimonial
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Testimonial>> GetApprovedForHomePageAsync()
    {
        return await _context.Testimonials
            .Where(t => t.IsActive && t.IsApproved && t.ShowOnHomePage)
            .OrderBy(t => t.DisplayOrder)
            .ThenByDescending(t => t.CreatedAt)
            .Take(6) // Ana sayfa için maksimum 6 testimonial
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Testimonial>> GetByRatingAsync(int rating)
    {
        return await _context.Testimonials
            .Where(t => t.IsActive && t.IsApproved && t.Rating == rating)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Testimonial>> GetPendingApprovalAsync()
    {
        return await _context.Testimonials
            .Where(t => t.IsActive && !t.IsApproved)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<bool> IsActiveAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        return testimonial?.IsActive ?? false;
    }
    
    public async Task<bool> IsApprovedAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        return testimonial?.IsApproved ?? false;
    }
    
    public async Task<Testimonial> ApproveAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null)
            throw new ArgumentException("Testimonial not found");
        
        testimonial.IsApproved = true;
        testimonial.UpdatedAt = DateTime.UtcNow;
        
        _context.Testimonials.Update(testimonial);
        await _context.SaveChangesAsync();
        
        return testimonial;
    }
    
    public async Task<Testimonial> RejectAsync(int id, string? reason = null)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null)
            throw new ArgumentException("Testimonial not found");
        
        testimonial.IsApproved = false;
        testimonial.AdminNotes = reason;
        testimonial.UpdatedAt = DateTime.UtcNow;
        
        _context.Testimonials.Update(testimonial);
        await _context.SaveChangesAsync();
        
        return testimonial;
    }
    
    public async Task<double> GetAverageRatingAsync()
    {
        var approvedTestimonials = await _context.Testimonials
            .Where(t => t.IsActive && t.IsApproved)
            .Select(t => t.Rating)
            .ToListAsync();
        
        return approvedTestimonials.Any() ? approvedTestimonials.Average() : 0.0;
    }
    
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Testimonials.AnyAsync(t => t.Id == id);
    }
    
    public async Task<int> GetCountAsync()
    {
        return await _context.Testimonials.CountAsync();
    }
    
    public async Task<int> GetCountByRatingAsync(int rating)
    {
        return await _context.Testimonials
            .Where(t => t.IsActive && t.IsApproved && t.Rating == rating)
            .CountAsync();
    }
    
    public async Task<int> GetPendingCountAsync()
    {
        return await _context.Testimonials
            .Where(t => t.IsActive && !t.IsApproved)
            .CountAsync();
    }
}

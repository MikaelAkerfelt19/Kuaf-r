using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface ITestimonialService
{
    // CRUD Operations
    Task<IEnumerable<Testimonial>> GetAllAsync();
    Task<Testimonial?> GetByIdAsync(int id);
    Task<Testimonial> CreateAsync(Testimonial testimonial);
    Task<Testimonial> UpdateAsync(Testimonial testimonial);
    Task DeleteAsync(int id);
    
    // Query Operations
    Task<IEnumerable<Testimonial>> GetActiveAsync();
    Task<IEnumerable<Testimonial>> GetApprovedAsync();
    Task<IEnumerable<Testimonial>> GetForHomePageAsync();
    Task<IEnumerable<Testimonial>> GetApprovedForHomePageAsync(); // Eksik metot eklendi
    Task<IEnumerable<Testimonial>> GetByRatingAsync(int rating);
    Task<IEnumerable<Testimonial>> GetPendingApprovalAsync();
    
    // Business Logic
    Task<bool> IsActiveAsync(int id);
    Task<bool> IsApprovedAsync(int id);
    Task<Testimonial> ApproveAsync(int id);
    Task<Testimonial> RejectAsync(int id, string? reason = null);
    Task<double> GetAverageRatingAsync();
    
    // Utility Methods
    Task<bool> ExistsAsync(int id);
    Task<int> GetCountAsync();
    Task<int> GetCountByRatingAsync(int rating);
    Task<int> GetPendingCountAsync();
}

using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface ITestimonialService
{
    Task<IEnumerable<Testimonial>> GetAllAsync();
    Task<IEnumerable<Testimonial>> GetApprovedForHomePageAsync();
    Task<IEnumerable<Testimonial>> GetPendingApprovalAsync();
    Task<Testimonial?> GetByIdAsync(int id);
    Task<Testimonial> CreateAsync(Testimonial testimonial);
    Task<Testimonial> UpdateAsync(Testimonial testimonial);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task ApproveAsync(int id);
    Task RejectAsync(int id, string reason);
    Task UpdateDisplayOrderAsync(int id, int newOrder);
}

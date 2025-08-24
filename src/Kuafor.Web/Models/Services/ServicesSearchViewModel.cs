using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Models.Services;

public class ServicesSearchViewModel
{
    public string? Query { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<Service> Results { get; set; } = new();
}

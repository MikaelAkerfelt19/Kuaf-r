using System.ComponentModel.DataAnnotations;
namespace Kuafor.Web.Models.Entities;

public class Package
{
    public int Id { get; set; }
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [Range(0, 100000)]
    public decimal TotalPrice { get; set; }
    public int SessionCount { get; set; }
    [Range(0, 100000)]
    public decimal SessionUnitPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<PackageService> PackageServices { get; set; } = new List<PackageService>();
    public virtual ICollection<PackageSale> PackageSales { get; set; } = new List<PackageSale>();
}

public class PackageService
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public int ServiceId { get; set; }
    public int SessionCount { get; set; }
    
    public virtual Package Package { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
}

public class PackageSale
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public int CustomerId { get; set; }
    public int RemainingSessions { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    
    public virtual Package Package { get; set; } = null!;
    public virtual Customer Customer { get; set; } = null!;
}
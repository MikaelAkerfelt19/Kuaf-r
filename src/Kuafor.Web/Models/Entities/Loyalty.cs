using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Entities;

public class Loyalty
{
    public int Id { get; set; }
    
    [Required]
    public int CustomerId { get; set; }
    
    public int Points { get; set; } = 0;
    
    [Required, StringLength(20)]
    public string Tier { get; set; } = "Bronz"; // Bronz, Gümüş, Altın, Platin
    
    public int TotalSpent { get; set; } = 0;
    
    public int AppointmentCount { get; set; } = 0;
    
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public virtual Customer Customer { get; set; } = null!;
}

public class LoyaltyTransaction
{
    public int Id { get; set; }
    
    [Required]
    public int CustomerId { get; set; }
    
    [Required]
    public int Points { get; set; }
    
    [Required, StringLength(100)]
    public string Reason { get; set; } = string.Empty;
    
    [Required, StringLength(20)]
    public string Type { get; set; } = "Earned"; // Earned, Spent, Expired
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual Customer Customer { get; set; } = null!;
}
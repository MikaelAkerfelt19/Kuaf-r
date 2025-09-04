namespace Kuafor.Web.Models.Admin.Packages;

public class PackageViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public int SessionCount { get; set; }
    public decimal SessionUnitPrice { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

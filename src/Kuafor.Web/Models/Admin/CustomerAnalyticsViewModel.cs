using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Models.Admin;

public class CustomerAnalyticsViewModel
{
    public int TotalCustomers { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int ActiveCustomers { get; set; }
    public int TotalAppointments { get; set; }
    public List<object> CustomerList { get; set; } = new();
    public List<Entities.Customer> TopCustomers { get; set; } = new();
    public List<CustomerGrowthData> CustomerGrowthData { get; set; } = new();
}

public class CustomerDetailsViewModel
{
    public Entities.Customer Customer { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
    public decimal TotalSpent { get; set; }
    public DateTime? LastVisit { get; set; }
    public int TotalVisits { get; set; }
    public decimal AverageSpending { get; set; }
}

public class CustomerGrowthData
{
    public string Month { get; set; } = string.Empty;
    public int NewCustomers { get; set; }
    public int TotalCustomers { get; set; }
}

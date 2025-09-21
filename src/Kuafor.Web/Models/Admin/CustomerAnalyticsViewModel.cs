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
    public Entities.Customer CustomerInfo { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
    public List<Appointment> AppointmentList { get; set; } = new();
    public decimal TotalSpent { get; set; }
    public decimal TotalSpentAmount { get; set; }
    public int TotalVisits { get; set; }
    public decimal AverageSpending { get; set; }
    public DateTime? LastVisitDate { get; set; }
    public int TotalAppointments => AppointmentList.Count;
    public int CompletedAppointments => AppointmentList.Count(a => a.Status == Models.Enums.AppointmentStatus.Completed);
    public int CancelledAppointments => AppointmentList.Count(a => a.Status == Models.Enums.AppointmentStatus.Cancelled);
    public decimal AverageSpent => TotalAppointments > 0 ? TotalSpentAmount / TotalAppointments : 0;
    public double CompletionRate => TotalAppointments > 0 ? (CompletedAppointments * 100.0 / TotalAppointments) : 0;
    public double CancellationRate => TotalAppointments > 0 ? (CancelledAppointments * 100.0 / TotalAppointments) : 0;
}

public class CustomerGrowthData
{
    public string Month { get; set; } = string.Empty;
    public int NewCustomers { get; set; }
    public int TotalCustomers { get; set; }
}

using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Admin.Reports;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Enums;
using System.Text.Json;
using OfficeOpenXml;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace Kuafor.Web.Services
{
    public class ReportingService : IReportingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppointmentService _appointmentService;
        private readonly ICustomerService _customerService;
        private readonly IStylistService _stylistService;
        private readonly IBranchService _branchService;
        private readonly ILogger<ReportingService> _logger;

        public ReportingService(
            ApplicationDbContext context,
            IAppointmentService appointmentService,
            ICustomerService customerService,
            IStylistService stylistService,
            IBranchService branchService,
            ILogger<ReportingService> logger)
        {
            _context = context;
            _appointmentService = appointmentService;
            _customerService = customerService;
            _stylistService = stylistService;
            _branchService = branchService;
            _logger = logger;
        }

        public async Task<DashboardReport> GetDashboardReportAsync(DateTime? from = null, DateTime? to = null)
        {
            var fromDate = from ?? DateTime.Today.AddDays(-30);
            var toDate = to ?? DateTime.Today;

            var appointments = await _appointmentService.GetByDateRangeAsync(fromDate, toDate);
            var customers = await _customerService.GetAllAsync();
            var stylists = await _stylistService.GetAllAsync();
            var branches = await _branchService.GetAllAsync();

            return new DashboardReport
            {
                Period = $"{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}",
                TotalAppointments = appointments.Count(),
                TotalRevenue = appointments.Sum(a => a.FinalPrice),
                TotalCustomers = customers.Count(),
                ActiveStylists = stylists.Count(s => s.IsActive),
                ActiveBranches = branches.Count(b => b.IsActive),
                AverageTicketValue = appointments.Any() ? (decimal)appointments.Average(a => a.FinalPrice) : 0,
                ConversionRate = CalculateConversionRate(appointments, customers),
                TopServices = GetTopServices(appointments),
                TopStylists = GetTopStylists(appointments),
                RevenueByDay = GetRevenueByDay(appointments, fromDate, toDate),
                AppointmentStatusDistribution = GetAppointmentStatusDistribution(appointments)
            };
        }

        public async Task<RevenueReport> GetRevenueReportAsync(DateTime from, DateTime to)
        {
            var appointments = await _appointmentService.GetByDateRangeAsync(from, to);
            var branches = await _branchService.GetAllAsync();
            var stylists = await _stylistService.GetAllAsync();

            return new RevenueReport
            {
                Period = $"{from:dd/MM/yyyy} - {to:dd/MM/yyyy}",
                TotalRevenue = appointments.Sum(a => a.FinalPrice),
                TotalAppointments = appointments.Count(),
                AverageTicketValue = appointments.Any() ? (decimal)appointments.Average(a => a.FinalPrice) : 0,
                RevenueByBranch = branches.Select(b => new BranchRevenue
                {
                    BranchName = b.Name,
                    Revenue = appointments.Where(a => a.BranchId == b.Id).Sum(a => a.FinalPrice),
                    AppointmentCount = appointments.Count(a => a.BranchId == b.Id),
                    Percentage = CalculatePercentage((double)appointments.Where(a => a.BranchId == b.Id).Sum(a => a.FinalPrice), (double)appointments.Sum(a => a.FinalPrice))
                }).OrderByDescending(b => b.Revenue).ToList(),
                RevenueByStylist = stylists.Select(s => new StylistRevenue
                {
                    StylistName = $"{s.FirstName} {s.LastName}",
                    Revenue = appointments.Where(a => a.StylistId == s.Id).Sum(a => a.FinalPrice),
                    AppointmentCount = appointments.Count(a => a.StylistId == s.Id),
                AverageRating = (double)s.Rating,
                Percentage = CalculatePercentage((double)appointments.Where(a => a.StylistId == s.Id).Sum(a => a.FinalPrice), (double)appointments.Sum(a => a.FinalPrice))
                }).OrderByDescending(s => s.Revenue).ToList(),
                RevenueByService = GetRevenueByService(appointments),
                RevenueByDay = GetRevenueByDay(appointments, from, to),
                RevenueByHour = GetRevenueByHour(appointments)
            };
        }

        public async Task<AppointmentReport> GetAppointmentReportAsync(DateTime from, DateTime to)
        {
            var appointments = await _appointmentService.GetByDateRangeAsync(from, to);
            var branches = await _branchService.GetAllAsync();
            var stylists = await _stylistService.GetAllAsync();

            return new AppointmentReport
            {
                Period = $"{from:dd/MM/yyyy} - {to:dd/MM/yyyy}",
                TotalAppointments = appointments.Count(),
                ConfirmedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Confirmed),
                CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                NoShowAppointments = appointments.Count(a => a.Status == AppointmentStatus.NoShow),
                AverageAppointmentsPerDay = CalculateAverageAppointmentsPerDay(appointments, from, to),
                PeakHours = GetPeakHours(appointments),
                AppointmentStatusDistribution = GetAppointmentStatusDistribution(appointments),
                AppointmentsByBranch = GetAppointmentsByBranch(appointments, branches),
                AppointmentsByStylist = GetAppointmentsByStylist(appointments, stylists),
                AppointmentsByDay = GetAppointmentsByDay(appointments, from, to),
                AppointmentsByHour = GetAppointmentsByHour(appointments)
            };
        }

        public async Task<CustomerReport> GetCustomerReportAsync(DateTime from, DateTime to)
        {
            var customers = await _customerService.GetAllAsync();
            var appointments = await _appointmentService.GetByDateRangeAsync(from, to);
            var allAppointments = await _appointmentService.GetAllAsync();

            return new CustomerReport
            {
                Period = $"{from:dd/MM/yyyy} - {to:dd/MM/yyyy}",
                TotalCustomers = customers.Count(),
                NewCustomers = customers.Count(c => c.CreatedAt >= from && c.CreatedAt <= to),
                ActiveCustomers = customers.Count(c => allAppointments.Any(a => a.CustomerId == c.Id && a.StartAt >= from && a.StartAt <= to)),
                AverageAppointmentsPerCustomer = CalculateAverageAppointmentsPerCustomer(appointments, customers),
                CustomerRetentionRate = CalculateCustomerRetentionRate(customers, allAppointments, from, to),
                TopCustomers = GetTopCustomers(customers, allAppointments),
                CustomerSegments = GetCustomerSegments(customers, allAppointments),
                CustomerLifetimeValue = CalculateCustomerLifetimeValue(customers, allAppointments)
            };
        }

        public async Task<StylistReport> GetStylistReportAsync(DateTime from, DateTime to)
        {
            var stylists = await _stylistService.GetAllAsync();
            var appointments = await _appointmentService.GetByDateRangeAsync(from, to);

            return new StylistReport
            {
                Period = $"{from:dd/MM/yyyy} - {to:dd/MM/yyyy}",
                TotalStylists = stylists.Count(),
                ActiveStylists = stylists.Count(s => s.IsActive),
                AverageRating = stylists.Any() ? (double)stylists.Average(s => s.Rating) : 0,
                TopPerformers = GetTopPerformers(stylists, appointments),
                StylistUtilization = GetStylistUtilization(stylists, appointments, from, to),
                RevenueByStylist = GetRevenueByStylist(stylists, appointments),
                AppointmentsByStylist = GetAppointmentsByStylist(appointments, stylists),
                CustomerSatisfaction = GetCustomerSatisfaction(stylists, appointments)
            };
        }

        public async Task<BranchReport> GetBranchReportAsync(DateTime from, DateTime to)
        {
            var branches = await _branchService.GetAllAsync();
            var appointments = await _appointmentService.GetByDateRangeAsync(from, to);

            return new BranchReport
            {
                Period = $"{from:dd/MM/yyyy} - {to:dd/MM/yyyy}",
                TotalBranches = branches.Count(),
                ActiveBranches = branches.Count(b => b.IsActive),
                BranchPerformance = GetBranchPerformance(branches, appointments),
                RevenueByBranch = GetRevenueByBranch(branches, appointments),
                AppointmentsByBranch = GetAppointmentsByBranch(appointments, branches),
                BranchUtilization = GetBranchUtilization(branches, appointments, from, to)
            };
        }

        public async Task<ChartData> GetRevenueChartDataAsync(DateTime from, DateTime to, string period = "daily")
        {
            var appointments = await _appointmentService.GetByDateRangeAsync(from, to);
            
            return period.ToLower() switch
            {
                "hourly" => GetRevenueByHour(appointments),
                "daily" => GetRevenueByDay(appointments, from, to),
                "weekly" => GetRevenueByWeek(appointments, from, to),
                "monthly" => GetRevenueByMonth(appointments, from, to),
                _ => GetRevenueByDay(appointments, from, to)
            };
        }

        public async Task<ChartData> GetAppointmentChartDataAsync(DateTime from, DateTime to, string period = "daily")
        {
            var appointments = await _appointmentService.GetByDateRangeAsync(from, to);
            
            return period.ToLower() switch
            {
                "hourly" => GetAppointmentsByHour(appointments),
                "daily" => GetAppointmentsByDay(appointments, from, to),
                "weekly" => GetAppointmentsByWeek(appointments, from, to),
                "monthly" => GetAppointmentsByMonth(appointments, from, to),
                _ => GetAppointmentsByDay(appointments, from, to)
            };
        }

        public async Task<ChartData> GetCustomerChartDataAsync(DateTime from, DateTime to, string period = "daily")
        {
            var customers = await _customerService.GetAllAsync();
            var appointments = await _appointmentService.GetByDateRangeAsync(from, to);
            
            return period.ToLower() switch
            {
                "daily" => GetCustomersByDay(customers, from, to),
                "weekly" => GetCustomersByWeek(customers, from, to),
                "monthly" => GetCustomersByMonth(customers, from, to),
                _ => GetCustomersByDay(customers, from, to)
            };
        }

        public async Task<ComparisonReport> GetPeriodComparisonAsync(DateTime currentFrom, DateTime currentTo, DateTime previousFrom, DateTime previousTo)
        {
            var currentAppointments = await _appointmentService.GetByDateRangeAsync(currentFrom, currentTo);
            var previousAppointments = await _appointmentService.GetByDateRangeAsync(previousFrom, previousTo);

            var currentRevenue = currentAppointments.Sum(a => a.FinalPrice);
            var previousRevenue = previousAppointments.Sum(a => a.FinalPrice);
            var currentCount = currentAppointments.Count();
            var previousCount = previousAppointments.Count();

            return new ComparisonReport
            {
                CurrentPeriod = $"{currentFrom:dd/MM/yyyy} - {currentTo:dd/MM/yyyy}",
                PreviousPeriod = $"{previousFrom:dd/MM/yyyy} - {previousTo:dd/MM/yyyy}",
                RevenueComparison = new ComparisonMetric
                {
                    Current = (double)currentRevenue,
                    Previous = (double)previousRevenue,
                Change = (double)(currentRevenue - previousRevenue),
                ChangePercentage = CalculatePercentageChange((double)currentRevenue, (double)previousRevenue)
                },
                AppointmentComparison = new ComparisonMetric
                {
                    Current = currentCount,
                    Previous = previousCount,
                    Change = currentCount - previousCount,
                    ChangePercentage = CalculatePercentageChange(currentCount, previousCount)
                },
                AverageTicketValueComparison = new ComparisonMetric
                {
                    Current = (double)(currentAppointments.Any() ? currentAppointments.Average(a => a.FinalPrice) : 0),
                    Previous = (double)(previousAppointments.Any() ? previousAppointments.Average(a => a.FinalPrice) : 0),
                    Change = (double)(currentAppointments.Any() ? currentAppointments.Average(a => a.FinalPrice) : 0) - (double)(previousAppointments.Any() ? previousAppointments.Average(a => a.FinalPrice) : 0),
                    ChangePercentage = CalculatePercentageChange(
                        (double)(currentAppointments.Any() ? currentAppointments.Average(a => a.FinalPrice) : 0),
                        (double)(previousAppointments.Any() ? previousAppointments.Average(a => a.FinalPrice) : 0))
                }
            };
        }

        public async Task<YearOverYearReport> GetYearOverYearReportAsync(int year)
        {
            var currentYearAppointments = await _appointmentService.GetByDateRangeAsync(
                new DateTime(year, 1, 1), 
                new DateTime(year, 12, 31));
            var previousYearAppointments = await _appointmentService.GetByDateRangeAsync(
                new DateTime(year - 1, 1, 1), 
                new DateTime(year - 1, 12, 31));

            return new YearOverYearReport
            {
                Year = year,
                CurrentYearRevenue = currentYearAppointments.Sum(a => a.FinalPrice),
                PreviousYearRevenue = previousYearAppointments.Sum(a => a.FinalPrice),
                CurrentYearAppointments = currentYearAppointments.Count(),
                PreviousYearAppointments = previousYearAppointments.Count(),
                RevenueGrowth = CalculatePercentageChange(
                    (double)currentYearAppointments.Sum(a => a.FinalPrice),
                    (double)previousYearAppointments.Sum(a => a.FinalPrice)),
                AppointmentGrowth = CalculatePercentageChange(
                    currentYearAppointments.Count(),
                    previousYearAppointments.Count()),
                MonthlyComparison = GetMonthlyComparison(currentYearAppointments, previousYearAppointments)
            };
        }

        // Export Methods
        public byte[] ExportToExcel<T>(IEnumerable<T> data, string fileName, string sheetName = "Rapor")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            // Header'ları ekle
            var properties = typeof(T).GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = properties[i].Name;
            }

            // Verileri ekle
            int row = 2;
            foreach (var item in data)
            {
                for (int i = 0; i < properties.Length; i++)
                {
                    var value = properties[i].GetValue(item);
                    worksheet.Cells[row, i + 1].Value = value?.ToString() ?? "";
                }
                row++;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public Task<byte[]> ExportToPdfAsync<T>(IEnumerable<T> data, string fileName, string title)
        {
            return Task.FromResult(ExportToPdf(data, fileName, title));
        }

        private byte[] ExportToPdf<T>(IEnumerable<T> data, string fileName, string title)
        {
            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            
            // Başlık ekle
            var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var titleParagraph = new Paragraph(title)
                .SetFont(titleFont)
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(titleParagraph);
            
            document.Add(new Paragraph(" ")); // Boş satır
            
            // Tablo oluştur
            var properties = typeof(T).GetProperties();
            var table = new Table(properties.Length);
            table.SetWidth(UnitValue.CreatePercentValue(100));
            
            // Header'ları ekle
            var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            foreach (var property in properties)
            {
                var cell = new Cell().Add(new Paragraph(property.Name).SetFont(headerFont));
                cell.SetBackgroundColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY);
                table.AddHeaderCell(cell);
            }
            
            // Verileri ekle
            var dataFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            foreach (var item in data)
            {
                foreach (var property in properties)
                {
                    var value = property.GetValue(item)?.ToString() ?? "";
                    table.AddCell(new Cell().Add(new Paragraph(value).SetFont(dataFont)));
                }
            }
            
            document.Add(table);
            document.Close();
            
            return memoryStream.ToArray();
        }

        public byte[] ExportToCsv<T>(IEnumerable<T> data, string fileName)
        {
            var properties = typeof(T).GetProperties();
            var csv = new System.Text.StringBuilder();

            // Header'ları ekle
            csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

            // Verileri ekle
            foreach (var item in data)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item)?.ToString() ?? "";
                    // CSV'de virgül içeren değerleri tırnak içine al
                    if (value.Contains(",") || value.Contains("\""))
                    {
                        value = $"\"{value.Replace("\"", "\"\"")}\"";
                    }
                    return value;
                });
                csv.AppendLine(string.Join(",", values));
            }

            return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
        }

        public Task<CustomReport> GenerateCustomReportAsync(string reportType, Dictionary<string, object> parameters)
        {
            // Özel rapor oluşturma mantığı
            return Task.FromException<CustomReport>(new NotImplementedException("Custom report generation will be implemented"));
        }

        public Task<List<ReportTemplate>> GetReportTemplatesAsync()
        {
            // Rapor şablonları
            return Task.FromException<List<ReportTemplate>>(new NotImplementedException("Report templates will be implemented"));
        }

        public Task<ReportTemplate> SaveReportTemplateAsync(ReportTemplate template)
        {
            // Rapor şablonu kaydetme
            return Task.FromException<ReportTemplate>(new NotImplementedException("Report template saving will be implemented"));
        }

        // Helper Methods
        private double CalculateConversionRate(IEnumerable<Appointment> appointments, IEnumerable<Customer> customers)
        {
            if (!customers.Any()) return 0;
            return (double)appointments.Count() / customers.Count() * 100;
        }

        private List<TopService> GetTopServices(IEnumerable<Appointment> appointments)
        {
            return appointments
                .GroupBy(a => a.Service?.Name ?? "Bilinmeyen")
                .Select(g => new TopService
                {
                    Name = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(a => a.FinalPrice)
                })
                .OrderByDescending(s => s.Count)
                .Take(10)
                .ToList();
        }

        private List<TopStylist> GetTopStylists(IEnumerable<Appointment> appointments)
        {
            return appointments
                .GroupBy(a => new { a.Stylist?.FirstName, a.Stylist?.LastName })
                .Select(g => new TopStylist
                {
                    Name = $"{g.Key.FirstName} {g.Key.LastName}",
                    Count = g.Count(),
                    Revenue = g.Sum(a => a.FinalPrice)
                })
                .OrderByDescending(s => s.Count)
                .Take(10)
                .ToList();
        }

        private ChartData GetRevenueByDay(IEnumerable<Appointment> appointments, DateTime from, DateTime to)
        {
            var revenueByDay = appointments
                .GroupBy(a => a.StartAt.Date)
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key.ToString("dd/MM/yyyy"),
                    Value = (double)g.Sum(a => a.FinalPrice)
                })
                .OrderBy(d => d.Label)
                .ToList();

            return new ChartData
            {
                Labels = revenueByDay.Select(d => d.Label).ToList(),
                Values = revenueByDay.Select(d => d.Value).ToList(),
                Title = "Günlük Gelir"
            };
        }

        private ChartData GetRevenueByHour(IEnumerable<Appointment> appointments)
        {
            var revenueByHour = appointments
                .GroupBy(a => a.StartAt.Hour)
                .Select(g => new ChartDataPoint
                {
                    Label = $"{g.Key:00}:00",
                    Value = (double)g.Sum(a => a.FinalPrice)
                })
                .OrderBy(h => h.Label)
                .ToList();

            return new ChartData
            {
                Labels = revenueByHour.Select(h => h.Label).ToList(),
                Values = revenueByHour.Select(h => h.Value).ToList(),
                Title = "Saatlik Gelir"
            };
        }

        private List<AppointmentStatusDistribution> GetAppointmentStatusDistribution(IEnumerable<Appointment> appointments)
        {
            return appointments
                .GroupBy(a => a.Status)
                .Select(g => new AppointmentStatusDistribution
                {
                    Status = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = CalculatePercentage(g.Count(), appointments.Count())
                })
                .ToList();
        }

        private double CalculatePercentage(double value, double total)
        {
            return total == 0 ? 0 : (value / total) * 100;
        }

        private double CalculatePercentageChange(double current, double previous)
        {
            return previous == 0 ? 0 : ((current - previous) / previous) * 100;
        }

        // Diğer helper metodlar...
        private List<BranchRevenue> GetRevenueByBranch(IEnumerable<Branch> branches, IEnumerable<Appointment> appointments)
        {
            return branches.Select(b => new BranchRevenue
            {
                BranchName = b.Name,
                Revenue = appointments.Where(a => a.BranchId == b.Id).Sum(a => a.FinalPrice),
                AppointmentCount = appointments.Count(a => a.BranchId == b.Id),
                Percentage = CalculatePercentage(
                    (double)appointments.Where(a => a.BranchId == b.Id).Sum(a => a.FinalPrice),
                    (double)appointments.Sum(a => a.FinalPrice))
            }).OrderByDescending(b => b.Revenue).ToList();
        }

        private List<StylistRevenue> GetRevenueByStylist(IEnumerable<Stylist> stylists, IEnumerable<Appointment> appointments)
        {
            return stylists.Select(s => new StylistRevenue
            {
                StylistName = $"{s.FirstName} {s.LastName}",
                Revenue = appointments.Where(a => a.StylistId == s.Id).Sum(a => a.FinalPrice),
                AppointmentCount = appointments.Count(a => a.StylistId == s.Id),
                AverageRating = (double)s.Rating,
                Percentage = CalculatePercentage(
                    (double)appointments.Where(a => a.StylistId == s.Id).Sum(a => a.FinalPrice),
                    (double)appointments.Sum(a => a.FinalPrice))
            }).OrderByDescending(s => s.Revenue).ToList();
        }

        private List<ServiceRevenue> GetRevenueByService(IEnumerable<Appointment> appointments)
        {
            return appointments
                .GroupBy(a => a.Service?.Name ?? "Bilinmeyen")
                .Select(g => new ServiceRevenue
                {
                    ServiceName = g.Key,
                    Revenue = g.Sum(a => a.FinalPrice),
                    AppointmentCount = g.Count(),
                    Percentage = CalculatePercentage((double)g.Sum(a => a.FinalPrice), (double)appointments.Sum(a => a.FinalPrice))
                })
                .OrderByDescending(s => s.Revenue)
                .ToList();
        }

        private double CalculateAverageAppointmentsPerDay(IEnumerable<Appointment> appointments, DateTime from, DateTime to)
        {
            var days = (to - from).TotalDays + 1;
            return appointments.Count() / days;
        }

        private List<PeakHour> GetPeakHours(IEnumerable<Appointment> appointments)
        {
            return appointments
                .GroupBy(a => a.StartAt.Hour)
                .Select(g => new PeakHour
                {
                    Hour = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(a => a.FinalPrice)
                })
                .OrderByDescending(h => h.Count)
                .Take(5)
                .ToList();
        }

        private List<AppointmentsByBranch> GetAppointmentsByBranch(IEnumerable<Appointment> appointments, IEnumerable<Branch> branches)
        {
            return branches.Select(b => new AppointmentsByBranch
            {
                BranchName = b.Name,
                Count = appointments.Count(a => a.BranchId == b.Id),
                Percentage = CalculatePercentage(appointments.Count(a => a.BranchId == b.Id), appointments.Count())
            }).OrderByDescending(b => b.Count).ToList();
        }

        private List<AppointmentsByStylist> GetAppointmentsByStylist(IEnumerable<Appointment> appointments, IEnumerable<Stylist> stylists)
        {
            return stylists.Select(s => new AppointmentsByStylist
            {
                StylistName = $"{s.FirstName} {s.LastName}",
                Count = appointments.Count(a => a.StylistId == s.Id),
                Percentage = CalculatePercentage(appointments.Count(a => a.StylistId == s.Id), appointments.Count())
            }).OrderByDescending(s => s.Count).ToList();
        }

        private ChartData GetAppointmentsByDay(IEnumerable<Appointment> appointments, DateTime from, DateTime to)
        {
            var appointmentsByDay = appointments
                .GroupBy(a => a.StartAt.Date)
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key.ToString("dd/MM/yyyy"),
                    Value = g.Count()
                })
                .OrderBy(d => d.Label)
                .ToList();

            return new ChartData
            {
                Labels = appointmentsByDay.Select(d => d.Label).ToList(),
                Values = appointmentsByDay.Select(d => d.Value).ToList(),
                Title = "Günlük Randevu Sayısı"
            };
        }

        private ChartData GetAppointmentsByHour(IEnumerable<Appointment> appointments)
        {
            var appointmentsByHour = appointments
                .GroupBy(a => a.StartAt.Hour)
                .Select(g => new ChartDataPoint
                {
                    Label = $"{g.Key:00}:00",
                    Value = g.Count()
                })
                .OrderBy(h => h.Label)
                .ToList();

            return new ChartData
            {
                Labels = appointmentsByHour.Select(h => h.Label).ToList(),
                Values = appointmentsByHour.Select(h => h.Value).ToList(),
                Title = "Saatlik Randevu Sayısı"
            };
        }

        private double CalculateAverageAppointmentsPerCustomer(IEnumerable<Appointment> appointments, IEnumerable<Customer> customers)
        {
            if (!customers.Any()) return 0;
            return (double)appointments.Count() / customers.Count();
        }

        private double CalculateCustomerRetentionRate(IEnumerable<Customer> customers, IEnumerable<Appointment> allAppointments, DateTime from, DateTime to)
        {
            var activeCustomers = customers.Count(c => allAppointments.Any(a => a.CustomerId == c.Id && a.StartAt >= from && a.StartAt <= to));
            return customers.Any() ? (double)activeCustomers / customers.Count() * 100 : 0;
        }

        private List<Models.Admin.Reports.TopCustomer> GetTopCustomers(IEnumerable<Customer> customers, IEnumerable<Appointment> appointments)
        {
            return customers
                .Select(c => new Models.Admin.Reports.TopCustomer
                {
                    CustomerName = $"{c.FirstName} {c.LastName}",
                    AppointmentCount = appointments.Count(a => a.CustomerId == c.Id),
                    TotalSpent = appointments.Where(a => a.CustomerId == c.Id).Sum(a => a.FinalPrice),
                    LastAppointment = appointments.Where(a => a.CustomerId == c.Id).Max(a => a.StartAt)
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(10)
                .ToList();
        }

        private List<Models.Admin.Reports.CustomerSegment> GetCustomerSegments(IEnumerable<Customer> customers, IEnumerable<Appointment> appointments)
        {
            return customers
                .Select(c => new Models.Admin.Reports.CustomerSegment
                {
                    SegmentName = GetCustomerSegment(c, appointments),
                    Count = 1
                })
                .GroupBy(s => s.SegmentName)
                .Select(g => new Models.Admin.Reports.CustomerSegment
                {
                    SegmentName = g.Key,
                    Count = g.Count()
                })
                .ToList();
        }

        private string GetCustomerSegment(Customer customer, IEnumerable<Appointment> appointments)
        {
            var customerAppointments = appointments.Where(a => a.CustomerId == customer.Id);
            var appointmentCount = customerAppointments.Count();
            var totalSpent = customerAppointments.Sum(a => a.FinalPrice);

            if (appointmentCount >= 10 && totalSpent >= 1000)
                return "VIP Müşteri";
            else if (appointmentCount >= 5 && totalSpent >= 500)
                return "Sadık Müşteri";
            else if (appointmentCount >= 2)
                return "Düzenli Müşteri";
            else
                return "Yeni Müşteri";
        }

        private double CalculateCustomerLifetimeValue(IEnumerable<Customer> customers, IEnumerable<Appointment> appointments)
        {
            if (!customers.Any()) return 0;
            return (double)appointments.Sum(a => a.FinalPrice) / customers.Count();
        }

        // Diğer helper metodlar devam ediyor...
        private List<TopPerformer> GetTopPerformers(IEnumerable<Stylist> stylists, IEnumerable<Appointment> appointments)
        {
            return stylists
                .Select(s => new TopPerformer
                {
                    StylistName = $"{s.FirstName} {s.LastName}",
                    AppointmentCount = appointments.Count(a => a.StylistId == s.Id),
                    Revenue = appointments.Where(a => a.StylistId == s.Id).Sum(a => a.FinalPrice),
                    Rating = (double)s.Rating
                })
                .OrderByDescending(p => p.Revenue)
                .Take(10)
                .ToList();
        }

        private List<StylistUtilization> GetStylistUtilization(IEnumerable<Stylist> stylists, IEnumerable<Appointment> appointments, DateTime from, DateTime to)
        {
            var totalHours = (to - from).TotalHours;
            return stylists.Select(s => new StylistUtilization
            {
                StylistName = $"{s.FirstName} {s.LastName}",
                TotalHours = totalHours,
                BookedHours = appointments.Where(a => a.StylistId == s.Id).Sum(a => (a.EndAt - a.StartAt).TotalHours),
                UtilizationRate = CalculatePercentage(
                    (double)appointments.Where(a => a.StylistId == s.Id).Sum(a => (a.EndAt - a.StartAt).TotalHours),
                    totalHours)
            }).ToList();
        }

        private List<CustomerSatisfaction> GetCustomerSatisfaction(IEnumerable<Stylist> stylists, IEnumerable<Appointment> appointments)
        {
            return stylists.Select(s => new CustomerSatisfaction
            {
                StylistName = $"{s.FirstName} {s.LastName}",
                AverageRating = (double)s.Rating,
                TotalAppointments = appointments.Count(a => a.StylistId == s.Id),
                SatisfiedCustomers = appointments.Count(a => a.StylistId == s.Id && a.Status == AppointmentStatus.Completed)
            }).ToList();
        }

        private List<BranchPerformance> GetBranchPerformance(IEnumerable<Branch> branches, IEnumerable<Appointment> appointments)
        {
            return branches.Select(b => new BranchPerformance
            {
                Id = b.Id,
                Name = b.Name,
                AppointmentCount = appointments.Count(a => a.BranchId == b.Id),
                TotalRevenue = appointments.Where(a => a.BranchId == b.Id).Sum(a => a.FinalPrice),
                AverageRating = 4.5 // Bu değer gerçek veriden hesaplanmalı
            }).OrderByDescending(b => b.TotalRevenue).ToList();
        }

        private List<BranchUtilization> GetBranchUtilization(IEnumerable<Branch> branches, IEnumerable<Appointment> appointments, DateTime from, DateTime to)
        {
            var totalHours = (to - from).TotalHours;
            return branches.Select(b => new BranchUtilization
            {
                BranchName = b.Name,
                TotalHours = totalHours,
                BookedHours = appointments.Where(a => a.BranchId == b.Id).Sum(a => (a.EndAt - a.StartAt).TotalHours),
                UtilizationRate = CalculatePercentage(
                    (double)appointments.Where(a => a.BranchId == b.Id).Sum(a => (a.EndAt - a.StartAt).TotalHours),
                    totalHours)
            }).ToList();
        }

        // Chart data helper metodlar
        private ChartData GetRevenueByWeek(IEnumerable<Appointment> appointments, DateTime from, DateTime to)
        {
            var revenueByWeek = appointments
                .GroupBy(a => GetWeekOfYear(a.StartAt))
                .Select(g => new ChartDataPoint
                {
                    Label = $"Hafta {g.Key}",
                    Value = (double)g.Sum(a => a.FinalPrice)
                })
                .OrderBy(w => w.Label)
                .ToList();

            return new ChartData
            {
                Labels = revenueByWeek.Select(w => w.Label).ToList(),
                Values = revenueByWeek.Select(w => w.Value).ToList(),
                Title = "Haftalık Gelir"
            };
        }

        private ChartData GetRevenueByMonth(IEnumerable<Appointment> appointments, DateTime from, DateTime to)
        {
            var revenueByMonth = appointments
                .GroupBy(a => a.StartAt.ToString("yyyy-MM"))
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key,
                    Value = (double)g.Sum(a => a.FinalPrice)
                })
                .OrderBy(m => m.Label)
                .ToList();

            return new ChartData
            {
                Labels = revenueByMonth.Select(m => m.Label).ToList(),
                Values = revenueByMonth.Select(m => m.Value).ToList(),
                Title = "Aylık Gelir"
            };
        }

        private ChartData GetAppointmentsByWeek(IEnumerable<Appointment> appointments, DateTime from, DateTime to)
        {
            var appointmentsByWeek = appointments
                .GroupBy(a => GetWeekOfYear(a.StartAt))
                .Select(g => new ChartDataPoint
                {
                    Label = $"Hafta {g.Key}",
                    Value = g.Count()
                })
                .OrderBy(w => w.Label)
                .ToList();

            return new ChartData
            {
                Labels = appointmentsByWeek.Select(w => w.Label).ToList(),
                Values = appointmentsByWeek.Select(w => w.Value).ToList(),
                Title = "Haftalık Randevu Sayısı"
            };
        }

        private ChartData GetAppointmentsByMonth(IEnumerable<Appointment> appointments, DateTime from, DateTime to)
        {
            var appointmentsByMonth = appointments
                .GroupBy(a => a.StartAt.ToString("yyyy-MM"))
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .OrderBy(m => m.Label)
                .ToList();

            return new ChartData
            {
                Labels = appointmentsByMonth.Select(m => m.Label).ToList(),
                Values = appointmentsByMonth.Select(m => m.Value).ToList(),
                Title = "Aylık Randevu Sayısı"
            };
        }

        private ChartData GetCustomersByDay(IEnumerable<Customer> customers, DateTime from, DateTime to)
        {
            var customersByDay = customers
                .Where(c => c.CreatedAt >= from && c.CreatedAt <= to)
                .GroupBy(c => c.CreatedAt.Date)
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key.ToString("dd/MM/yyyy"),
                    Value = g.Count()
                })
                .OrderBy(d => d.Label)
                .ToList();

            return new ChartData
            {
                Labels = customersByDay.Select(d => d.Label).ToList(),
                Values = customersByDay.Select(d => d.Value).ToList(),
                Title = "Günlük Yeni Müşteri Sayısı"
            };
        }

        private ChartData GetCustomersByWeek(IEnumerable<Customer> customers, DateTime from, DateTime to)
        {
            var customersByWeek = customers
                .Where(c => c.CreatedAt >= from && c.CreatedAt <= to)
                .GroupBy(c => GetWeekOfYear(c.CreatedAt))
                .Select(g => new ChartDataPoint
                {
                    Label = $"Hafta {g.Key}",
                    Value = g.Count()
                })
                .OrderBy(w => w.Label)
                .ToList();

            return new ChartData
            {
                Labels = customersByWeek.Select(w => w.Label).ToList(),
                Values = customersByWeek.Select(w => w.Value).ToList(),
                Title = "Haftalık Yeni Müşteri Sayısı"
            };
        }

        private ChartData GetCustomersByMonth(IEnumerable<Customer> customers, DateTime from, DateTime to)
        {
            var customersByMonth = customers
                .Where(c => c.CreatedAt >= from && c.CreatedAt <= to)
                .GroupBy(c => c.CreatedAt.ToString("yyyy-MM"))
                .Select(g => new ChartDataPoint
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .OrderBy(m => m.Label)
                .ToList();

            return new ChartData
            {
                Labels = customersByMonth.Select(m => m.Label).ToList(),
                Values = customersByMonth.Select(m => m.Value).ToList(),
                Title = "Aylık Yeni Müşteri Sayısı"
            };
        }

        private int GetWeekOfYear(DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        private List<MonthlyComparison> GetMonthlyComparison(IEnumerable<Appointment> currentYear, IEnumerable<Appointment> previousYear)
        {
            var months = Enumerable.Range(1, 12);
            return months.Select(month => new MonthlyComparison
            {
                Month = month,
                CurrentYearRevenue = currentYear.Where(a => a.StartAt.Month == month).Sum(a => a.FinalPrice),
                PreviousYearRevenue = previousYear.Where(a => a.StartAt.Month == month).Sum(a => a.FinalPrice),
                CurrentYearAppointments = currentYear.Count(a => a.StartAt.Month == month),
                PreviousYearAppointments = previousYear.Count(a => a.StartAt.Month == month)
            }).ToList();
        }

        Task<byte[]> IReportingService.ExportToCsv<T>(IEnumerable<T> data, string fileName)
        {
            throw new NotImplementedException();
        }

        Task<byte[]> IReportingService.ExportToExcel<T>(IEnumerable<T> data, string fileName, string sheetName)
        {
            throw new NotImplementedException();
        }
    }
}
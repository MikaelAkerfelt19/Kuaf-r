using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Models.Entities.Analytics;

namespace Kuafor.Web.Controllers.Api.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IAppointmentService _appointmentService;
        private readonly IFinancialAnalyticsService _financialService;

        public ExportController(ICustomerService customerService, IAppointmentService appointmentService, IFinancialAnalyticsService financialService)
        {
            _customerService = customerService;
            _appointmentService = appointmentService;
            _financialService = financialService;
        }

        [HttpGet("customers/excel")]
        public async Task<IActionResult> ExportCustomersToExcel()
        {
            // Müşteri listesini Excel formatında export eder
            var customers = await _customerService.GetAllAsync();
            var stream = await GenerateCustomersExcel(customers);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "musteriler.xlsx");
        }

        [HttpPost("customers/import")]
        public async Task<IActionResult> ImportCustomersFromExcel(IFormFile file)
        {
            // Excel dosyasından müşteri listesi import eder
            var customers = await ParseCustomersFromExcel(file);
            await _customerService.BulkCreateAsync(customers); 
            return Ok(new { Message = "Müşteriler başarıyla içe aktarıldı", Count = customers.Count });
        }

        [HttpGet("appointments/excel")]
        public async Task<IActionResult> ExportAppointmentsToExcel(DateTime? startDate, DateTime? endDate)
        {
            // Randevu listesini Excel formatında export eder
            var appointments = await _appointmentService.GetByDateRangeAsync(startDate ?? DateTime.Now.AddMonths(-1), endDate ?? DateTime.Now);
            var stream = await GenerateAppointmentsExcel(appointments);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "randevular.xlsx");
        }

        [HttpGet("financial/excel")]
        public async Task<IActionResult> ExportFinancialReportToExcel(DateTime startDate, DateTime endDate)
        {
            // Finansal raporları Excel formatında export eder
            var report = await _financialService.GenerateFinancialReportAsync(startDate, endDate);
            var stream = await GenerateFinancialExcel(report);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "finansal_rapor.xlsx");
        }

        // Talep edilen specific route'lar
        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            try
            {
                // Filename'e göre dosya türünü belirle
                if (fileName.StartsWith("appointments_"))
                {
                    var appointments = await _appointmentService.GetAllAsync();
                    var stream = await GenerateAppointmentsExcel(appointments);
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
                else if (fileName.StartsWith("customers_"))
                {
                    var customers = await _customerService.GetAllAsync();
                    var stream = await GenerateCustomersExcel(customers);
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
                else if (fileName.StartsWith("financial_"))
                {
                    var report = await _financialService.GenerateFinancialReportAsync(DateTime.Now.AddMonths(-1), DateTime.Now);
                    var stream = await GenerateFinancialExcel(report);
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
                
                return NotFound("Dosya bulunamadı");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("all/excel")]
        public async Task<IActionResult> ExportAllToExcel()
        {
            try
            {
                // Tüm verileri tek Excel dosyasında export et
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage();
                
                // Müşteriler
                var customers = await _customerService.GetAllAsync();
                var customerSheet = package.Workbook.Worksheets.Add("Müşteriler");
                customerSheet.Cells[1, 1].Value = "Ad Soyad";
                customerSheet.Cells[1, 2].Value = "Telefon";
                customerSheet.Cells[1, 3].Value = "Email";
                customerSheet.Cells[1, 4].Value = "Kayıt Tarihi";
                
                int row = 2;
                foreach (var customer in customers)
                {
                    customerSheet.Cells[row, 1].Value = customer.FullName;
                    customerSheet.Cells[row, 2].Value = customer.Phone;
                    customerSheet.Cells[row, 3].Value = customer.Email;
                    customerSheet.Cells[row, 4].Value = customer.CreatedAt.ToString("dd/MM/yyyy");
                    row++;
                }
                
                // Randevular
                var appointments = await _appointmentService.GetAllAsync();
                var appointmentSheet = package.Workbook.Worksheets.Add("Randevular");
                appointmentSheet.Cells[1, 1].Value = "Tarih";
                appointmentSheet.Cells[1, 2].Value = "Müşteri";
                appointmentSheet.Cells[1, 3].Value = "Hizmet";
                appointmentSheet.Cells[1, 4].Value = "Durum";
                appointmentSheet.Cells[1, 5].Value = "Fiyat";
                
                row = 2;
                foreach (var appointment in appointments)
                {
                    appointmentSheet.Cells[row, 1].Value = appointment.StartAt.ToString("dd/MM/yyyy HH:mm");
                    appointmentSheet.Cells[row, 2].Value = appointment.Customer?.FullName;
                    appointmentSheet.Cells[row, 3].Value = appointment.Service?.Name;
                    appointmentSheet.Cells[row, 4].Value = appointment.Status.ToString();
                    appointmentSheet.Cells[row, 5].Value = appointment.FinalPrice;
                    row++;
                }
                
                // Auto-fit columns
                customerSheet.Cells.AutoFitColumns();
                appointmentSheet.Cells.AutoFitColumns();
                
                var stream = new MemoryStream();
                await package.SaveAsAsync(stream);
                stream.Position = 0;
                
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"tum_veriler_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private async Task<MemoryStream> GenerateCustomersExcel(IEnumerable<Customer> customers)
        {
            // Excel dosyası oluşturur ve müşteri verilerini yazar
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Müşteriler");
            
            // Header yazma ve veri doldurma işlemleri
            worksheet.Cells[1, 1].Value = "Ad Soyad";
            worksheet.Cells[1, 2].Value = "Telefon";
            worksheet.Cells[1, 3].Value = "Email";
            
            int row = 2;
            foreach (var customer in customers)
            {
                worksheet.Cells[row, 1].Value = customer.FullName;
                worksheet.Cells[row, 2].Value = customer.Phone;
                worksheet.Cells[row, 3].Value = customer.Email;
                row++;
            }
            
            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);
            stream.Position = 0;
            return stream;
        }

        private Task<List<Customer>> ParseCustomersFromExcel(IFormFile file)
        {
            return Task.Run(() =>
            {
                // Excel dosyasını okur ve müşteri listesi oluşturur
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var customers = new List<Customer>();
            using var stream = file.OpenReadStream();
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            
            for (int row = 2; row <= worksheet.Dimension.Rows; row++)
            {
                var fullName = worksheet.Cells[row, 1].Text;
                var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                customers.Add(new Customer
                {
                    FirstName = nameParts.FirstOrDefault() ?? "",
                    LastName = nameParts.Skip(1).FirstOrDefault() ?? "",
                    Phone = worksheet.Cells[row, 2].Text,
                    Email = worksheet.Cells[row, 3].Text,
                    UserId = Guid.NewGuid().ToString() // Required field
                });
                }
                
                return customers;
            });
        }

        private async Task<MemoryStream> GenerateAppointmentsExcel(IEnumerable<Appointment> appointments)
        {
            // Randevu verilerini Excel formatında hazırlar
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Randevular");
            
            worksheet.Cells[1, 1].Value = "Tarih";
            worksheet.Cells[1, 2].Value = "Müşteri";
            worksheet.Cells[1, 3].Value = "Hizmet";
            worksheet.Cells[1, 4].Value = "Durum";
            
            int row = 2;
            foreach (var appointment in appointments)
            {
                worksheet.Cells[row, 1].Value = appointment.StartAt.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cells[row, 2].Value = appointment.Customer?.FullName;
                worksheet.Cells[row, 3].Value = appointment.Service?.Name;
                worksheet.Cells[row, 4].Value = appointment.Status.ToString();
                row++;
            }
            
            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);
            stream.Position = 0;
            return stream;
        }

        private async Task<MemoryStream> GenerateFinancialExcel(object report)
        {
            // Finansal rapor Excel dosyası oluşturur
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Finansal Rapor");
            
            // Finansal veri yazma işlemleri
            
            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);
            stream.Position = 0;
            return stream;
        }
    }
}

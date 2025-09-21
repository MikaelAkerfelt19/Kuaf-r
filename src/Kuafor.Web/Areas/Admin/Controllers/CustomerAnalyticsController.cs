using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Admin;
using Kuafor.Web.Models.Entities;
using CustomerEntity = Kuafor.Web.Models.Entities.Customer;
using OfficeOpenXml;


namespace Kuafor.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/[controller]")]
public class CustomerAnalyticsController : Controller
{
    private readonly ICustomerAnalyticsService _customerAnalyticsService;
    private readonly ICustomerService _customerService;
    private readonly IAppointmentService _appointmentService;

    public CustomerAnalyticsController(
        ICustomerAnalyticsService customerAnalyticsService,
        ICustomerService customerService,
        IAppointmentService appointmentService)
    {
        _customerAnalyticsService = customerAnalyticsService;
        _customerService = customerService;
        _appointmentService = appointmentService;
    }

    // GET: /Admin/CustomerAnalytics
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var customers = await _customerService.GetAllAsync();
            var customerAnalytics = new CustomerAnalyticsViewModel
            {
                TotalCustomers = customers.Count(),
                NewCustomersThisMonth = customers.Count(c => c.CreatedAt >= DateTime.Now.AddMonths(-1)),
                ActiveCustomers = customers.Count(c => c.IsActive),
                TotalAppointments = 0, // GetTotalCountAsync method'u yok, placeholder
                CustomerList = customers.Take(50).Cast<object>().ToList()
            };

            return View(customerAnalytics);
        }
        catch (Exception)
        {
            var emptyAnalytics = new CustomerAnalyticsViewModel
            {
                TotalCustomers = 0,
                NewCustomersThisMonth = 0,
                ActiveCustomers = 0,
                TotalAppointments = 0,
                CustomerList = new List<object>()
            };
            return View(emptyAnalytics);
        }
    }

    // GET: /Admin/CustomerAnalytics/Details/5
    [HttpGet]
    [Route("Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var appointments = await _appointmentService.GetByCustomerIdAsync(id);
            var customerDetails = new CustomerDetailsViewModel
            {
                CustomerInfo = customer,
                AppointmentList = appointments.ToList(),
                TotalSpentAmount = appointments.Sum(a => a.FinalPrice),
                LastVisitDate = appointments.OrderByDescending(a => a.StartAt).FirstOrDefault()?.StartAt
            };

            return View(customerDetails);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    // POST: /Admin/CustomerAnalytics/Details/5 - Edit customer details
    [HttpPost]
    [Route("Details/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Details(int id, CustomerEntity customer)
    {
        if (id != customer.Id)
        {
            return NotFound();
        }

        try
        {
            if (ModelState.IsValid)
            {
                customer.UpdatedAt = DateTime.UtcNow;
                await _customerService.UpdateAsync(customer);
                TempData["Success"] = "Müşteri bilgileri başarıyla güncellendi.";
                return RedirectToAction(nameof(Details), new { id = customer.Id });
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Güncelleme hatası: {ex.Message}";
        }

        // Hata durumunda appointments'ları tekrar yükle
        var appointments = await _appointmentService.GetByCustomerIdAsync(id);
        var customerDetails = new CustomerDetailsViewModel
        {
            CustomerInfo = customer,
            AppointmentList = appointments.ToList(),
            TotalSpentAmount = appointments.Sum(a => a.FinalPrice),
            LastVisitDate = appointments.OrderByDescending(a => a.StartAt).FirstOrDefault()?.StartAt
        };

        return View(customerDetails);
    }

    // GET: /Admin/CustomerAnalytics/Delete/5
    [HttpGet]
    [Route("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    // POST: /Admin/CustomerAnalytics/Delete/5
    [HttpPost]
    [Route("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            await _customerService.DeleteAsync(id);
            TempData["Success"] = "Müşteri başarıyla silindi";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Müşteri silinirken hata oluştu: " + ex.Message;
            return RedirectToAction("Index");
        }
    }

    // GET: /Admin/CustomerAnalytics/Export
    [HttpGet]
    [Route("Export")]
    public async Task<IActionResult> Export()
    {
        try
        {
            var customers = await _customerService.GetAllAsync();
            var appointments = await _appointmentService.GetAllAsync();
            
            // Excel dosyası oluştur
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Müşteri Analitikleri");
            
            // Header
            worksheet.Cells[1, 1].Value = "Müşteri ID";
            worksheet.Cells[1, 2].Value = "Ad Soyad";
            worksheet.Cells[1, 3].Value = "E-posta";
            worksheet.Cells[1, 4].Value = "Telefon";
            worksheet.Cells[1, 5].Value = "Kayıt Tarihi";
            worksheet.Cells[1, 6].Value = "Toplam Randevu";
            worksheet.Cells[1, 7].Value = "Toplam Harcama";
            worksheet.Cells[1, 8].Value = "Son Randevu";
            worksheet.Cells[1, 9].Value = "Durum";
            
            // Veri doldur
            int row = 2;
            foreach (var customer in customers)
            {
                var customerAppointments = appointments.Where(a => a.CustomerId == customer.Id);
                var totalSpent = customerAppointments.Sum(a => a.FinalPrice);
                var lastVisit = customerAppointments.OrderByDescending(a => a.StartAt).FirstOrDefault()?.StartAt;
                
                worksheet.Cells[row, 1].Value = customer.Id;
                worksheet.Cells[row, 2].Value = customer.FullName;
                worksheet.Cells[row, 3].Value = customer.Email;
                worksheet.Cells[row, 4].Value = customer.Phone;
                worksheet.Cells[row, 5].Value = customer.CreatedAt.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 6].Value = customerAppointments.Count();
                worksheet.Cells[row, 7].Value = totalSpent;
                worksheet.Cells[row, 8].Value = lastVisit?.ToString("dd/MM/yyyy") ?? "Hiç";
                worksheet.Cells[row, 9].Value = customer.IsActive ? "Aktif" : "Pasif";
                
                row++;
            }
            
            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();
            
            var stream = new MemoryStream();
            await package.SaveAsAsync(stream);
            stream.Position = 0;
            
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                $"musteri_analitikleri_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Export hatası: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    [HttpGet("customer-segments")]
    [Route("CustomerSegments")]
    public async Task<IActionResult> CustomerSegments()
    {
        // Müşteri segmentasyonu sayfası
        try
        {
            var segments = await _customerAnalyticsService.GetCustomerSegmentationAsync();
            return View(segments);
        }
        catch (Exception)
        {
            return View(new List<object>());
        }
    }

    [HttpGet("risk-analysis")]
    [Route("RiskAnalysis")]
    public async Task<IActionResult> RiskAnalysis()
    {
        // Risk altındaki müşteriler sayfası
        try
        {
            var riskCustomers = await _customerAnalyticsService.GetCustomersAtRiskAsync();
            return View(riskCustomers);
        }
        catch (Exception)
        {
            return View(new List<CustomerEntity>());
        }
    }

    [HttpGet("behavior-analysis")]
    [Route("BehaviorAnalysis")]
    public async Task<IActionResult> BehaviorAnalysis()
    {
        // Müşteri davranış analizi sayfası
        try
        {
            var behaviorPatterns = await _customerAnalyticsService.AnalyzeCustomerBehaviorAsync();
            return View(behaviorPatterns);
        }
        catch (Exception)
        {
            return View(new List<object>());
        }
    }

    [HttpGet("customer-ltv/{customerId:int}")]
    [Route("CustomerLTV/{customerId:int}")]
    public async Task<IActionResult> CustomerLTV(int customerId)
    {
        // Müşteri yaşam boyu değeri detayı
        try
        {
            var ltv = await _customerAnalyticsService.CalculateCustomerLTVAsync(customerId);
            return Json(new { success = true, data = ltv });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("bulk-message")]
    public async Task<IActionResult> SendBulkMessage([FromBody] BulkMessageRequest request)
    {
        // Seçili müşterilere toplu mesaj gönderme
        try
        {
            await _customerService.SendBulkMessageAsync(request.CustomerIds, request.Message, request.MessageType);
            return Json(new { success = true, message = "Mesajlar başarıyla gönderildi" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("update-customer")]
    [Route("UpdateCustomer")]
    public async Task<IActionResult> UpdateCustomer([FromBody] CustomerUpdateRequest request)
    {
        try
        {
            var customer = await _customerService.GetByIdAsync(request.Id);
            if (customer == null)
            {
                return Json(new { success = false, message = "Müşteri bulunamadı" });
            }

            // Müşteri bilgilerini güncelle
            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.Email = request.Email;
            customer.Phone = request.Phone;
            customer.DateOfBirth = request.DateOfBirth;
            customer.Gender = request.Gender;
            customer.UpdatedAt = DateTime.UtcNow;

            await _customerService.UpdateAsync(customer);
            
            return Json(new { success = true, message = "Müşteri bilgileri başarıyla güncellendi" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    // Yardımcı model sınıfları
    public class BulkMessageRequest
    {
        public List<int> CustomerIds { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public string MessageType { get; set; } = "SMS"; // SMS, WhatsApp
    }

    public class CustomerUpdateRequest
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
    }
}
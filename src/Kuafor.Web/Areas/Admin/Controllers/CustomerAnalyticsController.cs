using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Admin;
using Kuafor.Web.Models.Entities;
using CustomerEntity = Kuafor.Web.Models.Entities.Customer;


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
                Customer = customer,
                Appointments = appointments.ToList(),
                TotalSpent = appointments.Sum(a => a.FinalPrice),
                LastVisit = appointments.OrderByDescending(a => a.StartAt).FirstOrDefault()?.StartAt
            };

            return View(customerDetails);
        }
        catch (Exception)
        {
            return NotFound();
        }
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
            // CSV export logic would go here
            return Json(new { message = "Export functionality will be implemented" });
        }
        catch (Exception)
        {
            return Json(new { error = "Export failed" });
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

    // Yardımcı model sınıfları
    public class BulkMessageRequest
    {
        public List<int> CustomerIds { get; set; } = new();
        public string Message { get; set; } = string.Empty;
        public string MessageType { get; set; } = "SMS"; // SMS, WhatsApp
    }
}
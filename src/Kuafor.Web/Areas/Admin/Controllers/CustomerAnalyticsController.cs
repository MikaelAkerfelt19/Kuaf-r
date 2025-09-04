using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomerAnalyticsController : Controller
    {
        private readonly ICustomerAnalyticsService _analyticsService;
        private readonly ICustomerService _customerService;

        public CustomerAnalyticsController(ICustomerAnalyticsService analyticsService, ICustomerService customerService)
        {
            _analyticsService = analyticsService;
            _customerService = customerService;
        }

        // GET: /Admin/CustomerAnalytics
        public async Task<IActionResult> Index()
        {
            var report = await _analyticsService.GetAnalyticsReportAsync();
            var segmentPerformance = await _analyticsService.GetSegmentPerformanceAsync();
            var atRiskCustomers = await _analyticsService.GetAtRiskCustomersAsync(10);
            var highValueCustomers = await _analyticsService.GetHighValueCustomersAsync(10);

            ViewBag.Report = report;
            ViewBag.SegmentPerformance = segmentPerformance;
            ViewBag.AtRiskCustomers = atRiskCustomers;
            ViewBag.HighValueCustomers = highValueCustomers;

            return View();
        }

        // GET: /Admin/CustomerAnalytics/Segments
        public async Task<IActionResult> Segments()
        {
            var segments = await _analyticsService.GetCustomerSegmentsAsync();
            return View(segments);
        }

        // GET: /Admin/CustomerAnalytics/SegmentDetails/VIP
        public async Task<IActionResult> SegmentDetails(string segment)
        {
            var customers = await _analyticsService.GetCustomersBySegmentAsync(segment);
            ViewBag.SegmentName = segment;
            return View(customers);
        }

        // GET: /Admin/CustomerAnalytics/CustomerDetails/5
        public async Task<IActionResult> CustomerDetails(int id)
        {
            var analytics = await _analyticsService.GetCustomerAnalyticsAsync(id);
            if (analytics == null) return NotFound();

            var journey = await _analyticsService.GetCustomerJourneyAsync(id);
            var preferences = await _analyticsService.GetCustomerPreferencesAsync(id);
            var behaviors = await _analyticsService.GetCustomerBehaviorsAsync(id, DateTime.UtcNow.AddDays(-30));

            ViewBag.Journey = journey;
            ViewBag.Preferences = preferences;
            ViewBag.Behaviors = behaviors;

            return View(analytics);
        }

        // GET: /Admin/CustomerAnalytics/ChurnRisk
        public async Task<IActionResult> ChurnRisk()
        {
            var atRiskCustomers = await _analyticsService.GetChurnRiskCustomersAsync(50);
            return View(atRiskCustomers);
        }

        // POST: /Admin/CustomerAnalytics/RecalculateRFM
        [HttpPost]
        public async Task<IActionResult> RecalculateRFM()
        {
            try
            {
                await _analyticsService.CalculateAllRFMAsync();
                TempData["Success"] = "RFM analizi başarıyla güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"RFM analizi güncellenirken hata oluştu: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/CustomerAnalytics/UpdateSegment
        [HttpPost]
        public async Task<IActionResult> UpdateSegment(int customerId, string segment)
        {
            try
            {
                await _analyticsService.UpdateCustomerSegmentAsync(customerId);
                TempData["Success"] = "Müşteri segmenti güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Segment güncellenirken hata oluştu: {ex.Message}";
            }

            return RedirectToAction(nameof(CustomerDetails), new { id = customerId });
        }

        // API: Müşteri analitik verileri
        [HttpGet]
        public async Task<IActionResult> GetAnalyticsData()
        {
            var report = await _analyticsService.GetAnalyticsReportAsync();
            return Json(report);
        }

        // API: Segment performansı
        [HttpGet]
        public async Task<IActionResult> GetSegmentPerformance()
        {
            var performance = await _analyticsService.GetSegmentPerformanceAsync();
            return Json(performance);
        }

        // API: Churn risk analizi
        [HttpGet]
        public async Task<IActionResult> GetChurnRiskData()
        {
            var atRiskCustomers = await _analyticsService.GetChurnRiskCustomersAsync(20);
            return Json(atRiskCustomers.Select(c => new
            {
                customerId = c.CustomerId,
                customerName = $"{c.Customer.FirstName} {c.Customer.LastName}",
                churnRisk = c.ChurnRisk,
                segment = c.Segment,
                lastVisit = c.LastActivityDate?.ToString("yyyy-MM-dd"),
                totalSpent = c.MonetaryScore
            }));
        }
    }
}



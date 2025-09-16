using Microsoft.AspNetCore.Mvc;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Areas.Admin.Models;

namespace Kuafor.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MessagingController : Controller
    {
        private readonly IMessagingService _messagingService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<MessagingController> _logger;

        public MessagingController(
            IMessagingService messagingService,
            ICustomerService customerService,
            ILogger<MessagingController> logger)
        {
            _messagingService = messagingService;
            _customerService = customerService;
            _logger = logger;
        }

        // Ana mesajlaşma sayfası 
        public IActionResult Index()
        {
            return View();
        }

        // Grup yönetimi 
        public async Task<IActionResult> GroupMessaging()
        {
            var groups = await _messagingService.GetAllGroupsAsync();
            var viewModel = new GroupMessagingViewModel
            {
                Groups = groups,
                SelectedGroupIds = new List<int>()
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendGroupMessage(GroupMessagingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Groups = await _messagingService.GetAllGroupsAsync();
                return View("GroupMessaging", model);
            }

            foreach (var groupId in model.SelectedGroupIds)
            {
                var success = await _messagingService.SendGroupMessageAsync(
                    groupId, model.Message, model.MessageType);
                
                if (!success)
                {
                    TempData["ErrorMessage"] = "Bazı gruplara mesaj gönderilemedi.";
                }
            }

            TempData["SuccessMessage"] = "Mesajlar gönderildi.";
            return RedirectToAction(nameof(GroupMessaging));
        }

        // Toplu mesajlaşma 
        public async Task<IActionResult> BulkMessaging()
        {
            var customers = await _customerService.GetAllAsync();
            var viewModel = new BulkMessagingViewModel
            {
                Customers = customers.ToList(),
                SelectedCustomerIds = new List<int>(),
                MessageType = "WhatsApp"
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendBulkMessage(BulkMessagingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Customers = (await _customerService.GetAllAsync()).ToList();
                return View("BulkMessaging", model);
            }

            var success = await _messagingService.SendBulkMessageAsync(
                model.SelectedCustomerIds, model.Message, model.MessageType);

            if (success)
            {
                TempData["SuccessMessage"] = "Toplu mesaj gönderildi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Mesaj gönderilirken hata oluştu.";
            }

            return RedirectToAction(nameof(BulkMessaging));
        }

        // Filtreli mesajlaşma 
        public async Task<IActionResult> FilteredMessaging()
        {
            var customers = await _customerService.GetAllAsync();
            var viewModel = new FilteredMessagingViewModel
            {
                Customers = customers.ToList(),
                Filter = new CustomerFilter(),
                ExcludeCustomerIds = new List<int>(),
                MessageType = "WhatsApp"
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ApplyFilter(FilteredMessagingViewModel model)
        {
            var filteredCustomers = await _messagingService.GetFilteredCustomersAsync(model.Filter);
            model.Customers = filteredCustomers;
            return View("FilteredMessaging", model);
        }

        [HttpPost]
        public async Task<IActionResult> SendFilteredMessage(FilteredMessagingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("FilteredMessaging", model);
            }

            var success = await _messagingService.SendFilteredMessageAsync(
                model.Filter, model.Message, model.MessageType, model.ExcludeCustomerIds);

            if (success)
            {
                TempData["SuccessMessage"] = "Filtreli mesaj gönderildi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Mesaj gönderilirken hata oluştu.";
            }

            return RedirectToAction(nameof(FilteredMessaging));
        }

        // Mesaj raporları 
        public async Task<IActionResult> MessageReports()
        {
            var reports = await _messagingService.GetMessageReportsAsync();
            var viewModel = new MessageReportsViewModel
            {
                Reports = reports,
                FromDate = DateTime.Now.AddDays(-30),
                ToDate = DateTime.Now,
                MessageType = "All"
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> FilterReports(MessageReportsViewModel model)
        {
            var reports = await _messagingService.GetMessageReportsAsync(
                model.FromDate, model.ToDate, 
                model.MessageType == "All" ? null : model.MessageType);
            
            model.Reports = reports;
            return View("MessageReports", model);
        }

        // Kara liste yönetimi 
        public async Task<IActionResult> MessageBlacklist()
        {
            var blacklistedNumbers = await _messagingService.GetBlacklistedNumbersAsync();
            return View(blacklistedNumbers);
        }

        [HttpPost]
        public async Task<IActionResult> AddToBlacklist(string phoneNumber, string reason, string? customerName = null, string? notes = null)
        {
            var success = await _messagingService.AddToBlacklistAsync(phoneNumber, reason, customerName, notes);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Numara kara listeye eklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kara listeye eklenirken hata oluştu.";
            }

            return RedirectToAction(nameof(MessageBlacklist));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromBlacklist(string phoneNumber)
        {
            var success = await _messagingService.RemoveFromBlacklistAsync(phoneNumber);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Numara kara listeden çıkarıldı.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kara listeden çıkarılırken hata oluştu.";
            }

            return RedirectToAction(nameof(MessageBlacklist));
        }

        // Grup yönetimi API'leri
        [HttpPost]
        public async Task<IActionResult> CreateGroup(string name, string? description = null)
        {
            var group = await _messagingService.CreateGroupAsync(name, description);
            return Json(new { success = true, groupId = group.Id });
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomerToGroup(int groupId, int customerId)
        {
            var success = await _messagingService.AddCustomerToGroupAsync(groupId, customerId);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCustomerFromGroup(int groupId, int customerId)
        {
            var success = await _messagingService.RemoveCustomerFromGroupAsync(groupId, customerId);
            return Json(new { success });
        }

        // Kredi bilgileri API'si
        [HttpGet]
        public async Task<IActionResult> GetCredits()
        {
            var smsCredit = await _messagingService.GetCreditAsync("SMS");
            var whatsappCredit = await _messagingService.GetCreditAsync("WhatsApp");
            
            return Json(new 
            { 
                sms = smsCredit.CreditAmount,
                whatsapp = whatsappCredit.CreditAmount
            });
        }
    }
}

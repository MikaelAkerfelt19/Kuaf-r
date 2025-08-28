using Kuafor.Web.Models.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IAppointmentService _appointmentService;
        private readonly IBranchService _branchService;
        private readonly IStylistService _stylistService;
        private readonly ICouponService _couponService;
        private readonly ILoyaltyService _loyaltyService;

        public ProfileController(
            ICustomerService customerService,
            IAppointmentService appointmentService,
            IBranchService branchService,
            IStylistService stylistService,
            ICouponService couponService,
            ILoyaltyService loyaltyService)
        {
            _customerService = customerService;
            _appointmentService = appointmentService;
            _branchService = branchService;
            _stylistService = stylistService;
            _couponService = couponService;
            _loyaltyService = loyaltyService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var customer = await _customerService.GetByUserIdAsync(userId);
            if (customer == null)
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            // Yaklaşan randevuları al
            var upcomingAppointments = await _appointmentService.GetUpcomingByCustomerAsync(customer.Id);
            var nextAppointment = upcomingAppointments.FirstOrDefault();

            // Şube ve kuaför seçeneklerini al
            var branches = await _branchService.GetActiveAsync();
            var stylists = await _stylistService.GetActiveAsync();

            // Müşteri kuponlarını al
            var customerCoupons = await _couponService.GetActiveForCustomerAsync(customer.Id);

            // Loyalty bilgilerini al
            var loyalty = await _loyaltyService.GetByCustomerIdAsync(customer.Id);

            var vm = new ProfilePageViewModel
            {
                Identity = new IdentityViewModel
                {
                    FullName = $"{customer.FirstName} {customer.LastName}",
                    DisplayName = customer.FirstName,
                    BirthDate = customer.DateOfBirth,
                    Email = customer.Email ?? string.Empty,
                    Phone = customer.Phone ?? string.Empty,
                    WhatsappOptIn = true, 
                    AvatarUrl = "/images/avatars/default.png"
                },
                Addresses = new AddressesViewModel
                {
                    Items = new List<AddressItemVm>() // Şimdilik boş, gelecekte Address entity eklenecek
                },
                Notifications = new NotificationsViewModel
                {
                    Email = true,
                    Sms = true,
                    Push = false,
                    WhatsApp = true,
                    Reminders = true,
                    Campaigns = true,
                    Critical = true,
                    QuietFrom = new TimeSpan(22, 0, 0),
                    QuietTo = new TimeSpan(8, 0, 0)
                },
                Preferences = new PreferencesViewModel
                {
                    PreferredBranch = branches.FirstOrDefault()?.Name ?? "Merkez",
                    PreferredStylist = stylists.FirstOrDefault()?.FirstName + " " + stylists.FirstOrDefault()?.LastName ?? "Ahmet Özdoğan",
                    PreferredTimeBand = "Hafta içi 18:00-21:00",
                    FlexMinutes = 15,
                    BranchOptions = branches.Select(b => b.Name).ToList(),
                    StylistOptions = stylists.Select(s => $"{s.FirstName} {s.LastName}").ToList(),
                    TimeBandOptions = new List<string>
                    {
                        "Hafta içi 10:00-13:00",
                        "Hafta içi 13:00-17:00",
                        "Hafta içi 18:00-21:00",
                        "Hafta sonu 10:00-14:00"
                    },
                    QuickRebooks = new List<QuickRebookItem>() // Şimdilik boş
                },
                Loyalty = new LoyaltyViewModel
                {
                    Tier = loyalty?.Tier ?? "Bronz",
                    Points = loyalty?.Points ?? 0,
                    NextTierPoints = 200,
                    Benefits = new List<string>
                    {
                        "Randevu hatırlatmada öncelik",
                        "Seçili hizmetlerde %5 indirim",
                        "Doğum gününde ekstra kupon"
                    }
                },
                Coupons = new CouponsViewModel
                {
                    Active = customerCoupons.Select(c => new CouponVm(
                        c.Code,
                        c.Title,
                        c.DiscountType == "Percent" ? $"%{c.Amount} indirim" : $"₺{c.Amount} indirim",
                        c.ExpiresAt ?? DateTime.Today.AddMonths(1),
                        c.MinSpend,
                        c.DiscountType == "Percent" ? $"%{c.Amount}" : $"₺{c.Amount}",
                        false
                    )).ToList(),
                    Expired = new List<CouponVm>()
                },
                Security = new SecurityViewModel
                {
                    ChangePassword = new ChangePasswordModel(),
                    TwoFactor = new TwoFactorSetupModel
                    {
                        IsEnabled = false,
                        SecretKey = "JBSWY3DPEHPK3PXP",
                        OtpauthUri = $"otpauth://totp/xxx-hairdresser:{customer.Email ?? "customer"}?secret=JBSWY3DPEHPK3PXP&issuer=xxx-hairdresser",
                        RecoveryCodes = new List<string>
                        {
                            "5XK2-7QHT", "9LMP-2CVA", "R7QZ-3JTY", "Z9ND-4HKK",
                            "PQ2N-8WLA", "T7YU-1MME", "V3SA-6QQD", "H2JD-5LLX"
                        }
                    },
                    ActiveSessions = new List<ActiveSessionVm>
                    {
                        new("s_cur","Windows · PC","Chrome 126", DateTime.Now.AddMinutes(-5), true)
                    }
                },
                Payments = new PaymentsViewModel
                {
                    Cards = new List<SavedCardVm>(), // Şimdilik boş, gelecekte Payment entity eklenecek
                    Billing = new BillingInfoViewModel
                    {
                        InvoiceType = "Bireysel",
                        FullNameOrCompany = $"{customer.FirstName} {customer.LastName}",
                        TaxNumber = "",
                        AddressLine1 = "",
                        City = "",
                        District = "",
                        Zip = "",
                        Email = customer.Email ?? string.Empty,
                        EInvoiceOptIn = true
                    }
                },
                Privacy = new PrivacyViewModel
                {
                    ConsentEmail = true,
                    ConsentSms = false,
                    ConsentPush = false,
                    ConsentWhatsApp = true,
                    ConsentedAt = DateTime.Today.AddDays(-10),
                    ConsentedIp = "203.0.113.42",
                    ExportReady = true,
                    LastExportFileName = "export_2023-03-15.json"
                }
            };

            // Yaklaşan randevu bilgisini ViewBag'e ekle
            if (nextAppointment != null)
            {
                ViewBag.NextAppointment = nextAppointment;
            }

            return View(vm);
        }
    }
}
using Kuafor.Web.Models.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace Kuafor.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            var vm = new ProfilePageViewModel
            {
                Identity = new IdentityViewModel
                {
                    FullName = User?.Identity?.Name ?? "Misafir Kullanıcı",
                    DisplayName = "feas",
                    BirthDate = new DateTime(2003, 7, 15),
                    Email = "user@example.com",
                    Phone = "+90 5xx xxx xx xx",
                    WhatsappOptIn = true,
                    AvatarUrl = "/images/avatars/default.png"
                },
                Addresses = new AddressesViewModel
                {
                    Items =
                    {
                        new AddressItemVm(1,"Ev","Boğaçhan Sk. No:10/3","İstanbul","Kadıköy","34710",true),
                        new AddressItemVm(2,"İş","Rıhtım Cd. No:21","İstanbul","Kadıköy","34716",false),
                    }
                },
                Notifications = new NotificationsViewModel
                {
                    Email = true, Sms = true, Push = false, WhatsApp = true,
                    Reminders = true, Campaigns = true, Critical = true,
                    QuietFrom = new TimeSpan(22, 0, 0), QuietTo = new TimeSpan(8, 0, 0)
                },
                Preferences = new PreferencesViewModel
                {
                    PreferredBranch = "Merkez",
                    PreferredStylist = "Ahmet Özdoğan",
                    PreferredTimeBand = "Hafta içi 18:00-21:00",
                    FlexMinutes = 15,
                    BranchOptions = { "Merkez", "İldem", "Talas" },
                    StylistOptions = { "Ahmet Özdoğan", "Ahmet Ülker", "İbrahim Taşkın" },
                    TimeBandOptions =
                    {
                        "Hafta içi 10:00-13:00",
                        "Hafta içi 13:00-17:00",
                        "Hafta içi 18:00-21:00",
                        "Hafta sonu 10:00-14:00"
                    },
                    QuickRebooks = {
                                        // (<serviceId>, <stylistId>, <label>)
                        new QuickRebookItem(1, 10, "Saç Kesimi · Ahmet Ö."),
                        new QuickRebookItem(3, 12, "Bakım & Spa · İbrahim T."),
                    }
                },
                Loyalty = new LoyaltyViewModel
                {
                    Tier = "Gümüş",
                    Points = 120,
                    NextTierPoints = 200,
                    Benefits =
                    {
                        "Randevu hatırlatmada öncelik",
                        "Seçili hizmetlerde %5 indirim",
                        "Doğum gününde ekstra kupon"
                    }
                },
                Coupons = new CouponsViewModel
                {
                    Active =
                    {
                        new CouponVm("FEAS10","Hoş Geldin İndirimi","İlk randevunda geçerli",
                                     DateTime.Today.AddDays(30),  null, "%10", false),
                        new CouponVm("SPA50","Bakım Kampanyası","Bakım & Spa için",
                                     DateTime.Today.AddDays(10),  300m, "₺50", false),
                    },
                    Expired =
                    {
                        new CouponVm("SUMMER15","Yaz İndirimi", "Belirli hizmetlerde",
                                     DateTime.Today.AddDays(-3), null, "%15", false),
                        new CouponVm("USED20","Kullanılmış Kupon", "Geçmiş sipariş",
                                     DateTime.Today.AddDays(5),  null, "%20", true)
                    }
                }
            };
            return View(vm);
        }
    }
}
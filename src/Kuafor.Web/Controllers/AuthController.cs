using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Kuafor.Web.ViewModels;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Data;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Account; // Reset* view modellerin burada varsayılıyor

namespace Kuafor.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        // Şifre sıfırlama akışı (SMS + MemoryCache)
        private readonly ISmsService _sms;
        private readonly IMemoryCache _cache;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IJwtService jwtService,
            ISmsService sms,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _jwtService = jwtService;
            _sms = sms;
            _cache = cache;
        }

        // ==========================
        // LOGIN
        // ==========================

        [HttpGet]
        public IActionResult Login()
        {
            // Yorum: Login view’i döner; modal açılışı view tarafında TempData ile.
            return View("_LoginModal", new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Yorum: CSRF koruması
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Yorum: Modal’ı tekrar açmak için işaret bırak
                TempData["OpenModal"] = "Login";
                return View("_LoginModal", model);
            }

            // Yorum: SignInManager.PasswordSignInAsync parametre olarak "KULLANICI ADI" ister.
            // Bu nedenle önce kullanıcıyı username ya da e-mail ile bulup, SignIn’i user.UserName ile yapıyoruz.
            var user =
                await _userManager.FindByNameAsync(model.UserNameOrEmail)
                ?? await _userManager.FindByEmailAsync(model.UserNameOrEmail);

            if (user == null )
            {
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
                TempData["OpenModal"] = "Login";
                return View("_LoginModal",model);
            }

            // Yorum: lockoutOnFailure:true dersen, varsayılan policy’e göre hatalı denemelerde hesap kilitlenir.
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                // Yorum: Kilitlenmişse kullanıcıya özel mesaj verilebilir.
                if (result.IsLockedOut)
                    ModelState.AddModelError(string.Empty, "Çok fazla hatalı deneme. Lütfen bir süre sonra tekrar deneyin.");
                else
                    ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");

                TempData["OpenModal"] = "Login";
                return View("_LoginModal", model);
            }

            // Yorum: Giriş başarılı → roller alınır, JWT üretilir, hedef sayfaya yönlendirilir.
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user.Id, user.Email ?? string.Empty, roles.ToList());

            TempData["Success"] = "Giriş işlemi başarıyla tamamlandı!";
            TempData["JwtToken"] = token;

            if (roles.Contains("Admin"))
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            if (roles.Contains("Customer"))
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            if (roles.Contains("Stylist"))
                return RedirectToAction("Index", "Dashboard", new { area = "Stylist" });

            return RedirectToAction("Index", "Home");
        }

        // ==========================
        // REGISTER
        // ==========================

        [HttpGet]
        public IActionResult Register()
        {
            // Yorum: Modal açılışını view tarafında TempData ile tetikleyebilirsin.
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Yorum: CSRF koruması
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["OpenModal"] = "Register";
                return View(model);
            }

            // Yorum: Kullanıcı adı ve e-posta benzersizliği
            if (await _userManager.FindByNameAsync(model.UserName) is not null)
            {
                ModelState.AddModelError(nameof(model.UserName), "Bu kullanıcı adı zaten kullanılıyor.");
                TempData["OpenModal"] = "Register";
                return View(model);
            }

            if (await _userManager.FindByEmailAsync(model.Email) is not null)
            {
                ModelState.AddModelError(nameof(model.Email), "Bu e-posta adresi zaten kullanılıyor.");
                TempData["OpenModal"] = "Register";
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = true // Yorum: E-posta onayı istemiyorsan true kalabilir.
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var e in createResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                TempData["OpenModal"] = "Register";
                return View(model);
            }

            // Yorum: Varsayılan rol Customer
            await _userManager.AddToRoleAsync(user, "Customer");

            // Yorum: Domain entity kaydı
            var customer = new Customer
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                UserId = user.Id,
                IsActive = true
            };

            try
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Yorum: Başarılı kayıt → otomatik login
                await _signInManager.SignInAsync(user, isPersistent: false);

                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.GenerateToken(user.Id, user.Email ?? string.Empty, roles.ToList());

                TempData["Success"] = "Kayıt işlemi başarıyla tamamlandı!";
                TempData["JwtToken"] = token;

                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }
            catch (Exception ex)
            {
                // Yorum: Domain kayıt hatası → Identity user’ı geri al
                await _userManager.DeleteAsync(user);
                ModelState.AddModelError(string.Empty, "Kayıt sırasında veritabanı hatası: " + ex.Message);
                TempData["OpenModal"] = "Register";
                return View(model);
            }
        }

        // ==========================
        // LOGOUT & ACCESSDENIED
        // ==========================

        [HttpPost]
        [ValidateAntiForgeryToken] // Yorum: Logout’u POST yapmak daha güvenli
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                TempData["Success"] = "Çıkış işlemi başarıyla tamamlandı!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Çıkış işlemi sırasında bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return RedirectToAction("Index", "Home");
        }

        // ==========================
        // ŞİFRE SIFIRLAMA (SMS 2FA ile)
        // ==========================
        // Not: Login modalındaki linki asp-action="ForgotPassword" bırakırsan
        // aşağıdaki aksiyon ResetStart'a yönlendirir. (UI dokunmadan akış oturur)

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            // Yorum: Eski linklerle uyumluluk için redirect.
            return RedirectToAction(nameof(ResetStart));
        }

        // STEP 1: Kimlik bilgisini al (email veya kullanıcı adı), SMS kodu gönder
        [HttpGet]
        public IActionResult ResetStart() => View(new ResetStartViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetStart(ResetStartViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Yorum: Kullanıcıyı e-posta ya da kullanıcı adına göre bul
            IdentityUser? user = model.EmailOrUserName.Contains("@")
                ? await _userManager.FindByEmailAsync(model.EmailOrUserName)
                : await _userManager.FindByNameAsync(model.EmailOrUserName);

            // Yorum: Kullanıcıyı ifşa etme → yanıt hep aynı
            if (user is null || string.IsNullOrWhiteSpace(user.PhoneNumber) ||
                !(await _userManager.IsPhoneNumberConfirmedAsync(user)))
            {
                TempData["Info"] = "Eğer bilgiler doğruysa telefonunuza kod gönderildi.";

                // Yorum: Dummy session, güvenlik için gerçek kullanıcı bilgisi yok
                var dummy = new ResetSession
                {
                    Sid = Guid.NewGuid().ToString("N"),
                    UserId = string.Empty,
                    Email = string.Empty,
                    PhoneNumber = string.Empty,
                    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5)
                };

                _cache.Set($"reset:{dummy.Sid}", dummy, TimeSpan.FromMinutes(5));
                return RedirectToAction(nameof(ResetVerify), new { sid = dummy.Sid });
            }

            // Yorum: Gerçek session
            var session = new ResetSession
            {
                // Sid ctor’da üretiliyor varsayımı → değilse Guid üret
                // Sid boş gelirse:
                // Sid = Guid.NewGuid().ToString("N"),
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber!,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
                NextSendAllowedUtc = DateTime.UtcNow
            };

            // Yorum: Sid garanti; yoksa ekle
            if (string.IsNullOrWhiteSpace(session.Sid))
                session.Sid = Guid.NewGuid().ToString("N");

            _cache.Set($"reset:{session.Sid}", session, TimeSpan.FromMinutes(5));

            await SendSmsCodeAsync(user, session, force: true);

            TempData["Info"] = "Eğer bilgiler doğruysa telefonunuza kod gönderildi.";
            return RedirectToAction(nameof(ResetVerify), new { sid = session.Sid });
        }

        // Yorum: SMS kodu gönderimi (Throttle: 60 sn)
        private async Task SendSmsCodeAsync(IdentityUser user, ResetSession session, bool force = false)
        {
            if (!force && DateTime.UtcNow < session.NextSendAllowedUtc) return;

            // Yorum: Varsayılan telefon sağlayıcısı için 2FA kodu üret
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
            var message = $"Şifre sıfırlama kodunuz: {code}. 5 dakika içinde kullanın.";
            await _sms.SendSmsAsync(session.PhoneNumber, message);

            session.NextSendAllowedUtc = DateTime.UtcNow.AddSeconds(60);
            _cache.Set($"reset:{session.Sid}", session, session.ExpiresAtUtc - DateTime.UtcNow);
        }

        // STEP 2: Kod doğrulama
        [HttpGet]
        public IActionResult ResetVerify(string sid)
        {
            if (string.IsNullOrWhiteSpace(sid)) return BadRequest();
            return View(new ResetVerifyViewModel { Sid = sid });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetVerify(ResetVerifyViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (!_cache.TryGetValue<ResetSession>($"reset:{model.Sid}", out var session) ||
                DateTime.UtcNow > session!.ExpiresAtUtc)
            {
                ModelState.AddModelError(string.Empty, "Oturum süreniz doldu. Lütfen baştan deneyin.");
                return View(model);
            }

            if (string.IsNullOrEmpty(session.UserId))
            {
                // Yorum: Dummy oturumda daima başarısız
                ModelState.AddModelError(string.Empty, "Kod doğrulanamadı.");
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(session.UserId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Kod doğrulanamadı.");
                return View(model);
            }

            // Yorum: 2FA kodu kontrolü
            var valid = await _userManager.VerifyTwoFactorTokenAsync(
                user, TokenOptions.DefaultPhoneProvider, model.Code);

            if (!valid)
            {
                ModelState.AddModelError(string.Empty, "Kod hatalı veya süresi doldu.");
                return View(model);
            }

            // Yorum: Doğrulandı → kısa süreli yeni süre tanımla
            session.Verified = true;
            session.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5);
            _cache.Set($"reset:{session.Sid}", session, session.ExpiresAtUtc - DateTime.UtcNow);

            return RedirectToAction(nameof(ResetPassword), new { sid = session.Sid });
        }

        // STEP 3: Yeni şifre belirleme
        [HttpGet]
        public IActionResult ResetPassword(string sid)
        {
            if (string.IsNullOrWhiteSpace(sid)) return BadRequest();
            return View(new ResetPasswordViewModel { Sid = sid });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (!_cache.TryGetValue<ResetSession>($"reset:{model.Sid}", out var session) ||
                !session!.Verified || DateTime.UtcNow > session.ExpiresAtUtc)
            {
                ModelState.AddModelError(string.Empty, "Oturum geçersiz veya süresi doldu.");
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(session.UserId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "İşlem gerçekleştirilemedi.");
                return View(model);
            }

            // Yorum: Identity reset token’ı üret ve yeni şifreyi set et
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            _cache.Remove($"reset:{session.Sid}");
            return RedirectToAction(nameof(ResetDone));
        }

        [HttpGet]
        public IActionResult ResetDone() => View();
    }
}

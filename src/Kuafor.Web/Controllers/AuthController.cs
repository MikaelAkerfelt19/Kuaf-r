using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Kuafor.Web.ViewModels;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Data;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Controllers;

public class AuthController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _userSignInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> userSignInManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _userSignInManager = userSignInManager;
        _roleManager = roleManager;
        _context = context;
        _jwtService = jwtService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        // Login sayfasını göster, sayfa içinde modal otomatik açılacak
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _userSignInManager.PasswordSignInAsync(
                model.UserNameOrEmail, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
                if (user == null)
                    user = await _userManager.FindByNameAsync(model.UserNameOrEmail);

                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // JWT Token oluştur
                    var token = _jwtService.GenerateToken(user.Id, user.Email ?? "", roles.ToList());

                    // Başarılı giriş mesajı
                    TempData["Success"] = "Giriş işlemi başarıyla tamamlandı!";
                    TempData["JwtToken"] = token; // Token'ı TempData'ya ekle (isteğe bağlı)

                    if (roles.Contains("Admin"))
                    {
                        // Admin kullanıcısını Admin area'daki Dashboard'a yönlendir
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }
                    else if (roles.Contains("Customer"))
                    {
                        // Customer kullanıcısını Customer area'daki Home'a yönlendir
                        return RedirectToAction("Index", "Home", new { area = "Customer" });
                    }
                    else if (roles.Contains("Stylist"))
                    {
                        // Stylist kullanıcısını Stylist area'daki Dashboard'a yönlendir
                        return RedirectToAction("Index", "Dashboard", new { area = "Stylist" });
                    }
                    else
                    {
                        // Varsayılan olarak ana sayfaya yönlendir
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ModelState.AddModelError("", "Geçersiz giriş denemesi");
        }

        // Hata durumunda login sayfasına dön ve modal'ı tekrar göster
        TempData["OpenModal"] = "Login";
        TempData["Error"] = "Geçersiz giriş denemesi";
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        // Register sayfasını göster, sayfa içinde modal otomatik açılacak
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Kullanıcı adı ve email benzersizlik kontrolü
            if (await _userManager.FindByNameAsync(model.UserName) != null)
            {
                ModelState.AddModelError("UserName", "Bu kullanıcı adı zaten kullanılıyor");
                TempData["OpenModal"] = "Register";
                TempData["Error"] = "Bu kullanıcı adı zaten kullanılıyor";
                return View(model);
            }

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor");
                TempData["OpenModal"] = "Register";
                TempData["Error"] = "Bu e-posta adresi zaten kullanılıyor";
                return View(model);
            }

            // Identity User oluştur
            var user = new IdentityUser 
            { 
                UserName = model.UserName, 
                Email = model.Email,
                EmailConfirmed = true // E-posta onayı gerekmiyor
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Customer rolü ekle
                await _userManager.AddToRoleAsync(user, "Customer");

                // Customer entity oluştur
                var customer = new Customer
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    UserId = user.Id,
                    IsActive = true
                    // CreatedAt default değeri DateTime.UtcNow olarak ayarlanacak
                };

                try
                {
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                    
                    // Başarılı kayıt sonrası otomatik giriş
                    await _userSignInManager.SignInAsync(user, isPersistent: false);
                    
                    // JWT Token oluştur
                    var roles = await _userManager.GetRolesAsync(user);
                    var token = _jwtService.GenerateToken(user.Id, user.Email ?? "", roles.ToList());
                    
                    TempData["Success"] = "Kayıt işlemi başarıyla tamamlandı!";
                    TempData["JwtToken"] = token; // Token'ı TempData'ya ekle (isteğe bağlı)
                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                }
                catch (Exception ex)
                {
                    // Customer kaydı başarısız olursa Identity user'ı da sil
                    await _userManager.DeleteAsync(user);
                    ModelState.AddModelError("", "Veritabanı hatası: " + ex.Message);
                    TempData["Error"] = "Kayıt sırasında veritabanı hatası oluştu";
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
        }

        // Hata durumunda register sayfasına dön ve modal'ı tekrar göster
        TempData["OpenModal"] = "Register";
        TempData["Error"] = "Kayıt işlemi sırasında bir hata oluştu";
        return View(model);
    }

    [HttpPost]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _userSignInManager.SignOutAsync();
            TempData["Success"] = "Çıkış işlemi başarıyla tamamlandı!";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            // Hata durumunda log yazın ve ana sayfaya yönlendirin
            TempData["Error"] = "Çıkış işlemi sırasında bir hata oluştu: " + ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        // Access denied sayfası için ana sayfaya dön
        return RedirectToAction("Index", "Home");
    }
}

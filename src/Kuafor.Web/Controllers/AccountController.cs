using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Kuafor.Web.ViewModels;

namespace Kuafor.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _userSignInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> userSignInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _userSignInManager = userSignInManager;
        _roleManager = roleManager;
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

                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home", new { area = "Customer" });
                    }
                }
            }

            ModelState.AddModelError("", "Geçersiz giriş denemesi");
        }

        TempData["LoginError"] = "Giriş başarısız. Lütfen bilgilerinizi kontrol edin.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new IdentityUser { 
                UserName = model.UserName, 
                Email = model.Email 
            };
            
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
                await _userSignInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        TempData["RegisterError"] = "Kayıt başarısız. Lütfen bilgilerinizi kontrol edin.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _userSignInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}

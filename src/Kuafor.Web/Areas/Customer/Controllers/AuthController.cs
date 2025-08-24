using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Kuafor.Web.Areas.Customer.Controllers;

[Area("Customer")]
[Authorize(Roles = "Customer")]
public class AuthController : Controller
{
    private readonly SignInManager<IdentityUser> _userSignInManager;

    public AuthController(SignInManager<IdentityUser> userSignInManager)
    {
        _userSignInManager = userSignInManager;
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _userSignInManager.SignOutAsync();
            TempData["Success"] = "Çıkış işlemi başarıyla tamamlandı!";
            return RedirectToAction("Index", "Home", new { area = "" });
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Çıkış işlemi sırasında bir hata oluştu: " + ex.Message;
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}

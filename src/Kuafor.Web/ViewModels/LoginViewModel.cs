using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı veya E-posta gereklidir")]
    public string UserNameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre gereklidir")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Account
{
    /// <summary>
    /// SMS doğrulamasından sonra şifre yenileme ekranında kullanılan model.
    /// </summary>
    public class ResetPasswordViewModel
    {
        [Required]
        public string Sid { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre zorunludur.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [Display(Name = "Yeni Şifre")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

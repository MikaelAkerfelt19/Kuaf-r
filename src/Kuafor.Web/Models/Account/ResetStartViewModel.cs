using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Account
{
    /// <summary>
    /// Şifre sıfırlama işlemini başlatmak için kullanıcıdan alınan bilgiler.
    /// </summary>
    public class ResetStartViewModel
    {
        [Required(ErrorMessage = "E-posta veya kullanıcı adı zorunludur.")]
        [Display(Name = "E-posta veya Kullanıcı Adı")]
        public string EmailOrUserName { get; set; } = string.Empty;
    }
}

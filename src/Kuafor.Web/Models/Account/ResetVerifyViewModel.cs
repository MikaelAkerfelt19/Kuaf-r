using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Account
{
    /// <summary>
    /// SMS ile gönderilen 6 haneli kodun girildiği aşamada kullanılan model.
    /// </summary>
    public class ResetVerifyViewModel
    {
        [Required]
        public string Sid { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğrulama kodu zorunludur.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "6 haneli kod giriniz.")]
        [Display(Name = "Doğrulama Kodu")]
        public string Code { get; set; } = string.Empty;
    }
}

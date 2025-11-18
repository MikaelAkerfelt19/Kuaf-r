using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad gereklidir")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad gereklidir")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon numarası gereklidir")]
    [RegularExpression(@"^[1-9][0-9]{9}$", ErrorMessage = "Lütfen 0 yazmadan 10 haneli geçerli bir numara giriniz.")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Telefon numarası 10 haneli olmalıdır")]
    public string Phone { get; set; } = string.Empty;


    [Required(ErrorMessage = "Şifre gereklidir")]
    [StringLength(100, ErrorMessage = "{0} en az {2} karakter uzunluğunda olmalıdır", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kullanıcı sözleşmesini kabul etmelisiniz")]
    public bool Terms { get; set; }

    public bool Newsletter { get; set; }


}

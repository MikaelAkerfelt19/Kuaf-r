using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Admin.Coupons
{
    public enum DiscountType
    {
        Percent = 1,   // %
        Amount = 2    // ₺
    }

    // Liste satırı için DTO
    public class CouponDto
    {
        public int Id { get; set; }

        [Required, StringLength(40)]
        public string Code { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DiscountType DiscountType { get; set; } = DiscountType.Percent;

        [Range(0, 999999)]
        public decimal Amount { get; set; } = 0m;

        [Range(0, 999999)]
        public decimal? MinSpend { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // Ekle/Düzenle form modeli
    public class CouponFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kod zorunludur.")]
        [StringLength(40, ErrorMessage = "Kod 40 karakteri aşamaz.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(120, ErrorMessage = "Başlık 120 karakteri aşamaz.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "İndirim tipi zorunludur.")]
        public DiscountType DiscountType { get; set; } = DiscountType.Percent;

        [Range(0, 999999, ErrorMessage = "Tutar 0-999999 aralığında olmalıdır.")]
        public decimal Amount { get; set; } = 0m;

        [Range(0, 999999, ErrorMessage = "Asgari harcama 0-999999 aralığında olmalıdır.")]
        public decimal? MinSpend { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

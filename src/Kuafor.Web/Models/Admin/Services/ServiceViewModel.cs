using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Admin.Services
{
    // Liste ve form için kullanılan DTO
    public class ServiceDto
    {
        public int Id { get; set; }
        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;
        [Range(5, 480)]
        public int DurationMin { get; set; } = 30;
        [Range(0, 100000)]
        public decimal Price { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    // Ekle/Düzenle için form modeli
    public class ServiceFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(80, ErrorMessage = "Ad 80 karakteri aşamaz.")]
        public string Name { get; set; } = string.Empty;
        [Range(5, 480, ErrorMessage = "Süre 5-480 dk aralığında olmalıdır.")]
        public int DurationMin { get; set; } = 30;
        [Range(0, 100000, ErrorMessage = "Fiyat 0-100000 aralığında olmalıdır.")]
        public decimal Price { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}

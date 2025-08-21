using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kuafor.Web.Models.Admin.Stylists
{
    // Basit Branch DTO (mock şube)
    public class BranchDto
    {
        public int Id { get; set; }
        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
    }

    // Liste ve tablo satırı için kuaför DTO
    public class StylistDto
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 5)]
        public decimal Rating { get; set; } = 0m;

        [StringLength(500)]
        public string? Bio { get; set; }

        [Range(1, int.MaxValue)]
        public int BranchId { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // Ekle/Düzenle form modeli
    public class StylistFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(80, ErrorMessage = "Ad 80 karakteri aşamaz.")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 5, ErrorMessage = "Puan 0-5 aralığında olmalıdır.")]
        public decimal Rating { get; set; } = 0m;

        [StringLength(500, ErrorMessage = "Biyografi 500 karakteri aşamaz.")]
        public string? Bio { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Şube zorunludur.")]
        public int BranchId { get; set; }

        public bool IsActive { get; set; } = true;

        // Dropdown için
        public List<SelectListItem> BranchOptions { get; set; } = new();
    }

    // Index sayfası için sayfa modeli
    public class StylistsPageViewModel
    {
        public List<StylistDto> Stylists { get; set; } = new();
        public Dictionary<int, string> BranchNames { get; set; } = new();
        public List<SelectListItem> BranchOptions { get; set; } = new();
    }
}

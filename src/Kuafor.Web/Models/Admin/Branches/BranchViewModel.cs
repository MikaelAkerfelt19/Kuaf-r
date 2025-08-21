using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Admin.Branches
{
    // Liste için DTO
    public class BranchDto
    {
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [StringLength(160)]
        public string Address { get; set; } = string.Empty;

        [StringLength(30)]
        public string Phone { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    // Ekle/Düzenle form modeli
    public class BranchFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad zorunludur.")]
        [StringLength(80, ErrorMessage = "Ad 80 karakteri aşamaz.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(160, ErrorMessage = "Adres 160 karakteri aşamaz.")]
        public string Address { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "Telefon 30 karakteri aşamaz.")]
        public string Phone { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}

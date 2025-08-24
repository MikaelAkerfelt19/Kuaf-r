using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Admin.Appointments
{
    // AppointmentStatus enum'u kaldırıldı, string kullanılıyor

    // Liste/tabloda gösterilecek temel DTO
    public class AppointmentDto
    {
        public int Id { get; set; }
        public DateTime StartAt { get; set; }
        public int DurationMin { get; set; } = 30;

        [Required, StringLength(80)]
        public string CustomerName { get; set; } = string.Empty;

        public int BranchId { get; set; }
        public int StylistId { get; set; }

        [Required, StringLength(80)]
        public string ServiceName { get; set; } = string.Empty;

        public string Status { get; set; } = "Scheduled";

        // Kolaylık için (tabloya hızlı yansıtmak üzere)
        public decimal Price { get; set; } = 0m;
        public string? Note { get; set; }
    }

    // Filtre formu
    public class AppointmentFilter
    {
        [DataType(DataType.Date)]
        public DateTime? From { get; set; }

        [DataType(DataType.Date)]
        public DateTime? To { get; set; }

        public int? BranchId { get; set; }
        public int? StylistId { get; set; }
        public string? Status { get; set; }
    }

    // Ertele formu
    public class RescheduleForm
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Yeni tarih/saat zorunludur.")]
        public DateTime NewStartAt { get; set; }

        [Range(5, 480)]
        public int DurationMin { get; set; } = 30;
    }

    // İptal formu
    public class CancelForm
    {
        [Required]
        public int Id { get; set; }

        [StringLength(200)]
        public string? Reason { get; set; }
    }

    // Sayfa modeli
    public class AppointmentsPageViewModel
    {
        public AppointmentFilter Filter { get; set; } = new();
        public List<AppointmentDto> Items { get; set; } = new();

        // Görsel ihtiyaçlar
        public Dictionary<int, string> BranchNames { get; set; } = new();
        public Dictionary<int, string> StylistNames { get; set; } = new();
    }
}

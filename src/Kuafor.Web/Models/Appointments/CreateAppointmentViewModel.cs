using System.ComponentModel.DataAnnotations;
using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Models.Appointments
{
    public class CreateAppointmentViewModel
    {
        [Required(ErrorMessage = "Hizmet seçimi zorunludur.")]
        [Display(Name = "Hizmet")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Şube seçimi zorunludur.")]
        [Display(Name = "Şube")]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "Kuaför seçimi zorunludur.")]
        [Display(Name = "Kuaför")]
        public int StylistId { get; set; }

        [Required(ErrorMessage = "Randevu tarihi ve saati zorunludur.")]
        [Display(Name = "Randevu Tarihi ve Saati")]
        public DateTime StartAt { get; set; }

        [Display(Name = "Süre (Dakika)")]
        public int DurationMin { get; set; } = 30;

        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olabilir.")]
        [Display(Name = "Notlar")]
        public string? Notes { get; set; }

        // Navigation properties for dropdowns
        public IEnumerable<Service> Services { get; set; } = new List<Service>();
        public IEnumerable<Branch> Branches { get; set; } = new List<Branch>();
        public IEnumerable<Models.Entities.Stylist> Stylists { get; set; } = new List<Models.Entities.Stylist>();

        // Selected service details (for duration calculation)
        public string? SelectedServiceName { get; set; }
        public decimal SelectedServicePrice { get; set; }
    }
}
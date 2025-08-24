using System.ComponentModel.DataAnnotations;

namespace Kuafor.Web.Models.Enums;

public enum AppointmentStatus
{
    Pending = 0,      // Onay bekliyor
    Confirmed = 1,    // Onaylandı
    InProgress = 2,   // Hizmet veriliyor
    Completed = 3,    // Tamamlandı
    Cancelled = 4,    // İptal edildi
    NoShow = 5,       // Gelmedi
    Rescheduled = 6   // Erteklendi
}

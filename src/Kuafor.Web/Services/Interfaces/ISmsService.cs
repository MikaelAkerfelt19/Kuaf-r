using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);
        Task<bool> SendAppointmentConfirmationAsync(Appointment appointment);
        Task<bool> SendAppointmentReminderAsync(Appointment appointment);
        Task<bool> SendAppointmentCancellationAsync(Appointment appointment);
        Task<bool> SendAppointmentRescheduleAsync(Appointment appointment, DateTime newDateTime);
        Task<bool> SendWelcomeMessageAsync(Customer customer);
        Task<bool> SendPromotionalMessageAsync(string phoneNumber, string message);
    }
}

using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces
{
    public interface IWhatsAppService
    {
        Task<bool> SendMessageAsync(string phoneNumber, string message);
        Task<bool> SendAppointmentConfirmationAsync(Appointment appointment);
        Task<bool> SendAppointmentReminderAsync(Appointment appointment);
        Task<bool> SendAppointmentCancellationAsync(Appointment appointment);
        Task<bool> SendAppointmentRescheduleAsync(Appointment appointment, DateTime newDateTime);
        Task<bool> SendWelcomeMessageAsync(Customer customer);
        Task<bool> SendPromotionalMessageAsync(string phoneNumber, string message);
        Task<bool> SendTemplateMessageAsync(string phoneNumber, string templateName, Dictionary<string, string> parameters);
        Task<bool> SendMediaMessageAsync(string phoneNumber, string mediaUrl, string caption = "");
    }
}
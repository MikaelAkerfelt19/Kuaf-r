using System.Net.Mail;
using System.Net;

namespace Kuafor.Web.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendAppointmentConfirmationAsync(int appointmentId);
    Task SendAppointmentReminderAsync(int appointmentId);
    Task SendCancellationNotificationAsync(int appointmentId);
}



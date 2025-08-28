using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly SmtpClient _smtpClient;
    private readonly IAppointmentService _appointmentService;
    private readonly ITemplateService _templateService;
    
    public EmailService(
        IConfiguration configuration,
        IAppointmentService appointmentService,
        ITemplateService templateService)
    {
        _configuration = configuration;
        _appointmentService = appointmentService;
        _templateService = templateService;
        
        _smtpClient = new SmtpClient
        {
            Host = _configuration["Email:SmtpHost"] ?? "localhost",
            Port = int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
            EnableSsl = true,
            Credentials = new NetworkCredential(
                _configuration["Email:Username"] ?? "",
                _configuration["Email:Password"] ?? ""
            )
        };
    }
    
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_configuration["Email:From"] ?? "noreply@kuafor.com"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        message.To.Add(to);
        
        await _smtpClient.SendMailAsync(message);
    }
    
    public async Task SendAppointmentConfirmationAsync(int appointmentId)
    {
        var appointment = await _appointmentService.GetByIdAsync(appointmentId);
        if (appointment?.Customer?.Email == null) return;
        
        var template = _templateService.GetTemplate("AppointmentConfirmation");
        var body = await _templateService.RenderTemplateAsync("AppointmentConfirmation", new { appointment });
        
        await SendEmailAsync(
            appointment.Customer.Email,
            "Randevu Onaylandı",
            body
        );
    }
    
    public async Task SendAppointmentReminderAsync(int appointmentId)
    {
        var appointment = await _appointmentService.GetByIdAsync(appointmentId);
        if (appointment?.Customer?.Email == null) return;
        
        var template = _templateService.GetTemplate("AppointmentReminder");
        var body = await _templateService.RenderTemplateAsync("AppointmentReminder", new { appointment });
        
        await SendEmailAsync(
            appointment.Customer.Email,
            "Randevu Hatırlatması",
            body
        );
    }
    
    public async Task SendCancellationNotificationAsync(int appointmentId)
    {
        var appointment = await _appointmentService.GetByIdAsync(appointmentId);
        if (appointment?.Customer?.Email == null) return;
        
        var template = _templateService.GetTemplate("AppointmentCancellation");
        var body = await _templateService.RenderTemplateAsync("AppointmentCancellation", new { appointment });
        
        await SendEmailAsync(
            appointment.Customer.Email,
            "Randevu İptal Edildi",
            body
        );
    }
    
    public void Dispose()
    {
        _smtpClient?.Dispose();
    }
}

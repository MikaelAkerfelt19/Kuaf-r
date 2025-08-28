using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Kuafor.Web.Services.Interfaces;
using Kuafor.Web.Models.Enums;

namespace Kuafor.Web.Services.BackgroundServices;

public class AppointmentReminderService : BackgroundService
{
    private readonly ILogger<AppointmentReminderService> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public AppointmentReminderService(
        ILogger<AppointmentReminderService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendRemindersAsync();
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken); // 15 dakikada bir kontrol
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hatırlatma gönderimi sırasında hata");
            }
        }
    }
    
    private async Task SendRemindersAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var appointmentService = scope.ServiceProvider.GetRequiredService<IAppointmentService>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        
        var now = DateTime.UtcNow;
        var reminderTime = now.AddHours(24); // 24 saat önce
        
        var appointments = await appointmentService.GetByDateAsync(reminderTime.Date);
        
        foreach (var appointment in appointments.Where(a => a.Status == AppointmentStatus.Confirmed))
        {
            if (appointment.StartAt.Date == reminderTime.Date && 
                appointment.StartAt.Hour == reminderTime.Hour)
            {
                try
                {
                    await emailService.SendAppointmentReminderAsync(appointment.Id);
                    _logger.LogInformation("Hatırlatma gönderildi: Appointment {AppointmentId}", appointment.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Hatırlatma gönderilemedi: Appointment {AppointmentId}", appointment.Id);
                }
            }
        }
    }
}



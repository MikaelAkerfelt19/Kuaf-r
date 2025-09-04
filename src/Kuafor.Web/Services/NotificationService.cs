using Microsoft.EntityFrameworkCore;
using Kuafor.Web.Data;
using Kuafor.Web.Models.Entities;
using Kuafor.Web.Services.Interfaces;

namespace Kuafor.Web.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> CreateNotificationAsync(string userId, string title, string message, string type = "Info", string? actionUrl = null, string? iconClass = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ActionUrl = actionUrl,
            IconClass = iconClass,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<List<Notification>> CreateBulkNotificationsAsync(List<string> userIds, string title, string message, string type = "Info", string? actionUrl = null, string? iconClass = null)
    {
        var notifications = userIds.Select(userId => new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ActionUrl = actionUrl,
            IconClass = iconClass,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();
        return notifications;
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, int page = 1, int pageSize = 20)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Notification?> GetNotificationAsync(int notificationId)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId);
    }

    public async Task<bool> MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId);

        if (notification == null) return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId);

        if (notification == null) return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteOldNotificationsAsync(int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var oldNotifications = await _context.Notifications
            .Where(n => n.CreatedAt < cutoffDate)
            .ToListAsync();

        _context.Notifications.RemoveRange(oldNotifications);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    // Otomatik bildirimler
    public async Task SendAppointmentReminderAsync(int appointmentId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Stylist)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment?.Customer?.UserId == null) return;

        var title = "Randevu Hatırlatması";
        var message = $"Merhaba {appointment.Customer.Name}, yarın {appointment.StartAt:HH:mm} saatinde {appointment.Stylist.Name} ile {appointment.Service.Name} randevunuz bulunmaktadır.";
        
        await CreateNotificationAsync(
            appointment.Customer.UserId,
            title,
            message,
            "Info",
            $"/Customer/Appointments/{appointmentId}",
            "bi bi-calendar-check"
        );
    }

    public async Task SendAppointmentConfirmationAsync(int appointmentId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Stylist)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment?.Customer?.UserId == null) return;

        var title = "Randevu Onaylandı";
        var message = $"Randevunuz onaylandı! {appointment.StartAt:dd.MM.yyyy HH:mm} tarihinde {appointment.Stylist.Name} ile {appointment.Service.Name} hizmeti için bekliyoruz.";
        
        await CreateNotificationAsync(
            appointment.Customer.UserId,
            title,
            message,
            "Success",
            $"/Customer/Appointments/{appointmentId}",
            "bi bi-check-circle"
        );
    }

    public async Task SendAppointmentCancellationAsync(int appointmentId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Stylist)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment?.Customer?.UserId == null) return;

        var title = "Randevu İptal Edildi";
        var message = $"Maalesef {appointment.StartAt:dd.MM.yyyy HH:mm} tarihindeki randevunuz iptal edilmiştir. Yeni randevu almak için lütfen bizimle iletişime geçin.";
        
        await CreateNotificationAsync(
            appointment.Customer.UserId,
            title,
            message,
            "Warning",
            "/Customer/Appointments",
            "bi bi-x-circle"
        );
    }

    public async Task SendLowStockAlertAsync(int productId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null) return;

        // Admin kullanıcılarına bildirim gönder
        var adminUsers = await _context.Users
            .Where(u => u.Email != null)
            .Select(u => u.Id)
            .ToListAsync();

        var title = "Düşük Stok Uyarısı";
        var message = $"{product.Name} ürününün stok miktarı kritik seviyeye düştü. Mevcut stok: {product.Stock}";

        await CreateBulkNotificationsAsync(
            adminUsers,
            title,
            message,
            "Warning",
            "/Admin/Inventory",
            "bi bi-exclamation-triangle"
        );
    }

    public async Task SendPaymentReminderAsync(int appointmentId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment?.Customer?.UserId == null) return;

        var title = "Ödeme Hatırlatması";
        var message = $"Randevunuz için ödeme yapmanız gerekmektedir. Toplam tutar: {appointment.FinalPrice:C}";
        
        await CreateNotificationAsync(
            appointment.Customer.UserId,
            title,
            message,
            "Info",
            $"/Customer/Payments/{appointmentId}",
            "bi bi-credit-card"
        );
    }

    public async Task SendBirthdayWishAsync(int customerId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer?.UserId == null) return;

        var title = "Doğum Gününüz Kutlu Olsun! 🎉";
        var message = $"Sevgili {customer.Name}, doğum gününüzü kutlar, sağlık ve mutluluklar dileriz! Özel indirimlerimizden yararlanmak için randevu alabilirsiniz.";
        
        await CreateNotificationAsync(
            customer.UserId,
            title,
            message,
            "Success",
            "/Customer/Appointments/Create",
            "bi bi-gift"
        );
    }

    public async Task SendSystemMaintenanceNotificationAsync(string message)
    {
        var allUsers = await _context.Users
            .Select(u => u.Id)
            .ToListAsync();

        var title = "Sistem Bakım Bildirimi";
        
        await CreateBulkNotificationsAsync(
            allUsers,
            title,
            message,
            "Info",
            null,
            "bi bi-tools"
        );
    }

    public async Task SendNewFeatureNotificationAsync(string title, string message)
    {
        var allUsers = await _context.Users
            .Select(u => u.Id)
            .ToListAsync();
        
        await CreateBulkNotificationsAsync(
            allUsers,
            title,
            message,
            "Success",
            null,
            "bi bi-star"
        );
    }

    public async Task SendPromotionNotificationAsync(string title, string message, List<string> targetUserIds)
    {
        await CreateBulkNotificationsAsync(
            targetUserIds,
            title,
            message,
            "Success",
            "/Customer/Promotions",
            "bi bi-percent"
        );
    }
}

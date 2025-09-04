using Kuafor.Web.Models.Entities;

namespace Kuafor.Web.Services.Interfaces;

public interface INotificationService
{
    // Temel bildirim işlemleri
    Task<Notification> CreateNotificationAsync(string userId, string title, string message, string type = "Info", string? actionUrl = null, string? iconClass = null);
    Task<List<Notification>> CreateBulkNotificationsAsync(List<string> userIds, string title, string message, string type = "Info", string? actionUrl = null, string? iconClass = null);
    Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, int page = 1, int pageSize = 20);
    Task<Notification?> GetNotificationAsync(int notificationId);
    Task<bool> MarkAsReadAsync(int notificationId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<bool> DeleteNotificationAsync(int notificationId);
    Task<bool> DeleteOldNotificationsAsync(int daysOld = 30);
    Task<int> GetUnreadCountAsync(string userId);
    
    // Otomatik bildirimler
    Task SendAppointmentReminderAsync(int appointmentId);
    Task SendAppointmentConfirmationAsync(int appointmentId);
    Task SendAppointmentCancellationAsync(int appointmentId);
    Task SendLowStockAlertAsync(int productId);
    Task SendPaymentReminderAsync(int appointmentId);
    Task SendBirthdayWishAsync(int customerId);
    Task SendSystemMaintenanceNotificationAsync(string message);
    Task SendNewFeatureNotificationAsync(string title, string message);
    Task SendPromotionNotificationAsync(string title, string message, List<string> targetUserIds);
}

using BE_CinePass.Domain.Models;

namespace BE_CinePass.Domain.Interface;

public interface INotificationRepository : IRepository<Notification>
{
    Task<List<Notification>> GetUserNotificationsAsync(
        Guid userId, 
        int limit = 50, 
        bool unreadOnly = false,
        CancellationToken cancellationToken = default);
    
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    
    Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<List<Notification>> GetExpiredNotificationsAsync(CancellationToken cancellationToken = default);
}

public interface INotificationSettingsRepository : IRepository<NotificationSettings>
{
    Task<NotificationSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<NotificationSettings> CreateDefaultAsync(Guid userId, CancellationToken cancellationToken = default);
}
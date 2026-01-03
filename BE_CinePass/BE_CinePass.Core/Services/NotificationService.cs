using BE_CinePass.Core.Configurations;
using BE_CinePass.Core.Repositories;
using BE_CinePass.Domain.Models;
using BE_CinePass.Shared.DTOs.Notification;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BE_CinePass.Shared.Common;

namespace BE_CinePass.Core.Services;

/// <summary>
/// NotificationService with Level 3 Push Notification Integration
/// </summary>
public class NotificationService
{
    private readonly NotificationRepository _notificationRepository;
    private readonly NotificationSettingsRepository _settingsRepository;
    private readonly PushNotificationService _pushNotificationService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        NotificationRepository notificationRepository,
        NotificationSettingsRepository settingsRepository,
        PushNotificationService pushNotificationService,
        ApplicationDbContext context,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _settingsRepository = settingsRepository;
        _pushNotificationService = pushNotificationService;
        _context = context;
        _logger = logger;
    }

    #region Get Notifications

    public async Task<List<NotificationResponseDto>> GetUserNotificationsAsync(
        Guid userId,
        int limit = 50,
        bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetUserNotificationsAsync(
            userId, limit, unreadOnly, cancellationToken);

        return notifications.Select(MapToResponseDto).ToList();
    }

    public async Task<NotificationResponseDto?> GetByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        return notification == null ? null : MapToResponseDto(notification);
    }

    public async Task<int> GetUnreadCountAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);
    }

    #endregion

    #region Create Notifications

    /// <summary>
    /// Create notification with automatic push notification (Level 3)
    /// </summary>
    public async Task<NotificationResponseDto> CreateNotificationAsync(
        CreateNotificationDto dto,
        CancellationToken cancellationToken = default)
    {
        // Check user settings if UserId is provided
        if (dto.UserId.HasValue)
        {
            var settings = await _settingsRepository.GetByUserIdAsync(dto.UserId.Value, cancellationToken);
            if (settings != null && !ShouldSendNotification(settings, dto.Type))
            {
                _logger.LogInformation(
                    "Notification blocked by user settings. UserId: {UserId}, Type: {Type}",
                    dto.UserId.Value, dto.Type);
                
                throw new InvalidOperationException("Notification blocked by user settings");
            }

            // LEVEL 3: Check if push notifications are enabled
            if (settings != null && !settings.PushEnabled)
            {
                _logger.LogInformation(
                    "Push notifications disabled for user {UserId}",
                    dto.UserId.Value);
            }
        }

        // Create in-app notification
        var notification = new Notification
        {
            UserId = dto.UserId,
            Type = dto.Type,
            Title = dto.Title,
            Message = dto.Message,
            Data = dto.Data != null ? JsonSerializer.Serialize(dto.Data) : null,
            ImageUrl = dto.ImageUrl,
            ActionType = dto.ActionType.HasValue 
                ? dto.ActionType.Value 
                : null,
            ActionData = dto.ActionData,
            Priority = dto.Priority,
            ExpiresAt = dto.ExpiresAt,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "In-app notification created. Id: {Id}, UserId: {UserId}, Type: {Type}",
            notification.Id, notification.UserId, notification.Type);

        // LEVEL 3: Send push notification automatically
        if (notification.UserId.HasValue)
        {
            await SendPushNotificationAsync(notification, cancellationToken);
        }

        return MapToResponseDto(notification);
    }

    /// <summary>
    /// LEVEL 3: Send push notification for an in-app notification
    /// </summary>
    private async Task SendPushNotificationAsync(
        Notification notification,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!notification.UserId.HasValue)
                return;

            // Check user's push notification settings
            var settings = await _settingsRepository.GetByUserIdAsync(
                notification.UserId.Value, 
                cancellationToken);

            if (settings?.PushEnabled != true)
            {
                _logger.LogDebug(
                    "Push notifications disabled for user {UserId}",
                    notification.UserId.Value);
                return;
            }

            // Prepare data payload for deep linking
            var data = new Dictionary<string, string>
            {
                { "notificationId", notification.Id.ToString() },
                { "type", notification.Type.ToString() }
            };

            // Add action data if present
            if (!string.IsNullOrEmpty(notification.ActionData))
            {
                data["actionData"] = notification.ActionData;
            }

            if (notification.ActionType.HasValue)
            {
                data["actionType"] = notification.ActionType.Value.ToString();
            }

            // Add priority for push notification prioritization
            data["priority"] = notification.Priority.ToString();

            // Send push notification to all user's devices
            var (success, sentCount, failedCount) = await _pushNotificationService.SendToUserAsync(
                notification.UserId.Value,
                notification.Title,
                notification.Message,
                data,
                notification.ImageUrl,
                cancellationToken);

            if (success)
            {
                _logger.LogInformation(
                    "Push notification sent for notification {NotificationId}. " +
                    "Devices: {SentCount} successful, {FailedCount} failed",
                    notification.Id, sentCount, failedCount);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to send push notification for notification {NotificationId}. " +
                    "No active devices found or all sends failed.",
                    notification.Id);
            }
        }
        catch (Exception ex)
        {
            // Don't throw - push notification failure should not fail the entire operation
            _logger.LogError(
                ex, 
                "Error sending push notification for notification {NotificationId}",
                notification.Id);
        }
    }

    /// <summary>
    /// LEVEL 3: Create notifications for multiple users with push
    /// </summary>
    public async Task<List<NotificationResponseDto>> CreateNotificationsForUsersAsync(
        List<Guid> userIds,
        CreateNotificationDto dto,
        CancellationToken cancellationToken = default)
    {
        var results = new List<NotificationResponseDto>();

        foreach (var userId in userIds)
        {
            try
            {
                var userDto = new CreateNotificationDto
                {
                    UserId = userId,
                    Type = dto.Type,
                    Title = dto.Title,
                    Message = dto.Message,
                    Data = dto.Data,
                    ImageUrl = dto.ImageUrl,
                    ActionType = dto.ActionType,
                    ActionData = dto.ActionData,
                    Priority = dto.Priority,
                    ExpiresAt = dto.ExpiresAt
                };

                var result = await CreateNotificationAsync(userDto, cancellationToken);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, 
                    "Failed to create notification for user {UserId}", 
                    userId);
            }
        }

        return results;
    }

    /// <summary>
    /// LEVEL 3: Broadcast notification to all users (push only, no in-app)
    /// Use this for system-wide announcements
    /// </summary>
    public async Task<(int SuccessCount, int FailureCount)> BroadcastPushNotificationAsync(
        string title,
        string message,
        Dictionary<string, string>? data = null,
        string? imageUrl = null,
        string? platform = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Add notification type to data
            var pushData = data ?? new Dictionary<string, string>();
            if (!pushData.ContainsKey("type"))
            {
                pushData["type"] = NotificationType.OpenNotificationCenter.ToString();
            }

            var (successCount, failureCount) = await _pushNotificationService.BroadcastAsync(
                title,
                message,
                pushData,
                imageUrl,
                platform,
                cancellationToken);

            _logger.LogInformation(
                "Broadcast push notification sent. Success: {Success}, Failed: {Failed}, Platform: {Platform}",
                successCount, failureCount, platform ?? "all");

            return (successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting push notification");
            return (0, 0);
        }
    }

    #endregion

    #region Mark as Read

    public async Task<bool> MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        
        if (notification == null)
            return false;

        // Verify ownership
        if (notification.UserId != userId)
            throw new UnauthorizedAccessException("Cannot mark other user's notification");

        var result = await _notificationRepository.MarkAsReadAsync(notificationId, cancellationToken);
        if (result)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    public async Task<bool> MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);
        if (result)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Marked all notifications as read for user {UserId}", userId);
        }

        return result;
    }

    #endregion

    #region Delete Notifications

    public async Task<bool> DeleteNotificationAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);
        
        if (notification == null)
            return false;

        // Verify ownership
        if (notification.UserId != userId)
            throw new UnauthorizedAccessException("Cannot delete other user's notification");

        _notificationRepository.Remove(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<int> DeleteExpiredNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var expiredNotifications = await _notificationRepository.GetExpiredNotificationsAsync(cancellationToken);
        
        if (!expiredNotifications.Any())
            return 0;

        _notificationRepository.RemoveRange(expiredNotifications);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted {Count} expired notifications", expiredNotifications.Count);
        return expiredNotifications.Count;
    }

    #endregion

    #region LEVEL 3: Push-Only Methods

    /// <summary>
    /// LEVEL 3: Send push notification without creating in-app notification
    /// Useful for transient notifications or reminders
    /// </summary>
    public async Task<bool> SendPushOnlyAsync(
        Guid userId,
        string title,
        string message,
        Dictionary<string, string>? data = null,
        string? imageUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if push is enabled for user
            var settings = await _settingsRepository.GetByUserIdAsync(userId, cancellationToken);
            if (settings?.PushEnabled != true)
            {
                _logger.LogDebug("Push notifications disabled for user {UserId}", userId);
                return false;
            }

            var (success, sentCount, failedCount) = await _pushNotificationService.SendToUserAsync(
                userId,
                title,
                message,
                data,
                imageUrl,
                cancellationToken);

            _logger.LogInformation(
                "Push-only notification sent to user {UserId}. Success: {Success}, Devices: {Sent}/{Total}",
                userId, success, sentCount, sentCount + failedCount);

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push-only notification to user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// LEVEL 3: Send silent data notification (no alert)
    /// Useful for background data sync triggers
    /// </summary>
    public async Task<bool> SendSilentPushAsync(
        Guid userId,
        Dictionary<string, string> data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _pushNotificationService.SendDataOnlyToUserAsync(
                userId,
                data,
                cancellationToken);

            _logger.LogInformation(
                "Silent push notification sent to user {UserId}. Success: {Success}",
                userId, success);

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending silent push to user {UserId}", userId);
            return false;
        }
    }

    #endregion

    #region Helper Methods

    private bool ShouldSendNotification(NotificationSettings settings, NotificationType type)
    {
        // Map your NotificationType to settings
        return type switch
        {
            NotificationType.OpenOrderDetail => settings.EnableOrderStatus,
            NotificationType.OpenTicket => settings.EnableUpcomingShowtime,
            NotificationType.OpenVoucherList => settings.EnablePromotion,
            NotificationType.OpenMovieDetail => settings.EnablePromotion,
            NotificationType.OpenNotificationCenter => settings.EnableSystem,
            _ => true
        };
    }

    private static NotificationResponseDto MapToResponseDto(Notification notification)
    {
        object? parsedData = null;
        if (!string.IsNullOrEmpty(notification.Data))
        {
            try
            {
                parsedData = JsonSerializer.Deserialize<object>(notification.Data);
            }
            catch
            {
                parsedData = notification.Data;
            }
        }

        return new NotificationResponseDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type.ToString(),
            Title = notification.Title,
            Message = notification.Message,
            Data = parsedData,
            IsRead = notification.IsRead,
            ImageUrl = notification.ImageUrl,
            ActionType = notification.ActionType?.ToString(),
            ActionData = notification.ActionData,
            Priority = (int)notification.Priority,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        };
    }

    #endregion

    #region LEVEL 3: Statistics & Monitoring

    /// <summary>
    /// Get notification statistics for a user
    /// </summary>
    public async Task<NotificationStatsDto> GetUserStatsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var allNotifications = await _notificationRepository.GetUserNotificationsAsync(
            userId, 1000, false, cancellationToken);

        var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);

        var stats = new NotificationStatsDto
        {
            TotalNotifications = allNotifications.Count,
            UnreadCount = unreadCount,
            ReadCount = allNotifications.Count - unreadCount,
            TodayCount = allNotifications.Count(n => n.CreatedAt.Date == DateTime.UtcNow.Date),
            ThisWeekCount = allNotifications.Count(n => n.CreatedAt >= DateTime.UtcNow.AddDays(-7)),
            ByType = allNotifications
                .GroupBy(n => n.Type)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            ByPriority = allNotifications
                .GroupBy(n => n.Priority)
                .ToDictionary(g => g.Key.ToString(), g => g.Count())
        };

        return stats;
    }

    #endregion
}
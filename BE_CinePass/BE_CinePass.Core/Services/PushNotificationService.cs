using BE_CinePass.Core.Configurations;
using BE_CinePass.Core.Repositories;
using BE_CinePass.Domain.Models;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.Services;

/// <summary>
/// High-level service for managing push notifications
/// Wraps FirebaseMessagingService with business logic
/// </summary>
public class PushNotificationService
{
    private readonly FirebaseMessagingService _fcmService;
    private readonly DeviceTokenRepository _deviceTokenRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(
        FirebaseMessagingService fcmService,
        DeviceTokenRepository deviceTokenRepository,
        ApplicationDbContext context,
        ILogger<PushNotificationService> logger)
    {
        _fcmService = fcmService;
        _deviceTokenRepository = deviceTokenRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Send push notification to a specific user (all their devices)
    /// </summary>
    public async Task<(bool Success, int SentCount, int FailedCount)> SendToUserAsync(
        Guid userId,
        string title,
        string message,
        Dictionary<string, string>? data = null,
        string? imageUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get user's active device tokens
            var deviceTokens = await _deviceTokenRepository.GetActiveByUserIdAsync(userId, cancellationToken);

            if (!deviceTokens.Any())
            {
                _logger.LogWarning("No active device tokens found for user {UserId}", userId);
                return (false, 0, 0);
            }

            // Send to all user's devices
            var tokens = deviceTokens.Select(dt => dt.Token).ToList();
            var (successCount, failureCount, invalidTokens) = 
                await _fcmService.SendToDevicesAsync(tokens, title, message, data, imageUrl);

            // Handle invalid tokens
            if (invalidTokens.Any())
            {
                await DeactivateInvalidTokensAsync(invalidTokens, userId, cancellationToken);
            }

            // Update last_used_at for successful sends
            await UpdateLastUsedForSuccessfulTokensAsync(deviceTokens, invalidTokens, cancellationToken);

            _logger.LogInformation(
                "Push sent to user {UserId}. Success: {Success}/{Total}, Failed: {Failed}",
                userId, successCount, tokens.Count, failureCount);

            return (successCount > 0, successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
            return (false, 0, 0);
        }
    }

    /// <summary>
    /// Send push notification to multiple users
    /// </summary>
    public async Task<(int SuccessCount, int FailedCount)> SendToUsersAsync(
        List<Guid> userIds,
        string title,
        string message,
        Dictionary<string, string>? data = null,
        string? imageUrl = null,
        CancellationToken cancellationToken = default)
    {
        int totalSuccess = 0;
        int totalFailed = 0;

        foreach (var userId in userIds)
        {
            var (success, sentCount, failedCount) = await SendToUserAsync(
                userId, title, message, data, imageUrl, cancellationToken);

            totalSuccess += sentCount;
            totalFailed += failedCount;
        }

        return (totalSuccess, totalFailed);
    }

    /// <summary>
    /// Broadcast push notification to all active users
    /// </summary>
    public async Task<(int SuccessCount, int FailedCount)> BroadcastAsync(
        string title,
        string message,
        Dictionary<string, string>? data = null,
        string? imageUrl = null,
        string? platform = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all active device tokens (optionally filtered by platform)
            var deviceTokens = string.IsNullOrEmpty(platform)
                ? await _deviceTokenRepository.GetAllActiveAsync(cancellationToken)
                : await _deviceTokenRepository.GetActiveByPlatformAsync(platform, cancellationToken);

            if (!deviceTokens.Any())
            {
                _logger.LogWarning("No active device tokens found for broadcast");
                return (0, 0);
            }

            var tokens = deviceTokens.Select(dt => dt.Token).ToList();
            var (successCount, failureCount, invalidTokens) = 
                await _fcmService.SendToDevicesAsync(tokens, title, message, data, imageUrl);

            // Handle invalid tokens
            if (invalidTokens.Any())
            {
                await DeactivateInvalidTokensByTokenStringAsync(invalidTokens, cancellationToken);
            }

            _logger.LogInformation(
                "Broadcast sent. Success: {Success}/{Total}, Failed: {Failed}, Platform: {Platform}",
                successCount, tokens.Count, failureCount, platform ?? "all");

            return (successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting push notification");
            return (0, 0);
        }
    }

    /// <summary>
    /// Send push notification to specific platform
    /// </summary>
    public async Task<(int SuccessCount, int FailedCount)> SendToPlatformAsync(
        string platform,
        string title,
        string message,
        Dictionary<string, string>? data = null,
        string? imageUrl = null,
        CancellationToken cancellationToken = default)
    {
        return await BroadcastAsync(title, message, data, imageUrl, platform, cancellationToken);
    }

    /// <summary>
    /// Send silent/data-only notification to a user
    /// </summary>
    public async Task<bool> SendDataOnlyToUserAsync(
        Guid userId,
        Dictionary<string, string> data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deviceTokens = await _deviceTokenRepository.GetActiveByUserIdAsync(userId, cancellationToken);

            if (!deviceTokens.Any())
            {
                return false;
            }

            bool anySuccess = false;

            foreach (var deviceToken in deviceTokens)
            {
                var (success, _, error) = await _fcmService.SendDataOnlyAsync(
                    deviceToken.Token, data, deviceToken.Platform);

                if (success)
                {
                    anySuccess = true;
                    await _deviceTokenRepository.UpdateLastUsedAsync(deviceToken.Id, cancellationToken);
                }
                else if (error == "INVALID_TOKEN")
                {
                    await _deviceTokenRepository.DeactivateAsync(deviceToken.Id, cancellationToken);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return anySuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending silent notification to user {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// Subscribe user's devices to a topic
    /// </summary>
    public async Task<bool> SubscribeUserToTopicAsync(
        Guid userId,
        string topic,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deviceTokens = await _deviceTokenRepository.GetActiveByUserIdAsync(userId, cancellationToken);

            if (!deviceTokens.Any())
            {
                return false;
            }

            var tokens = deviceTokens.Select(dt => dt.Token).ToList();
            var success = await _fcmService.SubscribeToTopicAsync(tokens, topic);

            _logger.LogInformation(
                "User {UserId} subscribed to topic '{Topic}'. Success: {Success}",
                userId, topic, success);

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing user {UserId} to topic {Topic}", userId, topic);
            return false;
        }
    }

    /// <summary>
    /// Unsubscribe user's devices from a topic
    /// </summary>
    public async Task<bool> UnsubscribeUserFromTopicAsync(
        Guid userId,
        string topic,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var deviceTokens = await _deviceTokenRepository.GetActiveByUserIdAsync(userId, cancellationToken);

            if (!deviceTokens.Any())
            {
                return false;
            }

            var tokens = deviceTokens.Select(dt => dt.Token).ToList();
            var success = await _fcmService.UnsubscribeFromTopicAsync(tokens, topic);

            _logger.LogInformation(
                "User {UserId} unsubscribed from topic '{Topic}'. Success: {Success}",
                userId, topic, success);

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing user {UserId} from topic {Topic}", userId, topic);
            return false;
        }
    }

    /// <summary>
    /// Send push notification to a topic
    /// </summary>
    public async Task<bool> SendToTopicAsync(
        string topic,
        string title,
        string message,
        Dictionary<string, string>? data = null)
    {
        try
        {
            var (success, messageId) = await _fcmService.SendToTopicAsync(topic, title, message, data);

            _logger.LogInformation(
                "Push sent to topic '{Topic}'. Success: {Success}, MessageId: {MessageId}",
                topic, success, messageId);

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push to topic {Topic}", topic);
            return false;
        }
    }

    /// <summary>
    /// Deactivate invalid tokens
    /// </summary>
    private async Task DeactivateInvalidTokensAsync(
        List<string> invalidTokens,
        Guid userId,
        CancellationToken cancellationToken)
    {
        foreach (var token in invalidTokens)
        {
            await _deviceTokenRepository.DeactivateByTokenAsync(token, userId, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Deactivated {Count} invalid token(s) for user {UserId}",
            invalidTokens.Count, userId);
    }

    /// <summary>
    /// Deactivate invalid tokens (without user context)
    /// </summary>
    private async Task DeactivateInvalidTokensByTokenStringAsync(
        List<string> invalidTokens,
        CancellationToken cancellationToken)
    {
        foreach (var tokenString in invalidTokens)
        {
            var token = await _deviceTokenRepository.FindByTokenAsync(tokenString, cancellationToken);
            if (token != null)
            {
                await _deviceTokenRepository.DeactivateAsync(token.Id, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Deactivated {Count} invalid token(s) from broadcast",
            invalidTokens.Count);
    }

    /// <summary>
    /// Update last_used_at for successful token sends
    /// </summary>
    private async Task UpdateLastUsedForSuccessfulTokensAsync(
        List<DeviceToken> deviceTokens,
        List<string> invalidTokens,
        CancellationToken cancellationToken)
    {
        var successfulTokens = deviceTokens
            .Where(dt => !invalidTokens.Contains(dt.Token))
            .ToList();

        foreach (var token in successfulTokens)
        {
            await _deviceTokenRepository.UpdateLastUsedAsync(token.Id, cancellationToken);
        }

        if (successfulTokens.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Clean up old inactive tokens
    /// </summary>
    public async Task<int> CleanupInactiveTokensAsync(int daysOld = 30)
    {
        try
        {
            var count = await _deviceTokenRepository.DeleteInactiveOlderThanAsync(daysOld);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} inactive tokens older than {Days} days", count, daysOld);

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up inactive tokens");
            return 0;
        }
    }
}
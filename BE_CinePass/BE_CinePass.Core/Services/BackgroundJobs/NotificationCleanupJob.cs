using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.Services.BackgroundJobs;

public class NotificationCleanupJob
{
    private readonly NotificationService _notificationService;
    private readonly ILogger<NotificationCleanupJob> _logger;

    public NotificationCleanupJob(
        NotificationService notificationService,
        ILogger<NotificationCleanupJob> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Delete expired notifications
    /// </summary>
    public async Task CleanupExpiredNotificationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting notification cleanup job");

        try
        {
            var deletedCount = await _notificationService.DeleteExpiredNotificationsAsync(cancellationToken);
            
            _logger.LogInformation(
                "Completed notification cleanup job, deleted {Count} expired notifications", 
                deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in notification cleanup job");
            throw;
        }
    }
}
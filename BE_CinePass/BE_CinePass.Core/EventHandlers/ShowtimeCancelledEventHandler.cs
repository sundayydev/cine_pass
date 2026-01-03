using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class ShowtimeCancelledEventHandler : IEventHandler<ShowtimeCancelledEvent>
{
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<ShowtimeCancelledEventHandler> _logger;

    public ShowtimeCancelledEventHandler(
        NotificationHelperService notificationHelper,
        ILogger<ShowtimeCancelledEventHandler> logger)
    {
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    public async Task HandleAsync(ShowtimeCancelledEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling ShowtimeCancelledEvent for Showtime {ShowtimeId} - notifying {Count} users", 
            @event.ShowtimeId,
            @event.AffectedUserIds.Count);

        // Send notification to all affected users
        foreach (var userId in @event.AffectedUserIds)
        {
            await _notificationHelper.SendShowtimeCancelledNotificationAsync(
                userId,
                @event.MovieTitle,
                @event.CinemaName,
                @event.StartTime,
                cancellationToken);
        }
    }
}
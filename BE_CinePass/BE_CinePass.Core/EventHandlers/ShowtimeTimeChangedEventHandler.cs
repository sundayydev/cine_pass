using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class ShowtimeTimeChangedEventHandler : IEventHandler<ShowtimeTimeChangedEvent>
{
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<ShowtimeTimeChangedEventHandler> _logger;

    public ShowtimeTimeChangedEventHandler(
        NotificationHelperService notificationHelper,
        ILogger<ShowtimeTimeChangedEventHandler> logger)
    {
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    public async Task HandleAsync(ShowtimeTimeChangedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling ShowtimeTimeChangedEvent for Showtime {ShowtimeId} - notifying {Count} users", 
            @event.ShowtimeId,
            @event.AffectedUserIds.Count);

        // Send notification to all affected users
        foreach (var userId in @event.AffectedUserIds)
        {
            await _notificationHelper.SendShowtimeTimeChangedNotificationAsync(
                userId,
                @event.MovieTitle,
                @event.CinemaName,
                @event.OldStartTime,
                @event.NewStartTime,
                cancellationToken);
        }
    }
}
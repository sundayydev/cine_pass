using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class PointsEarnedEventHandler : IEventHandler<PointsEarnedEvent>
{
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<PointsEarnedEventHandler> _logger;

    public PointsEarnedEventHandler(
        NotificationHelperService notificationHelper,
        ILogger<PointsEarnedEventHandler> logger)
    {
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    public async Task HandleAsync(PointsEarnedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling PointsEarnedEvent for User {UserId}, Points {Points}", 
            @event.UserId,
            @event.Points);

        await _notificationHelper.SendPointsEarnedNotificationAsync(
            @event.UserId,
            @event.PointsEarned,
            @event.Source,
            cancellationToken);
    }
}

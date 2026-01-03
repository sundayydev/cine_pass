using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class SecurityAlertEventHandler : IEventHandler<SecurityAlertEvent>
{
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<SecurityAlertEventHandler> _logger;

    public SecurityAlertEventHandler(
        NotificationHelperService notificationHelper,
        ILogger<SecurityAlertEventHandler> logger)
    {
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    public async Task HandleAsync(SecurityAlertEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling SecurityAlertEvent for User {UserId}, Type {AlertType}", 
            @event.UserId,
            @event.AlertType);

        await _notificationHelper.SendSecurityAlertNotificationAsync(
            @event.UserId,
            @event.AlertType,
            @event.Description,
            @event.IpAddress,
            @event.Location,
            cancellationToken);
    }
}
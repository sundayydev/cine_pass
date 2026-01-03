using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class SystemMaintenanceEventHandler : IEventHandler<SystemMaintenanceEvent>
{
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<SystemMaintenanceEventHandler> _logger;

    public SystemMaintenanceEventHandler(
        NotificationHelperService notificationHelper,
        ILogger<SystemMaintenanceEventHandler> logger)
    {
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    public async Task HandleAsync(SystemMaintenanceEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling SystemMaintenanceEvent from {Start} to {End}", 
            @event.MaintenanceStart,
            @event.MaintenanceEnd);

        await _notificationHelper.SendSystemMaintenanceNotificationAsync(
            @event.MaintenanceStart,
            @event.MaintenanceEnd,
            cancellationToken);
    }
}
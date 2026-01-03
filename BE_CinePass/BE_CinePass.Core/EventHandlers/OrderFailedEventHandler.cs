using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class OrderFailedEventHandler : IEventHandler<OrderFailedEvent>
{
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<OrderFailedEventHandler> _logger;

    public OrderFailedEventHandler(
        NotificationHelperService notificationHelper,
        ILogger<OrderFailedEventHandler> logger)
    {
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    public async Task HandleAsync(OrderFailedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling OrderFailedEvent for Order {OrderId}", 
            @event.OrderId);

        await _notificationHelper.SendOrderFailedNotificationAsync(
            @event.UserId,
            @event.OrderId,
            @event.OrderCode,
            @event.Reason,
            cancellationToken);
    }
}
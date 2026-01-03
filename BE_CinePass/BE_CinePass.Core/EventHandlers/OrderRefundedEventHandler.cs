using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class OrderRefundedEventHandler : IEventHandler<OrderRefundedEvent>
{
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<OrderRefundedEventHandler> _logger;

    public OrderRefundedEventHandler(
        NotificationHelperService notificationHelper,
        ILogger<OrderRefundedEventHandler> logger)
    {
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    public async Task HandleAsync(OrderRefundedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling OrderRefundedEvent for Order {OrderId}", 
            @event.OrderId);

        await _notificationHelper.SendOrderRefundedNotificationAsync(
            @event.UserId,
            @event.OrderId,
            @event.OrderCode,
            @event.RefundAmount,
            @event.Reason,
            cancellationToken);
    }
}
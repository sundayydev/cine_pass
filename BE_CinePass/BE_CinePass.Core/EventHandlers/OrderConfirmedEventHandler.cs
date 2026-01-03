using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class OrderConfirmedEventHandler : IEventHandler<OrderConfirmedEvent>
{
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<OrderConfirmedEventHandler> _logger;

    public OrderConfirmedEventHandler(
        NotificationHelperService notificationHelper,
        ILogger<OrderConfirmedEventHandler> logger)
    {
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    public async Task HandleAsync(OrderConfirmedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling OrderConfirmedEvent for Order {OrderId}", 
            @event.OrderId);

        await _notificationHelper.SendOrderConfirmedNotificationAsync(
            @event.UserId,
            @event.OrderId,
            @event.OrderCode,
            @event.TotalAmount,
            cancellationToken);
    }
}
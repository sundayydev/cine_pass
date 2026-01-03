using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class PaymentFailedEventHandler : IEventHandler<PaymentFailedEvent>
{
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<PaymentFailedEventHandler> _logger;

    public PaymentFailedEventHandler(
        NotificationHelperService notificationHelper,
        ILogger<PaymentFailedEventHandler> logger)
    {
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    public async Task HandleAsync(PaymentFailedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling PaymentFailedEvent for Order {OrderId}", 
            @event.OrderId);

        await _notificationHelper.SendPaymentFailedNotificationAsync(
            @event.UserId,
            @event.OrderId,
            @event.OrderCode,
            @event.Reason,
            cancellationToken);
    }
}
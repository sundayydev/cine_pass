using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class PaymentSuccessEventHandler : IEventHandler<PaymentSuccessEvent>
{
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<PaymentSuccessEventHandler> _logger;

    public PaymentSuccessEventHandler(
        NotificationHelperService notificationHelper,
        ILogger<PaymentSuccessEventHandler> logger)
    {
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    public async Task HandleAsync(PaymentSuccessEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling PaymentSuccessEvent for Order {OrderId}", 
            @event.OrderId);

        await _notificationHelper.SendPaymentSuccessNotificationAsync(
            @event.UserId,
            @event.OrderId,
            @event.OrderCode,
            @event.Amount,
            @event.PaymentMethod,
            cancellationToken);
    }
}
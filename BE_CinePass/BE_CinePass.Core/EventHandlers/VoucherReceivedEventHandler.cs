using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class VoucherReceivedEventHandler : IEventHandler<VoucherReceivedEvent>
{
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<VoucherReceivedEventHandler> _logger;

    public VoucherReceivedEventHandler(
        NotificationHelperService notificationHelper,
        ILogger<VoucherReceivedEventHandler> logger)
    {
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    public async Task HandleAsync(VoucherReceivedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling VoucherReceivedEvent for User {UserId}, Voucher {VoucherCode}", 
            @event.UserId,
            @event.VoucherCode);

        await _notificationHelper.SendVoucherReceivedNotificationAsync(
            @event.UserId,
            @event.VoucherCode,
            @event.VoucherName,
            @event.ExpiresAt,
            cancellationToken);
    }
}
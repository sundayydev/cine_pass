using BE_CinePass.Core.Services;
using BE_CinePass.Domain.Events;
using Microsoft.Extensions.Logging;

namespace BE_CinePass.Core.EventHandlers;

public class VoucherExpiringSoonEventHandler : IEventHandler<VoucherExpiringSoonEvent>
{
    private readonly NotificationHelperService _notificationHelper;
    private readonly ILogger<VoucherExpiringSoonEventHandler> _logger;

    public VoucherExpiringSoonEventHandler(
        NotificationHelperService notificationHelper,
        ILogger<VoucherExpiringSoonEventHandler> logger)
    {
        _notificationHelper = notificationHelper;
        _logger = logger;
    }

    public async Task HandleAsync(VoucherExpiringSoonEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Handling VoucherExpiringSoonEvent for User {UserId}, Voucher {VoucherCode}", 
            @event.UserId,
            @event.VoucherCode);

        await _notificationHelper.SendVoucherExpiringSoonNotificationAsync(
            @event.UserId,
            @event.VoucherCode,
            @event.VoucherName,
            @event.ExpiresAt,
            @event.DaysUntilExpiry,
            cancellationToken);
    }
}
